namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordBusDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IBusService _buses;
        [ObservableProperty] private int _busId;
        [ObservableProperty] private BusItem? _bus;

        public CoordBusDetailViewModel(IAuthService auth, INavigationService nav, IBusService buses)
            : base(auth, nav) { _buses = buses; Title = "Bus Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("BusId", out var id)) BusId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () => { Bus = await _buses.GetByIdAsync(BusId); });
        }

        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("CoordBusForm", new Dictionary<string, object> { ["BusId"] = BusId });
    }
}
