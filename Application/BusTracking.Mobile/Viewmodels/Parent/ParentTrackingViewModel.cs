namespace BusTracking.Mobile.Viewmodels.Parent
{
    public partial class ParentTrackingViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IParentService _parents;
        private System.Timers.Timer? _pollTimer;

        [ObservableProperty] private int _studentId;
        [ObservableProperty] private TrackingData? _tracking;
        [ObservableProperty] private string _statusLabel = "Loading…";
        [ObservableProperty] private double _busLatitude;
        [ObservableProperty] private double _busLongitude;
        [ObservableProperty] private bool _isLive;

        public ParentTrackingViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; Title = "Track Bus"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("StudentId", out var id)) StudentId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await PollAsync();
            // Poll every 10 seconds when live
            _pollTimer = new System.Timers.Timer(10_000);
            _pollTimer.Elapsed += async (_, _) => await PollAsync();
            _pollTimer.Start();
        }

        private async Task PollAsync()
        {
            var data = await _parents.TrackChildBusAsync(StudentId);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Tracking = data;
                IsLive = data?.IsLive ?? false;
                StatusLabel = IsLive ? $"🚌 {data?.Bus?.BusNumber} — Moving" : data?.Message ?? "No active trip";
                if (IsLive && data?.Location is not null)
                {
                    BusLatitude = (double)data.Location.Latitude;
                    BusLongitude = (double)data.Location.Longitude;
                }
            });
        }

        public void StopPolling() => _pollTimer?.Stop();
    }
}
