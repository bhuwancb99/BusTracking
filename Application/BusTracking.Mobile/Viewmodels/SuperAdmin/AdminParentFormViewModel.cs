namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminParentFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IParentService _parents;

        [ObservableProperty] private int? _userId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _email = ""; // optional, kept for backward compat
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _newPassword = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private string _studentCodes = "";   // comma-separated
        [ObservableProperty] private bool _isActive = true;


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

        public AdminParentFormViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UserId", out var id))
            {
                UserId = (int)id;
                IsEditMode = true;
                Title = "Edit Parent";
            }
            else Title = "Add Parent";
        }

        public override async Task InitializeAsync()
        {
            if (!IsEditMode || !UserId.HasValue) return;

            await RunAsync(async () =>
            {
                var p = await _parents.GetByIdAsync(UserId.Value);
                if (p is null) return;
                FullName = p.FullName;
                UserName = p.UserName ?? "";
                Email = p.Email ?? "";
                PhoneNumber = p.PhoneNumber ?? "";
                IsActive = p.IsActive;
                NewPassword = "";
                StudentCodes = string.Join(", ", p.Students.Select(s => s.StudentCode));
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
                SetError("Password is required for new parents."); return;
            }

            var codes = StudentCodes
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => c.Length > 0)
                .ToList();

            await RunAsync(async () =>
            {
                ApiResponse<object> r;

                if (IsEditMode)
                {
                    r = await _parents.UpdateAsync(UserId!.Value, new UpdateParentRequest
                    {
                        FullName = FullName,
                        UserName = UserName,
                        Email = Email.Length > 0 ? Email : null,
                        NewPassword = NewPassword.Length > 0 ? NewPassword : null,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        StudentCodes = codes,
                        IsActive = IsActive
                    });
                }
                else
                {
                    r = await _parents.CreateAsync(new CreateParentRequest
                    {
                        FullName = FullName,
                        UserName = UserName,
                        Email = Email.Length > 0 ? Email : null,
                        Password = Password,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        StudentCodes = codes,
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
                    await ShowToastAsync(IsEditMode ? "Parent updated." : "Parent created.");
                    await Nav.GoBackAsync();
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}