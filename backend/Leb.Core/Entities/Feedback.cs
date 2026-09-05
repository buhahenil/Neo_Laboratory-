using System;

namespace Leb.Core.Entities
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int PatientId { get; set; }
        public int Rating { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Joined Field
        public string PatientName { get; set; } = string.Empty;
    }
}
