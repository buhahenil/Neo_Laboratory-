using System;

namespace Leb.Core.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int AppointmentId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
