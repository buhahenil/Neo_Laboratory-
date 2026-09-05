using System;

namespace Leb.Core.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int AppointmentId { get; set; }
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Razorpay, Stripe, Cash, Online
        public string PaymentStatus { get; set; } = string.Empty; // Success, Failed, Refunded
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Joined Field
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
    }
}
