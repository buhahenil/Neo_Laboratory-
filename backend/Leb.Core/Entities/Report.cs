using System;

namespace Leb.Core.Entities
{
    public class Report
    {
        public int ReportId { get; set; }
        public int AppointmentId { get; set; }
        public int TestId { get; set; }
        public string? ResultValue { get; set; }
        public string? Remarks { get; set; }
        public int? UploadedByStaffId { get; set; }
        public DateTime? UploadedAt { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed

        // Outsourcing Fields
        public bool IsOutsourced { get; set; } = false;
        public string? ExternalLabName { get; set; }
        public string? ExternalBarcode { get; set; }
        public DateTime? DispatchedAt { get; set; }

        // Joined Fields
        public string TestName { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public string? NormalRange { get; set; }
        public string SampleType { get; set; } = string.Empty;
        public string? Preparation { get; set; }
        public string? StaffFirstName { get; set; }
        public string? StaffLastName { get; set; }
    }
}
