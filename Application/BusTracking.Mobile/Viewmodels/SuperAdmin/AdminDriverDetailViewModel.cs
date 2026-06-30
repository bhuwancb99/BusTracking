namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminDriverDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IDriverService _drivers;

        [ObservableProperty] private int _userId;
        [ObservableProperty] private DriverItem? _driver;

        public AdminDriverDetailViewModel(IAuthService auth, INavigationService nav, IDriverService drivers)
            : base(auth, nav) { _drivers = drivers; Title = "Driver Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UserId", out var id)) UserId = (int)id;
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                Driver = await _drivers.GetByIdAsync(UserId);
            });
        }

        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("AdminDriverForm", new Dictionary<string, object> { ["UserId"] = UserId });

        [RelayCommand]
        private async Task ToggleAsync()
        {
            var r = await _drivers.ToggleAsync(UserId);
            if (r.Success) { await ShowToastAsync(r.Message); await LoadAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {Driver?.FullName}?")) return;
            var r = await _drivers.ResetPasswordAsync(UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", $"New password: {r.Data?.PlainPassword}");
            else SetError(r.Message);
        }

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

