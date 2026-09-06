using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Leb.Core.Entities;
using Leb.Core.DTOs;
using Leb.Core.Interfaces;

namespace Leb.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IPdfService _pdfService;
        private readonly IEmailService _emailService;

        public AppointmentController(
            IAppointmentRepository appointmentRepository,
            IReportRepository reportRepository,
            IBranchRepository branchRepository,
            IInvoiceRepository invoiceRepository,
            IPaymentRepository paymentRepository,
            IFeedbackRepository feedbackRepository,
            INotificationRepository notificationRepository,
            IPdfService pdfService,
            IEmailService emailService)
        {
            _appointmentRepository = appointmentRepository;
            _reportRepository = reportRepository;
            _branchRepository = branchRepository;
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _feedbackRepository = feedbackRepository;
            _notificationRepository = notificationRepository;
            _pdfService = pdfService;
            _emailService = emailService;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto dto)
        {
            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                BranchId = dto.BranchId,
                AppointmentDate = dto.AppointmentDate,
                SlotId = dto.SlotId,
                TotalAmount = dto.TotalAmount,
                DiscountAmount = dto.DiscountAmount,
                PaidAmount = dto.PaidAmount,
                PaymentStatus = dto.PaymentStatus,
                Notes = dto.Notes,
                DoctorId = dto.DoctorId
            };

            int appointmentId = await _appointmentRepository.CreateAppointmentAsync(appointment, dto.TestIds, dto.PackageIds);

            // Fetch appointment details to get joins
            var bookedApp = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
            if (bookedApp == null) return BadRequest(new { Message = "Booking failed." });

            // 1. Create Invoice
            decimal subtotal = bookedApp.TotalAmount - bookedApp.DiscountAmount;
            decimal tax = subtotal * 0.18m; // 18% GST
            decimal finalAmount = subtotal + tax;

            var invoice = new Invoice
            {
                AppointmentId = appointmentId,
                InvoiceNumber = $"INV-{DateTime.UtcNow.ToString("yyyyMMdd")}-{appointmentId}",
                TotalAmount = bookedApp.TotalAmount,
                DiscountAmount = bookedApp.DiscountAmount,
                TaxAmount = tax,
                FinalAmount = finalAmount
            };

            await _invoiceRepository.CreateInvoiceAsync(invoice);

            // 2. Pre-schedule empty test reports (Pending status)
            // Populate individual reports for each test selected directly or through packages
            var allTests = new List<Test>();
            allTests.AddRange(bookedApp.Tests);
            foreach (var pkg in bookedApp.Packages)
            {
                // Note: Packages tests were fetched inside GetAppointmentById.
                allTests.AddRange(pkg.Tests);
            }

            // Remove duplicate tests
            var uniqueTestIds = new HashSet<int>();
            foreach (var test in allTests)
            {
                if (uniqueTestIds.Add(test.TestId))
                {
                    await _reportRepository.CreateReportAsync(new Report
                    {
                        AppointmentId = appointmentId,
                        TestId = test.TestId,
                        Status = "Pending"
                    });
                }
            }

            // 3. Create Notification
            await _notificationRepository.CreateNotificationAsync(new Notification
            {
                UserId = bookedApp.PatientId, // In our seed Patients matches Users. We will handle user notifications
                Message = $"Appointment scheduled successfully for {bookedApp.AppointmentDate.ToString("yyyy-MM-dd")} at {bookedApp.StartTime}.",
                Type = "Booking"
            });

            // 4. Send Mock Confirmation Email
            await _emailService.SendEmailAsync(bookedApp.PatientEmail, "Appointment Booking Confirmed",
                $"Hello {bookedApp.PatientFirstName},\n\nYour appointment is confirmed for {bookedApp.AppointmentDate.ToString("yyyy-MM-dd")} at {bookedApp.StartTime}.\nInvoice Number: {invoice.InvoiceNumber}");

            return Ok(new { Message = "Appointment booked successfully.", AppointmentId = appointmentId, InvoiceNumber = invoice.InvoiceNumber });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null) return NotFound(new { Message = "Appointment not found." });
            return Ok(appointment);
        }

        [HttpGet("patient/{id}")]
        public async Task<IActionResult> GetPatientAppointments(int id)
        {
            var list = await _appointmentRepository.GetAppointmentsByPatientAsync(id);
            return Ok(list);
        }

        [HttpGet("branch/{id}")]
        public async Task<IActionResult> GetBranchAppointments(int id)
        {
            var list = await _appointmentRepository.GetAppointmentsByBranchAsync(id);
            return Ok(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAppointments()
        {
            var list = await _appointmentRepository.GetAllAppointmentsAsync();
            return Ok(list);
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] AppointmentStatusUpdateDto dto)
        {
            await _appointmentRepository.UpdateAppointmentStatusAsync(dto.AppointmentId, dto.Status, dto.StaffId);
            return Ok(new { Message = $"Appointment status updated to '{dto.Status}'." });
        }

        [HttpPut("reschedule")]
        public async Task<IActionResult> Reschedule([FromBody] RescheduleDto dto)
        {
            await _appointmentRepository.RescheduleAppointmentAsync(dto.AppointmentId, dto.AppointmentDate, dto.SlotId);
            return Ok(new { Message = "Appointment rescheduled successfully." });
        }

        [HttpDelete("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            await _appointmentRepository.CancelAppointmentAsync(id);
            return Ok(new { Message = "Appointment cancelled." });
        }

        // --- PAYMENT TRANSACTIONS SIMULATION ---
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> ProcessPayment(int id, [FromBody] Payment payment)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null) return NotFound(new { Message = "Appointment not found." });

            payment.AppointmentId = id;
            payment.PaymentStatus = "Success";
            payment.TransactionId = payment.TransactionId ?? $"TXN-{Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()}";

            await _paymentRepository.CreatePaymentAsync(payment);

            // Update appointment paid status
            await _appointmentRepository.UpdateAppointmentPaymentStatusAsync(id, "Paid", payment.Amount);

            // Send notification
            await _notificationRepository.CreateNotificationAsync(new Notification
            {
                UserId = appointment.PatientId,
                Message = $"Payment of INR {payment.Amount} received for Appointment ID {id}.",
                Type = "Payment"
            });

            return Ok(new { Message = "Simulated payment processed successfully.", TransactionId = payment.TransactionId });
        }

        // --- INVOICES & REPORTS DOC GENERATION ---
        [HttpGet("{id}/invoice-pdf")]
        public async Task<IActionResult> DownloadInvoicePdf(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null) return NotFound(new { Message = "Appointment not found." });

            var invoice = await _invoiceRepository.GetInvoiceByAppointmentAsync(id);
            if (invoice == null) return NotFound(new { Message = "Invoice details not found." });

            var pdfBytes = _pdfService.GenerateInvoicePdf(appointment, invoice);
            return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
        }

        [HttpGet("{id}/report-pdf")]
        public async Task<IActionResult> DownloadReportPdf(int id, [FromQuery] bool preprinted = false)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null) return NotFound(new { Message = "Appointment not found." });

            var reports = await _reportRepository.GetReportsByAppointmentAsync(id);
            var branch = await _branchRepository.GetBranchByIdAsync(appointment.BranchId);

            var pdfBytes = _pdfService.GenerateReportPdf(appointment, reports, branch, preprinted);
            return File(pdfBytes, "application/pdf", $"LabReport-{appointment.AppointmentId}.pdf");
        }

        [HttpPost("{id}/mark-printed")]
        public async Task<IActionResult> MarkPrinted(int id, [FromBody] MarkPrintedDto dto)
        {
            await _appointmentRepository.LogPrintAuditAsync(id, dto.StaffId);
            return Ok(new { Message = "Print audit logged successfully." });
        }

        // --- RESULTS UPLOADS & PARAMETERS ---
        [HttpGet("{id}/reports")]
        public async Task<IActionResult> GetReports(int id)
        {
            var reports = await _reportRepository.GetReportsByAppointmentAsync(id);
            return Ok(reports);
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpPost("reports/upload")]
        public async Task<IActionResult> UploadReport([FromBody] UploadResultDto dto)
        {
            var report = new Report
            {
                AppointmentId = dto.AppointmentId,
                TestId = dto.TestId,
                ResultValue = dto.ResultValue,
                Remarks = dto.Remarks,
                UploadedByStaffId = dto.StaffId
            };

            int reportId = await _reportRepository.CreateReportAsync(report);

            // Verify if appointment was marked completed inside sp_CreateReport
            var app = await _appointmentRepository.GetAppointmentByIdAsync(dto.AppointmentId);
            if (app != null && app.Status == "ResultUploaded")
            {
                // Send Notification to patient that report is ready
                await _notificationRepository.CreateNotificationAsync(new Notification
                {
                    UserId = app.PatientId,
                    Message = $"Your diagnostic reports for Appointment ID {dto.AppointmentId} are ready for download.",
                    Type = "Report"
                });
                
                await _emailService.SendEmailAsync(app.PatientEmail, "Diagnostic Lab Report Ready",
                    $"Hello {app.PatientFirstName},\n\nYour laboratory reports are ready. Please log in to your patient dashboard to download them.");
            }

            return Ok(new { Message = "Test result uploaded successfully.", ReportId = reportId });
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpPut("reports/outsource-dispatch")]
        public async Task<IActionResult> OutsourceDispatch([FromBody] OutsourceDispatchDto dto)
        {
            await _reportRepository.UpdateReportOutsourceAsync(dto.ReportId, dto.IsOutsourced, dto.ExternalLabName, dto.ExternalBarcode);
            return Ok(new { Message = "Outsourced dispatch updated successfully." });
        }

        [HttpPost("feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] Feedback feedback)
        {
            int id = await _feedbackRepository.CreateFeedbackAsync(feedback);
            return Ok(new { Message = "Feedback submitted successfully. Thank you for your response!", FeedbackId = id });
        }
    }
}
