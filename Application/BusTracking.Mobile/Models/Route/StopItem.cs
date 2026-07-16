namespace BusTracking.Mobile.Models.Route
{
    public partial class StopItem : ObservableObject
    {
        public int StopId { get; set; }
        [ObservableProperty] private string _stopName = "";
        [ObservableProperty] private int _stopOrder;
        [ObservableProperty] private int _originalOrder;
        [ObservableProperty] private string _orderText = "";
        [ObservableProperty] private decimal? _latitude;
        [ObservableProperty] private decimal? _longitude;
        [ObservableProperty] private string? _morningTime;
        [ObservableProperty] private string? _eveningTime;

        // Local state for toggling inline editing view/edit templates
        [ObservableProperty] private bool _isEditing;
    }
}
