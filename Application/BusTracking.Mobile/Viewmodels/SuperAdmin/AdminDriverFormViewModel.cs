namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminDriverFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IDriverService _drivers;
        private readonly IBusService _buses;

        [ObservableProperty] private int? _userId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _email = ""; // optional, kept for backward compat
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _newPassword = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private string _licenseNumber = "";
        [ObservableProperty] private string _licenseExpiry = "";
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private List<BusItem> _busOptions = [];
        [ObservableProperty] private BusItem? _selectedBus;


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

                    int? excludeId = IsEditMode ? UserId : null;
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

        public AdminDriverFormViewModel(IAuthService auth, INavigationService nav,
            IDriverService drivers, IBusService buses)
            : base(auth, nav) { _drivers = drivers; _buses = buses; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UserId", out var id))
            {
                UserId = (int)id;
                IsEditMode = true;
                Title = "Edit Driver";
            }
            else Title = "Add Driver";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var buses = await _buses.GetAllForFormAsync();
                BusOptions = buses;

                if (IsEditMode && UserId.HasValue)
                {
                    var d = await _drivers.GetByIdAsync(UserId.Value);
                    if (d is null) return;
                    FullName = d.FullName;
                    UserName = d.UserName ?? "";
                    Email = d.Email ?? "";
                    NewPassword = "";
                    PhoneNumber = d.PhoneNumber ?? "";
                    LicenseNumber = d.LicenseNumber ?? "";
                    LicenseExpiry = d.LicenseExpiry ?? "";
                    IsActive = d.IsActive;
                    SelectedBus = BusOptions.FirstOrDefault(b => b.BusId == d.BusId);
                }
                _isLoadingData = false;
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
                SetError("Password is required for new drivers."); return;
            }

            await RunAsync(async () =>
            {
                ApiResponse<object> r;

                if (IsEditMode)
                {
                    r = await _drivers.UpdateAsync(UserId!.Value, new UpdateDriverRequest
                    {
                        FullName = FullName,
                        UserName = UserName,
                        Email = Email.Length > 0 ? Email : null,
                        NewPassword = NewPassword.Length > 0 ? NewPassword : null,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        LicenseNumber = LicenseNumber.Length > 0 ? LicenseNumber : null,
                        LicenseExpiry = LicenseExpiry.Length > 0 ? LicenseExpiry : null,
                        BusId = SelectedBus?.BusId,
                        IsActive = IsActive
                    });
                }
                else
                {
                    r = await _drivers.CreateAsync(new CreateDriverRequest
                    {
                        FullName = FullName,
                        UserName = UserName,
                        Email = Email.Length > 0 ? Email : null,
                        Password = Password,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        LicenseNumber = LicenseNumber.Length > 0 ? LicenseNumber : null,
                        LicenseExpiry = LicenseExpiry.Length > 0 ? LicenseExpiry : null,
                        BusId = SelectedBus?.BusId,
                        IsActive = IsActive
                    });
                }

                if (r.Success)
                {
                    if (IsEditMode)
                    {
                        // Force logout if current user's own username was changed
                    var session = await Auth.GetCurrentUserAsync();
                    if (session != null && session.UserId == UserId && session.UserName != UserName)
                    {
                        await ShowToastAsync("Username changed. Please log in again.");
                        await Auth.LogoutAsync();
                        await Nav.GoToLoginAsync();
                        return;
                    }
                    }
                    await ShowToastAsync(IsEditMode ? "Driver updated." : "Driver created.");
                    await Nav.GoBackAsync();
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}