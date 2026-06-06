namespace BusTracking.Mobile.Models.Feedback
{
    public partial class FeedbackItem : ObservableObject
    {
        public int FeedbackId { get; set; }
        public string UserName { get; set; } = "";
        public string Category { get; set; } = "";
        public string Email { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; }

        // Display helpers
        public string StatusBgColor => Status switch
        {
            "Open" => "#fff3cd",
            "InProgress" => "#cfe2ff",
            "Resolved" => "#d1e7dd",
            "Closed" => "#e2e3e5",
            _ => "#f8f9fa"
        };
        public string StatusTextColor => Status switch
        {
            "Open" => "#856404",
            "InProgress" => "#084298",
            "Resolved" => "#0f5132",
            "Closed" => "#41464b",
            _ => "#6c757d"
        };
        public string DisplayDate => CreatedAt.ToString("dd MMM yyyy");
    }

}
