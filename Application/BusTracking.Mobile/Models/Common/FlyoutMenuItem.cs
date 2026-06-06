namespace BusTracking.Mobile.Models.Common
{
    public partial class FlyoutMenuItem : ObservableObject
    {
        public string Icon { get; init; } = "";
        public string IconSvg { get; init; } = "";
        public string Title { get; init; } = "";
        public string Route { get; init; } = "";
        public string? IconColor { get; init; }

        [ObservableProperty] private bool _isActive;
    }

}
