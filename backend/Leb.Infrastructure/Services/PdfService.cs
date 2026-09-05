using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Leb.Core.Entities;
using Leb.Core.Interfaces;

namespace Leb.Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateReportPdf(Appointment appointment, IEnumerable<Report> reports)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;

            // Fonts
            var fontTitle = new XFont("Arial", 20, XFontStyleEx.Bold);
            var fontSubtitle = new XFont("Arial", 12, XFontStyleEx.Bold);
            var fontLabel = new XFont("Arial", 10, XFontStyleEx.Bold);
            var fontValue = new XFont("Arial", 10, XFontStyleEx.Regular);
            var fontTableHeader = new XFont("Arial", 10, XFontStyleEx.Bold);
            var fontTableBody = new XFont("Arial", 9, XFontStyleEx.Regular);
            var fontWatermark = new XFont("Arial", 8, XFontStyleEx.Italic);

            // Draw Header Banner
            var rectHeader = new XRect(20, 20, pageWidth - 40, 60);
            gfx.DrawRectangle(XBrushes.RoyalBlue, rectHeader);
            
            gfx.DrawString("NEO LEBORETORY", fontTitle, XBrushes.White, 35, 55);
            gfx.DrawString("Precision Health & Diagnostics", fontWatermark, XBrushes.LightSkyBlue, 35, 72);

            // Draw Laboratory Contact Details
            double yPos = 95;
            gfx.DrawString($"Branch: {appointment.BranchName}", fontValue, XBrushes.Black, 20, yPos);
            gfx.DrawString($"Address: {appointment.BranchAddress}, {appointment.BranchCity}", fontValue, XBrushes.Black, 20, yPos + 15);
            gfx.DrawString($"Phone: {appointment.PatientPhone}", fontValue, XBrushes.Black, 20, yPos + 30);

            gfx.DrawString($"Date: {appointment.AppointmentDate.ToString("yyyy-MM-dd")}", fontLabel, XBrushes.Black, pageWidth - 180, yPos);
            gfx.DrawString($"Status: {appointment.Status}", fontLabel, XBrushes.Red, pageWidth - 180, yPos + 15);
            gfx.DrawString($"Report ID: REP-{appointment.AppointmentId}", fontLabel, XBrushes.Black, pageWidth - 180, yPos + 30);

            // Separator Line
            gfx.DrawLine(XPens.DarkGray, 20, 140, pageWidth - 20, 140);

            // Patient Information
            gfx.DrawString("PATIENT DEMOGRAPHICS", fontSubtitle, XBrushes.RoyalBlue, 20, 160);
            
            yPos = 180;
            gfx.DrawString("Name:", fontLabel, XBrushes.Black, 20, yPos);
            gfx.DrawString($"{appointment.PatientFirstName} {appointment.PatientLastName}", fontValue, XBrushes.Black, 80, yPos);

            gfx.DrawString("Age / Gender:", fontLabel, XBrushes.Black, 20, yPos + 15);
            int age = DateTime.Today.Year - appointment.DateOfBirth.Year;
            if (appointment.DateOfBirth > DateTime.Today.AddYears(-age)) age--;
            gfx.DrawString($"{age} Years / {appointment.Gender}", fontValue, XBrushes.Black, 110, yPos + 15);

            gfx.DrawString("Patient Phone:", fontLabel, XBrushes.Black, pageWidth / 2, yPos);
            gfx.DrawString(appointment.PatientPhone, fontValue, XBrushes.Black, (pageWidth / 2) + 100, yPos);

            gfx.DrawString("Ref. Doctor:", fontLabel, XBrushes.Black, pageWidth / 2, yPos + 15);
            string docName = !string.IsNullOrEmpty(appointment.DoctorFirstName) ? $"Dr. {appointment.DoctorFirstName} {appointment.DoctorLastName}" : "Self Referrer";
            gfx.DrawString(docName, fontValue, XBrushes.Black, (pageWidth / 2) + 100, yPos + 15);

            // Separator Line
            gfx.DrawLine(XPens.DarkGray, 20, 210, pageWidth - 20, 210);

            // Test Results Header
            gfx.DrawString("DIAGNOSTIC TEST RESULTS", fontSubtitle, XBrushes.RoyalBlue, 20, 230);

            // Draw Table Headers
            double xCode = 20;
            double xName = 100;
            double xSample = 280;
            double xValue = 380;
            double xNormal = 470;
            
            yPos = 255;
            gfx.DrawRectangle(XBrushes.LightGray, new XRect(20, yPos, pageWidth - 40, 20));
            gfx.DrawString("CODE", fontTableHeader, XBrushes.Black, xCode + 5, yPos + 14);
            gfx.DrawString("TEST NAME", fontTableHeader, XBrushes.Black, xName + 5, yPos + 14);
            gfx.DrawString("SAMPLE TYPE", fontTableHeader, XBrushes.Black, xSample + 5, yPos + 14);
            gfx.DrawString("OBSERVED VALUE", fontTableHeader, XBrushes.Black, xValue + 5, yPos + 14);
            gfx.DrawString("REFERENCE RANGE", fontTableHeader, XBrushes.Black, xNormal + 5, yPos + 14);

            yPos = 280;
            foreach (var rep in reports)
            {
                gfx.DrawString(rep.TestCode, fontTableBody, XBrushes.Black, xCode + 5, yPos + 12);
                gfx.DrawString(rep.TestName, fontTableBody, XBrushes.Black, xName + 5, yPos + 12);
                gfx.DrawString(rep.SampleType, fontTableBody, XBrushes.Black, xSample + 5, yPos + 12);
                
                string res = rep.ResultValue ?? "Pending";
                var valColor = res == "Pending" ? XBrushes.Red : XBrushes.DarkGreen;
                gfx.DrawString(res, fontLabel, valColor, xValue + 5, yPos + 12);
                
                gfx.DrawString(rep.NormalRange ?? "N/A", fontTableBody, XBrushes.Black, xNormal + 5, yPos + 12);

                if (!string.IsNullOrEmpty(rep.Remarks))
                {
                    yPos += 15;
                    gfx.DrawString($"Remarks: {rep.Remarks}", fontWatermark, XBrushes.Gray, xName + 5, yPos + 10);
                }

                yPos += 25;
                gfx.DrawLine(XPens.LightGray, 20, yPos - 5, pageWidth - 20, yPos - 5);
            }

            // Draw QR Code Simulator block at the bottom
            double qrY = pageHeight - 120;
            gfx.DrawRectangle(XBrushes.WhiteSmoke, new XRect(20, qrY, pageWidth - 40, 80));
            gfx.DrawRectangle(XPens.DarkGray, new XRect(20, qrY, pageWidth - 40, 80));

            // Simulated QR Grid
            double qrX = 35;
            gfx.DrawRectangle(XPens.Black, XBrushes.Black, new XRect(qrX, qrY + 10, 60, 60));
            gfx.DrawRectangle(XPens.White, XBrushes.White, new XRect(qrX + 10, qrY + 20, 40, 40));
            gfx.DrawRectangle(XPens.Black, XBrushes.Black, new XRect(qrX + 20, qrY + 30, 20, 20));

            gfx.DrawString("ONLINE REPORT VERIFICATION", fontSubtitle, XBrushes.RoyalBlue, qrX + 80, qrY + 25);
            gfx.DrawString("This report is digitally signed and secure.", fontValue, XBrushes.Black, qrX + 80, qrY + 42);
            gfx.DrawString($"Scan OR visit: https://neoleboretory.lab/verify/REP-{appointment.AppointmentId}", fontWatermark, XBrushes.RoyalBlue, qrX + 80, qrY + 58);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        public byte[] GenerateInvoicePdf(Appointment appointment, Invoice invoice)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;

            // Fonts
            var fontTitle = new XFont("Arial", 22, XFontStyleEx.Bold);
            var fontSubtitle = new XFont("Arial", 12, XFontStyleEx.Bold);
            var fontLabel = new XFont("Arial", 10, XFontStyleEx.Bold);
            var fontValue = new XFont("Arial", 10, XFontStyleEx.Regular);
            var fontTableHeader = new XFont("Arial", 10, XFontStyleEx.Bold);
            var fontTableBody = new XFont("Arial", 9, XFontStyleEx.Regular);

            // Draw Header Banner
            var rectHeader = new XRect(20, 20, pageWidth - 40, 60);
            gfx.DrawRectangle(XBrushes.RoyalBlue, rectHeader);
            gfx.DrawString("LABORATORY RECEIPT / INVOICE", fontTitle, XBrushes.White, 35, 57);

            // Invoice Metadata
            double yPos = 95;
            gfx.DrawString($"Invoice No: {invoice.InvoiceNumber}", fontLabel, XBrushes.Black, 20, yPos);
            gfx.DrawString($"Date Generated: {invoice.GeneratedAt.ToString("yyyy-MM-dd HH:mm")}", fontValue, XBrushes.Black, 20, yPos + 15);
            gfx.DrawString($"Appointment ID: {appointment.AppointmentId}", fontValue, XBrushes.Black, 20, yPos + 30);

            gfx.DrawString($"Patient: {appointment.PatientFirstName} {appointment.PatientLastName}", fontLabel, XBrushes.Black, pageWidth - 250, yPos);
            gfx.DrawString($"Branch: {appointment.BranchName}", fontValue, XBrushes.Black, pageWidth - 250, yPos + 15);
            gfx.DrawString($"Payment Status: {appointment.PaymentStatus}", fontLabel, XBrushes.DarkGreen, pageWidth - 250, yPos + 30);

            // Separator Line
            gfx.DrawLine(XPens.DarkGray, 20, 140, pageWidth - 20, 140);

            // Services Purchased List
            gfx.DrawString("SERVICES PURCHASED", fontSubtitle, XBrushes.RoyalBlue, 20, 165);

            yPos = 190;
            gfx.DrawRectangle(XBrushes.LightGray, new XRect(20, yPos, pageWidth - 40, 20));
            gfx.DrawString("CODE", fontTableHeader, XBrushes.Black, 30, yPos + 14);
            gfx.DrawString("ITEM DESCRIPTION", fontTableHeader, XBrushes.Black, 130, yPos + 14);
            gfx.DrawString("TYPE", fontTableHeader, XBrushes.Black, 350, yPos + 14);
            gfx.DrawString("PRICE (INR)", fontTableHeader, XBrushes.Black, 470, yPos + 14);

            yPos = 215;

            // Draw Tests
            foreach (var test in appointment.Tests)
            {
                gfx.DrawString(test.Code, fontTableBody, XBrushes.Black, 30, yPos + 12);
                gfx.DrawString(test.Name, fontTableBody, XBrushes.Black, 130, yPos + 12);
                gfx.DrawString("Individual Test", fontTableBody, XBrushes.Black, 350, yPos + 12);
                gfx.DrawString(test.Price.ToString("F2"), fontTableBody, XBrushes.Black, 470, yPos + 12);
                yPos += 20;
            }

            // Draw Packages
            foreach (var pkg in appointment.Packages)
            {
                gfx.DrawString(pkg.Code, fontTableBody, XBrushes.Black, 30, yPos + 12);
                gfx.DrawString(pkg.Name, fontTableBody, XBrushes.Black, 130, yPos + 12);
                gfx.DrawString("Test Package", fontTableBody, XBrushes.Black, 350, yPos + 12);
                gfx.DrawString(pkg.Price.ToString("F2"), fontTableBody, XBrushes.Black, 470, yPos + 12);
                yPos += 20;
            }

            gfx.DrawLine(XPens.LightGray, 20, yPos + 5, pageWidth - 20, yPos + 5);
            yPos += 15;

            // Pricing totals
            double totalXLabel = pageWidth - 220;
            double totalXValue = pageWidth - 90;

            gfx.DrawString("Subtotal Amount:", fontValue, XBrushes.Black, totalXLabel, yPos);
            gfx.DrawString($"INR {invoice.TotalAmount.ToString("F2")}", fontValue, XBrushes.Black, totalXValue, yPos);

            gfx.DrawString("Discount Applied:", fontValue, XBrushes.Black, totalXLabel, yPos + 15);
            gfx.DrawString($"INR -{invoice.DiscountAmount.ToString("F2")}", fontValue, XBrushes.Black, totalXValue, yPos + 15);

            gfx.DrawString("Tax (18% GST):", fontValue, XBrushes.Black, totalXLabel, yPos + 30);
            gfx.DrawString($"INR {invoice.TaxAmount.ToString("F2")}", fontValue, XBrushes.Black, totalXValue, yPos + 30);

            gfx.DrawLine(XPens.DarkGray, totalXLabel, yPos + 38, pageWidth - 20, yPos + 38);

            gfx.DrawString("Total Paid:", fontLabel, XBrushes.Black, totalXLabel, yPos + 52);
            gfx.DrawString($"INR {invoice.FinalAmount.ToString("F2")}", fontLabel, XBrushes.Black, totalXValue, yPos + 52);

            // Footer notes
            double footerY = pageHeight - 80;
            gfx.DrawLine(XPens.LightGray, 20, footerY, pageWidth - 20, footerY);
            gfx.DrawString("Thank you for choosing Neo Leboretory.", fontLabel, XBrushes.RoyalBlue, 20, footerY + 20);
            gfx.DrawString("For queries or support, contact help@neoleboretory.lab or call 1800-200-NEOLEB.", fontTableBody, XBrushes.Black, 20, footerY + 35);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }
    }
}
