namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordSubAdminDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ICoordSubAdminService _service;
        private readonly IAuthService _auth;

        [ObservableProperty] private int _userId;
        [ObservableProperty] private CoordinatorItem? _coordinator;
        [ObservableProperty] private ObservableCollection<PermissionGroup> _permissionGroups = [];
        [ObservableProperty] private bool _isSelf;          // true when viewing own account

        public bool CanEdit => Can("subadmin.edit") && !IsSelf;
        public bool CanDelete => Can("subadmin.delete") && !IsSelf;
        public bool CanToggle => Can("subadmin.edit") && !IsSelf;

        public CoordSubAdminDetailViewModel(IAuthService auth, INavigationService nav, ICoordSubAdminService service)
            : base(auth, nav) { _service = service; _auth = auth; Title = "Bus Coordinators Detail"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("CoordId", out var id)) UserId = (int)id;
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                // Detect own account
                var session = await _auth.GetCurrentUserAsync();
                IsSelf = session?.UserId == UserId;

                Coordinator = await _service.GetByIdAsync(UserId);
                BuildPermissionGroups();

                // Notify computed properties after IsSelf is known
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(CanToggle));
            });
        }

        private void BuildPermissionGroups()
        {
            if (Coordinator?.Permissions == null) { PermissionGroups = []; return; }

            var groups = Coordinator.Permissions
                .Select(p => new PermissionItem
                {
                    Key = p,
                    Description = FormatDescription(p),
                    IsSelected = true
                })
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
            var raw = parts[0];
            return raw.Length > 0 ? char.ToUpper(raw[0]) + raw[1..] : raw;
        }

        private static string FormatDescription(string key)
        {
            var parts = key.Split('.');
            if (parts.Length < 2) return key;
            var action = parts[1];
            var module = parts[0];
            var actionStr = action.Length > 0 ? char.ToUpper(action[0]) + action[1..] : action;
            var moduleStr = module.Length > 0 ? char.ToUpper(module[0]) + module[1..] : module;
            return $"{actionStr} {moduleStr}";
        }

        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("CoordSubAdminForm", new Dictionary<string, object> { ["CoordId"] = UserId });

        [RelayCommand]
        private async Task ToggleAsync()
        {
            var r = await _service.ToggleAsync(UserId);
            if (r.Success) { await ShowToastAsync(r.Message); await LoadAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {Coordinator?.FullName}?")) return;
            var r = await _service.ResetPasswordAsync(UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", r.Message);
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!await ConfirmAsync("Delete", $"Mark '{Coordinator?.FullName}' as inactive?")) return;
            var r = await _service.DeleteAsync(UserId);
            if (r.Success) { await ShowToastAsync("Marked inactive."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}
