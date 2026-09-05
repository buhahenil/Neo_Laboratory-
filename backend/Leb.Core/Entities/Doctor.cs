namespace Leb.Core.Entities
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }
        public string? Password { get; set; }

        // User Fields (Join details)
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
