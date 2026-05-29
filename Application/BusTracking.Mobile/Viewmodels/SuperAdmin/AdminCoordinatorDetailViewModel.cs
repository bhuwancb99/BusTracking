namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminCoordinatorDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ICoordinatorService _coords;

        [ObservableProperty] private int _userId;
        [ObservableProperty] private CoordinatorItem? _coordinator;

        public AdminCoordinatorDetailViewModel(IAuthService auth, INavigationService nav, ICoordinatorService coords)
            : base(auth, nav) { _coords = coords; Title = "Coordinator Details"; }

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
                Coordinator = await _coords.GetByIdAsync(UserId);
            });
        }

        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("AdminCoordinatorForm", new Dictionary<string, object> { ["UserId"] = UserId });

        [RelayCommand]
        private async Task ToggleAsync()
        {
            var r = await _coords.ToggleAsync(UserId);
            if (r.Success) { await ShowToastAsync(r.Message); await LoadAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {Coordinator?.FullName}?")) return;
            var r = await _coords.ResetPasswordAsync(UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", r.Message);
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!await ConfirmAsync("Delete", $"Delete coordinator '{Coordinator?.FullName}'?")) return;
            var r = await _coords.DeleteAsync(UserId);
            if (r.Success) { await ShowToastAsync("Coordinator deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}
