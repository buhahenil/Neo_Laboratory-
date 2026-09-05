using System;

namespace Leb.Core.Entities
{
    public class Holiday
    {
        public int HolidayId { get; set; }
        public int BranchId { get; set; }
        public DateTime HolidayDate { get; set; }
        public string? Description { get; set; }
    }
}
