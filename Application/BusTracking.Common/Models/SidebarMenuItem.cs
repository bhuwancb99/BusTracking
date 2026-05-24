namespace BusTracking.Common.Models
{
    public class SidebarMenuItem
    {
        public string Label { get; set; } = "";
        public string Icon { get; set; } = "";     // Bootstrap icon class e.g. bi-house
        public string Controller { get; set; } = "";
        public string Action { get; set; } = "Index";
        public string Area { get; set; } = "";     // e.g. "SuperAdmin", "BusCoordinator", ""
        public string[] AllowedRoles { get; set; } = [];
        public List<SidebarMenuItem> Children { get; set; } = [];
        public bool HasChildren => Children.Count > 0;
    }
}
