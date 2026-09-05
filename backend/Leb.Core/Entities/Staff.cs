using System.Collections.Generic;

namespace Leb.Core.Entities
{
    public class Staff
    {
        public int StaffId { get; set; }
        public int UserId { get; set; }
        public int BranchId { get; set; }
        public string Designation { get; set; } = string.Empty;
        public List<int> BranchIds { get; set; } = new List<int>();
        public string? Password { get; set; }

        // Joined Fields
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string BranchName { get; set; } = string.Empty;
    }
}
