namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminCoordinatorDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ICoordinatorService _coords;

        [ObservableProperty] private int _userId;
        [ObservableProperty] private CoordinatorItem? _coordinator;
        [ObservableProperty] private ObservableCollection<PermissionGroup> _permissionGroups = [];

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
                BuildPermissionGroups();
            });
        }

        private void BuildPermissionGroups()
        {
            if (Coordinator?.Permissions == null) { PermissionGroups = []; return; }

            var groups = Coordinator.Permissions
                .Select(p => new PermissionItem { Key = p, Description = FormatDescription(p), IsSelected = true })
                .GroupBy(p => ExtractModule(p.Key))
                .Select(g => new PermissionGroup
                {
                    ModuleName = g.Key,
                    Permissions = new ObservableCollection<PermissionItem>(g)
                });
            PermissionGroups = new ObservableCollection<PermissionGroup>(groups);
        }

        private static string ExtractModule(string key)
        {
            var parts = key.Split('.');
            if (parts.Length < 1) return key;
            var raw = parts[0];
            return raw.Length > 0
                ? char.ToUpper(raw[0]) + raw.Substring(1)
                : raw;
        }

        private static string FormatDescription(string key)
        {
            var parts = key.Split('.');
            if (parts.Length < 2) return key;
            var action = parts[1];
            var module = parts[0];
            var actionStr = action.Length > 0 ? char.ToUpper(action[0]) + action.Substring(1) : action;
            var moduleStr = module.Length > 0 ? char.ToUpper(module[0]) + module.Substring(1) : module;
            return $"{actionStr} {moduleStr}";
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
            if (r.Success) await ShowAlertAsync("Password Reset", $"New password: {r.Data?.PlainPassword}");
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
