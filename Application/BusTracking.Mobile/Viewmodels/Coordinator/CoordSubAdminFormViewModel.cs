namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordSubAdminFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ICoordSubAdminService _service;

        [ObservableProperty] private int? _coordId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private ObservableCollection<PermissionGroup> _permissionGroups = [];
        [ObservableProperty] private bool _showNewPassword;
        [ObservableProperty] private string _newPassword = "";


        // ── Username live-check ───────────────────────────────────────────────
        [ObservableProperty] private string _usernameMessage = "";
        [ObservableProperty] private Color  _usernameMessageColor = Colors.Transparent;
        private CancellationTokenSource? _usernameCts;

        // Suppresses username check while page is loading data
        private bool _isLoadingData = true;

        partial void OnUserNameChanged(string value)
        {
            // Skip check entirely while InitializeAsync is populating fields
            if (_isLoadingData) return;

            _usernameCts?.Cancel();
            _usernameCts = new CancellationTokenSource();
            var cts = _usernameCts;
            UsernameMessage = "";
            UsernameMessageColor = Colors.Transparent;

            if (value.Length == 0) return;
            if (value.Length < 5)
            {
                UsernameMessage = "Username must have at least 5 characters";
                UsernameMessageColor = Color.FromArgb("#ef4444");
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(400, cts.Token);
                    if (cts.IsCancellationRequested) return;

                    int? excludeId = IsEditMode ? CoordId : null;
                    var r = await Auth.CheckUsernameAsync(value.Trim(), excludeId);

                    if (cts.IsCancellationRequested) return;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (!r.Success)
                        {
                            UsernameMessage = $"The username \"{value}\" is already taken";
                            UsernameMessageColor = Color.FromArgb("#ef4444");
                        }
                        else
                        {
                            UsernameMessage = $"\"{value}\" is available";
                            UsernameMessageColor = Color.FromArgb("#22c55e");
                        }
                    });
                }
                catch (TaskCanceledException) { }
                catch { }
            });
        }

        public CoordSubAdminFormViewModel(IAuthService auth, INavigationService nav, ICoordSubAdminService service)
            : base(auth, nav) { _service = service; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("CoordId", out var id)) { CoordId = (int)id; IsEditMode = true; Title = "Edit Bus Coordinators"; }
            else Title = "Add Bus Coordinators";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var allPerms = await _service.GetAllPermissionsAsync();
                var assignedIds = IsEditMode && CoordId.HasValue
                    ? await _service.GetAssignedPermissionsAsync(CoordId.Value)
                    : new List<int>();

                var groups = allPerms.GroupBy(p => p.ModuleName).Select(g => new PermissionGroup
                {
                    ModuleName = g.Key,
                    Permissions = new ObservableCollection<PermissionItem>(
                        g.Select(p => { p.IsSelected = assignedIds.Contains(p.Id); return p; }))
                });
                PermissionGroups = new ObservableCollection<PermissionGroup>(groups);

                if (IsEditMode && CoordId.HasValue)
                {
                    var c = await _service.GetByIdAsync(CoordId.Value);
                    if (c is null) return;
                    FullName = c.FullName; UserName = c.UserName ?? "";
                    Email = c.Email ?? ""; PhoneNumber = c.PhoneNumber ?? ""; IsActive = c.IsActive; NewPassword = "";
                }
                _isLoadingData = false;
            });
        }

        [RelayCommand]
        private void ToggleAllInGroup(PermissionGroup group)
        {
            var allSelected = group.Permissions.All(p => p.IsSelected);
            foreach (var p in group.Permissions) p.IsSelected = !allSelected;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(UserName)) { SetError("Full name and username are required."); return; }

            var selectedIds = PermissionGroups
                .SelectMany(g => g.Permissions)
                .Where(p => p.IsSelected)
                .Select(p => p.Id)
                .ToList();

            await RunAsync(async () =>
            {
                ApiResponse<object> r;
                if (IsEditMode)
                    r = await _service.UpdateAsync(CoordId!.Value, new UpdateCoordinatorRequest
                    { FullName = FullName, UserName = UserName, Email = Email.Length > 0 ? Email : null, NewPassword = NewPassword.Length > 0 ? NewPassword : null, PhoneNumber = PhoneNumber, IsActive = IsActive, PermissionIds = selectedIds });
                else
                    r = await _service.CreateAsync(new CreateCoordinatorRequest
                    { FullName = FullName, UserName = UserName, Email = Email.Length > 0 ? Email : null, PhoneNumber = PhoneNumber, Password = Password, PermissionIds = selectedIds, IsActive = IsActive });

                if (r.Success)
                {
                    if (IsEditMode)
                    {
                        var session = await Auth.GetCurrentUserAsync();
                        if (session != null && session.UserId == CoordId && session.UserName != UserName)
                        {
                            await ShowToastAsync("Username changed. Please log in again.");
                            await Auth.LogoutAsync();
                            await Nav.GoToLoginAsync();
                            return;
                        }
                    }
                    await ShowToastAsync(IsEditMode ? "Bus coordinator updated." : "Bus coordinator created.");
                    await Nav.GoBackAsync();
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (!CoordId.HasValue) return;
            if (!await ConfirmAsync("Reset Password", "Generate a new password for this bus coordinator?")) return;
            var r = await _service.ResetPasswordAsync(CoordId.Value);
            if (r.Success)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(r.Data);
                var doc = System.Text.Json.JsonDocument.Parse(json);
                NewPassword = doc.RootElement.TryGetProperty("plainPassword", out var pw) ? pw.GetString() ?? "" : "";
                ShowNewPassword = !string.IsNullOrEmpty(NewPassword);
                await ShowToastAsync("Password reset. Copy it now.");
            }
            else SetError(r.Message);
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
