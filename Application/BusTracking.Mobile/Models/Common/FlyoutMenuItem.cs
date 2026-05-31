namespace BusTracking.Mobile.Models.Common
{
    public class FlyoutMenuItem
    {
        public string Icon { get; init; } = "";
        public string IconSvg { get; init; } = "";
        public string Title { get; init; } = "";
        public string Route { get; init; } = "";
        public bool IsActive { get; set; }
    }
}
