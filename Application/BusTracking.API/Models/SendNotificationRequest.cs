namespace BusTracking.API.Models
{
    public class SendNotificationRequest
    {
        public int RecipientUserId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string Type { get; set; } = "General";
    }
}
