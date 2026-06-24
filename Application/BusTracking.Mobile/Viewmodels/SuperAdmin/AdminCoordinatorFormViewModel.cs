namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminCoordinatorFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ICoordinatorService _coords;

        [ObservableProperty] private int? _userId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _newPassword = "";
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private ObservableCollection<PermissionGroup> _permissionGroups = [];

        public AdminCoordinatorFormViewModel(IAuthService auth, INavigationService nav, ICoordinatorService coords)
            : base(auth, nav) { _coords = coords; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UserId", out var id)) { UserId = (int)id; IsEditMode = true; Title = "Edit Coordinator"; }
            else Title = "Add Coordinator";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var allPerms = await _coords.GetAllPermissionsAsync();
                var assignedIds = IsEditMode && UserId.HasValue
                    ? await _coords.GetAssignedPermissionsAsync(UserId.Value)
                    : new List<int>();

                // Group permissions by ModuleName
                var groups = allPerms.GroupBy(p => p.ModuleName).Select(g => new PermissionGroup
                {
                    ModuleName = g.Key,
                    Permissions = new ObservableCollection<PermissionItem>(
                        g.Select(p => { p.IsSelected = assignedIds.Contains(p.Id); return p; }))
                });
                PermissionGroups = new ObservableCollection<PermissionGroup>(groups);

                if (IsEditMode && UserId.HasValue)
                {
                    var c = await _coords.GetByIdAsync(UserId.Value);
                    if (c is null) return;
                    FullName = c.FullName; UserName = c.UserName ?? ""; Email = c.Email ?? ""; PhoneNumber = c.PhoneNumber ?? ""; IsActive = c.IsActive; NewPassword = "";
                }
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(UserName))
            {
                SetError("Full name and username are required."); return;
            }
            if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
            {
                SetError("Password is required for new coordinator."); return;
            }

            var selectedIds = PermissionGroups
                .SelectMany(g => g.Permissions)
                .Where(p => p.IsSelected)
                .Select(p => p.Id)
                .ToList();

            await RunAsync(async () =>
            {
                ApiResponse<object> r;
                if (IsEditMode)
                    r = await _coords.UpdateAsync(UserId!.Value, new UpdateCoordinatorRequest
                    { FullName = FullName, UserName = UserName, Email = Email.Length > 0 ? Email : null, NewPassword = NewPassword.Length > 0 ? NewPassword : null, PhoneNumber = PhoneNumber, PermissionIds = selectedIds, IsActive = IsActive });
                else
                    r = await _coords.CreateAsync(new CreateCoordinatorRequest
                    {
                        FullName = FullName,
                        UserName = UserName,
                        Email = Email.Length > 0 ? Email : null,
                        PhoneNumber = PhoneNumber,
                        Password = Password,
                        PermissionIds = selectedIds,
                        IsActive = IsActive
                    });

                if (r.Success) { await ShowToastAsync(IsEditMode ? "Coordinator updated." : "Coordinator created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();

        [RelayCommand]
        private void ToggleAllInGroup(PermissionGroup group)
        {
            var allSelected = group.Permissions.All(p => p.IsSelected);
            foreach (var p in group.Permissions) p.IsSelected = !allSelected;
        }
    }
}
