using System;

namespace Leb.Core.Entities
{
    public class Patient
    {
        public int PatientId { get; set; }
        public int UserId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? BloodGroup { get; set; }
        public string? Address { get; set; }

        // User Fields (Join details)
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
