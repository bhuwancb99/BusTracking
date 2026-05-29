namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordDriverDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IDriverService _drivers;
        [ObservableProperty] private int _userId;
        [ObservableProperty] private DriverItem? _driver;

        public CoordDriverDetailViewModel(IAuthService auth, INavigationService nav, IDriverService drivers)
            : base(auth, nav) { _drivers = drivers; Title = "Driver Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UserId", out var id)) UserId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () => { Driver = await _drivers.GetByIdAsync(UserId); });
        }
    }
}
