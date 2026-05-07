namespace BusTracking.Common.DTOs.Feedback
{
    public class FeedbackListDto
    {
        public int FeedbackId { get; set; }
        public string UserName { get; set; } = "";
        public string Category { get; set; } = "";
        public string Email { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
