using System;

namespace Leb.Core.Entities
{
    public class TimeSlot
    {
        public int SlotId { get; set; }
        public int BranchId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxBookings { get; set; }
        public bool IsActive { get; set; }
    }
}
