namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    // Coordinator role is view-only for drivers — no edit/delete/toggle in coordinator API
    public partial class CoordDriverDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IDriverService _drivers;
        [ObservableProperty] private int _userId;
        [ObservableProperty] private DriverItem? _driver;

        public bool CanEdit   => Can("driver.edit");
        public bool CanDelete => Can("driver.delete");

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
    
        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("CoordDriverForm", new Dictionary<string, object> { ["UserId"] = UserId });

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!await ConfirmAsync("Delete Driver", $"Delete '{Driver?.FullName}'?")) return;
            var r = await _drivers.DeleteAsync(UserId);
            if (r.Success) { await ShowToastAsync("Driver deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }

    }
}