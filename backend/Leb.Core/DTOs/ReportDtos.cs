namespace Leb.Core.DTOs
{
    public class UploadResultDto
    {
        public int AppointmentId { get; set; }
        public int TestId { get; set; }
        public string ResultValue { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public int StaffId { get; set; }
    }
}
