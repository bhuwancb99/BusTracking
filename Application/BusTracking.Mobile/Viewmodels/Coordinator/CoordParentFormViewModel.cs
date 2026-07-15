namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordParentFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IParentService _parents;

        [ObservableProperty] private int? _userId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _newPassword = "";
        [ObservableProperty] private bool _isActive = true;

        // ── Linked Students ───────────────────────────────────────────────────
        [ObservableProperty] private string _studentSearchText = "";
        [ObservableProperty] private ObservableCollection<StudentSearchItem> _searchResults = [];
        [ObservableProperty] private bool _showSearchResults;
        [ObservableProperty] private ObservableCollection<LinkedStudent> _linkedStudents = [];

        private CancellationTokenSource? _searchCts;

        partial void OnStudentSearchTextChanged(string value)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var cts = _searchCts;
            ShowSearchResults = false;
            if (string.IsNullOrWhiteSpace(value)) { SearchResults = []; return; }
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, cts.Token);
                    if (cts.IsCancellationRequested) return;
                    var results = await _parents.SearchStudentsAsync(value.Trim());
                    if (cts.IsCancellationRequested) return;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var linked = LinkedStudents.Select(s => s.StudentCode).ToHashSet();
                        SearchResults = new ObservableCollection<StudentSearchItem>(
                            results.Where(r => !linked.Contains(r.StudentCode)));
                        ShowSearchResults = SearchResults.Count > 0;
                    });
                }
                catch (TaskCanceledException) { }
            });
        }

        [RelayCommand]
        private void SelectStudent(StudentSearchItem item)
        {
            if (LinkedStudents.Any(s => s.StudentCode == item.StudentCode)) return;
            LinkedStudents.Add(new LinkedStudent
            {
                StudentId = item.StudentId,
                StudentCode = item.StudentCode,
                FullName = item.FullName,
                StandardName = item.StandardName
            });
            StudentSearchText = "";
            SearchResults = [];
            ShowSearchResults = false;
        }

        [RelayCommand]
        private void RemoveStudent(LinkedStudent student) => LinkedStudents.Remove(student);

        // ── Username live-check ───────────────────────────────────────────────
        [ObservableProperty] private string _usernameMessage = "";
        [ObservableProperty] private Color _usernameMessageColor = Colors.Transparent;
        private CancellationTokenSource? _usernameCts;
        private bool _isLoadingData = true;

        partial void OnUserNameChanged(string value)
        {
            if (_isLoadingData) return;
            _usernameCts?.Cancel();
            _usernameCts = new CancellationTokenSource();
            var cts = _usernameCts;
            UsernameMessage = "";
            UsernameMessageColor = Colors.Transparent;
            if (value.Length == 0) return;
            if (value.Length < 5) { UsernameMessage = "Username must have at least 5 characters"; UsernameMessageColor = Color.FromArgb("#ef4444"); return; }
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(400, cts.Token);
                    if (cts.IsCancellationRequested) return;
                    var r = await Auth.CheckUsernameAsync(value.Trim(), IsEditMode ? UserId : null);
                    if (cts.IsCancellationRequested) return;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (!r.Success) { UsernameMessage = $"The username \"{value}\" is already taken"; UsernameMessageColor = Color.FromArgb("#ef4444"); }
                        else { UsernameMessage = $"\"{value}\" is available"; UsernameMessageColor = Color.FromArgb("#22c55e"); }
                    });
                }
                catch (TaskCanceledException) { }
                catch { }
            });
        }

        public CoordParentFormViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UserId", out var id)) { UserId = (int)id; IsEditMode = true; Title = "Edit Parent"; }
            else Title = "Add Parent";
        }

        public override async Task InitializeAsync()
        {
            if (!IsEditMode || !UserId.HasValue) return;
            await RunAsync(async () =>
            {
                var p = await _parents.GetByIdAsync(UserId.Value);
                if (p is null) return;
                FullName = p.FullName; UserName = p.UserName ?? "";
                Email = p.Email ?? ""; PhoneNumber = p.PhoneNumber ?? "";
                IsActive = p.IsActive; NewPassword = "";
                LinkedStudents = new ObservableCollection<LinkedStudent>(p.Students);
                _isLoadingData = false;
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(UserName))
            { SetError("Full name and username are required."); return; }
            if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
            { SetError("Password is required for new parents."); return; }

            var codes = LinkedStudents.Select(s => s.StudentCode).ToList();

            await RunAsync(async () =>
            {
                ApiResponse<object> r = IsEditMode
                    ? await _parents.UpdateAsync(UserId!.Value, new UpdateParentRequest
                    {
                        FullName = FullName, UserName = UserName,
                        Email = Email.Length > 0 ? Email : null,
                        NewPassword = NewPassword.Length > 0 ? NewPassword : null,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        StudentCodes = codes, IsActive = IsActive
                    })
                    : await _parents.CreateAsync(new CreateParentRequest
                    {
                        FullName = FullName, UserName = UserName,
                        Email = Email.Length > 0 ? Email : null,
                        Password = Password,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        StudentCodes = codes, IsActive = IsActive
                    });

                if (r.Success)
                { await ShowToastAsync(IsEditMode ? "Parent updated." : "Parent created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
