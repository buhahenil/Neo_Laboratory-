using System;
using System.Collections.Generic;

namespace Leb.Core.Entities
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int BranchId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int SlotId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, SampleCollected, ResultUploaded, Completed, Cancelled
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Refunded
        public string? Notes { get; set; }
        public int? DoctorId { get; set; }
        public int? StaffId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Joined Fields
        public string BranchName { get; set; } = string.Empty;
        public string BranchAddress { get; set; } = string.Empty;
        public string BranchCity { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string PatientFirstName { get; set; } = string.Empty;
        public string PatientLastName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        
        public string? DoctorFirstName { get; set; }
        public string? DoctorLastName { get; set; }
        public string? StaffFirstName { get; set; }
        public string? StaffLastName { get; set; }

        // Selected Tests & Packages
        public List<Test> Tests { get; set; } = new List<Test>();
        public List<Package> Packages { get; set; } = new List<Package>();
    }
}
