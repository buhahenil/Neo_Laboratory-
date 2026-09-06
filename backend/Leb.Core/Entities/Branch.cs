using System;

namespace Leb.Core.Entities
{
    public class Branch
    {
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Letterhead Admin Configuration
        public string GujaratiTitle { get; set; } = "નીઓ લેબોરેટરી";
        public string Doctor1Name { get; set; } = "Ankur Ramani";
        public string Doctor1Degree { get; set; } = "B.Voc , PGDMLT";
        public string Doctor2Name { get; set; } = "Hardik Ramani";
        public string Doctor2Degree { get; set; } = "BSC. Micro, MSC. Embryo, PGDMLT";
        public string TimingInfo { get; set; } = "8:00 AM to 8:00 PM";
        public string? LetterheadImagePath { get; set; }
    }
}
