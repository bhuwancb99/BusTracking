namespace BusTracking.Mobile.Models.Bus
{
    public class BusItem
    {
        public int BusId { get; set; }
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public string? RouteName { get; set; }
        public int? RouteId { get; set; }
        public int? DriverUserId { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public int? Capacity { get; set; }
        public int StudentCount { get; set; }
        public bool IsActive { get; set; }

        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public Color StatusColor => IsActive ? Colors.Green : Colors.Red;
        public string DriverDisplay => DriverName ?? "Unassigned";
        public string RouteDisplay => RouteName ?? "No Route";
        public Color StatusBgColor => IsActive ? Color.FromArgb("#d1fae5") : Color.FromArgb("#f1f5f9");
        public Color StatusTextColor => IsActive ? Color.FromArgb("#065f46") : Color.FromArgb("#475569");
    }
}
