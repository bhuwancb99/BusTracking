namespace BusTracking.Common.DTOs.Feedback
{
    public class CreateFeedbackDto
    {
        public string Category { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Subject { get; set; }
        public string Description { get; set; } = "";
        public string? Message { get; set; }
    }
}
