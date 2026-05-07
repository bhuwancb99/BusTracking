namespace BusTracking.Common.Models
{
    public class DashboardCardModel
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "";   // CSS class like text-primary
        public string? NavUrl { get; set; }
    }
}
