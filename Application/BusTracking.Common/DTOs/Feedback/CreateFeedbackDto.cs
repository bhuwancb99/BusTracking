namespace BusTracking.Common.DTOs.Feedback
{
    public class CreateFeedbackDto
    {
        public string Category { get; set; } = "";   // Inquiry | Complaint
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Description { get; set; } = "";
    }
}
