namespace BusTracking.Mobile.Models.Driver
{
    public class DriverNotificationItem
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string NotificationType { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }

        public string TimeDisplay => SentAt.ToLocalTime().ToString("dd MMM, HH:mm");
        public Color BgColor => IsRead ? Colors.White : Color.FromArgb("#f0f7ff");

        public string IconGlyph => NotificationType switch
        {
            "BusApproaching" => "\uf5e8",   // bus
            "StudentPickedUp" => "\uf26b",   // check-circle
            "NoShow" => "\uf071",   // exclamation-triangle
            "BusAssigned" => "\uf207",   // bus-alt
            "RouteChanged" => "\uf279",   // map-marked
            "Broadcast" => "\uf519",   // bullhorn
            _ => "\uf0f3"    // bell
        };
    }
}
