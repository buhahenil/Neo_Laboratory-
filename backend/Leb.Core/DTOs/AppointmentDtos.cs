using System;
using System.Collections.Generic;

namespace Leb.Core.DTOs
{
    public class BookAppointmentDto
    {
        public int PatientId { get; set; }
        public int BranchId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int SlotId { get; set; }
        public List<int> TestIds { get; set; } = new List<int>();
        public List<int> PackageIds { get; set; } = new List<int>();
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; } = "Online"; // Razorpay, Stripe, Cash, Online
        public string PaymentStatus { get; set; } = "Unpaid"; // Success, Failed, Unpaid
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public int? DoctorId { get; set; }
    }

    public class RescheduleDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int SlotId { get; set; }
    }

    public class AppointmentStatusUpdateDto
    {
        public int AppointmentId { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Confirmed, SampleCollected, ResultUploaded, Completed, Cancelled
        public int? StaffId { get; set; }
    }
}
