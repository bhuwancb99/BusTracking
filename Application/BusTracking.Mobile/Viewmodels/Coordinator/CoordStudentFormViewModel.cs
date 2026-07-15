namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordStudentFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IStudentService _students;
        private readonly IBusService _buses;
        private readonly IRouteService _routes;

        [ObservableProperty] private int? _studentId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _email = ""; // optional, kept for backward compat
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _newPassword = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private List<StandardItem> _standardOptions = [];
        [ObservableProperty] private StandardItem? _selectedStandard;
        [ObservableProperty] private string _studentCode = "";
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private List<BusItem> _busOptions = [];
        [ObservableProperty] private List<StopItem> _stopOptions = [];
        [ObservableProperty] private BusItem? _selectedBus;
        [ObservableProperty] private StopItem? _selectedStop;


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

                    int? excludeId = IsEditMode ? StudentId : null;
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

        public CoordStudentFormViewModel(IAuthService auth, INavigationService nav,
            IStudentService students, IBusService buses, IRouteService routes)
            : base(auth, nav) { _students = students; _buses = buses; _routes = routes; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("StudentId", out var id)) { StudentId = (int)id; IsEditMode = true; Title = "Edit Student"; }
            else Title = "Add Student";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                BusOptions = await _buses.GetAllForFormAsync();
                StandardOptions = await _students.GetStandardsAsync();
                if (IsEditMode && StudentId.HasValue)
                {
                    var s = await _students.GetByIdAsync(StudentId.Value);
                    if (s is null) return;
                    FullName = s.FullName; UserName = s.UserName ?? "";
                    Email = s.Email ?? "";
                    NewPassword = "";
                    PhoneNumber = s.PhoneNumber ?? "";
                    StudentCode = s.StudentCode;
                    SelectedStandard = StandardOptions.FirstOrDefault(st => st.StandardId == s.StandardId);
                    IsActive = s.IsActive;
                    SelectedBus = BusOptions.FirstOrDefault(b => b.BusId == s.BusId);
                    if (SelectedBus?.RouteId.HasValue == true)
                    {
                        StopOptions = await _routes.GetStopsAsync(SelectedBus.RouteId.Value);
                        SelectedStop = StopOptions.FirstOrDefault(st => st.StopId == s.StopId);
                    }
                }
                _isLoadingData = false;
            });
        }

        partial void OnSelectedBusChanged(BusItem? value)
        {
            if (value?.RouteId.HasValue == true) _ = LoadStopsAsync(value.RouteId.Value);
            else StopOptions = [];
        }

        private async Task LoadStopsAsync(int routeId)
        {
            StopOptions = await _routes.GetStopsAsync(routeId);
            SelectedStop = null;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(UserName))
            { SetError("Full name and username are required."); return; }
            if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
            { SetError("Password is required for new students."); return; }

            await RunAsync(async () =>
            {
                ApiResponse<object> r = IsEditMode
                    ? await _students.UpdateAsync(StudentId!.Value, new UpdateStudentRequest
                    {
                        FullName = FullName,
                        UserName = UserName,
                        Email = Email.Length > 0 ? Email : null,
                        NewPassword = NewPassword.Length > 0 ? NewPassword : null,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        StudentCode = StudentCode,
                        StandardId = SelectedStandard?.StandardId,
                        BusId = SelectedBus?.BusId,
                        StopId = SelectedStop?.StopId,
                        IsActive = IsActive
                    })
                    : await _students.CreateAsync(new CreateStudentRequest
                    {
                        FullName = FullName,
                        UserName = UserName,
                        Email = Email.Length > 0 ? Email : null,
                        Password = Password,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        StudentCode = string.Empty,
                        StandardId = SelectedStandard?.StandardId,
                        BusId = SelectedBus?.BusId,
                        StopId = SelectedStop?.StopId,
                        IsActive = IsActive
                    });

                                if (r.Success)
                {
                    if (IsEditMode)
                    {
                        var session = await Auth.GetCurrentUserAsync();
                        if (session != null && session.UserId == StudentId && session.UserName != UserName)
                        {
                            await ShowToastAsync("Username changed. Please log in again.");
                            await Auth.LogoutAsync();
                            await Nav.GoToLoginAsync();
                            return;
                        }
                    }
                    await ShowToastAsync(IsEditMode ? "Student updated." : "Student created.");
                    await Nav.GoBackAsync();
                }
                else SetError(r.Message);
                _isLoadingData = false;
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
