namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminStudentListViewModel : BaseViewModel
    {
        private readonly IStudentService _students;

        [ObservableProperty] private ObservableCollection<StudentItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _canLoadMore;
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private string _selectedFilter = "Active";

        public string SearchPlaceholder => "Search students…";
        public List<string> FilterOptions => ["Active", "Inactive", "Both"];

        public bool CanAdd => Can("student.add");
        public bool CanEdit => Can("student.edit");
        public bool CanDelete => Can("student.delete");

        public AdminStudentListViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "Students"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        partial void OnSelectedFilterChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _students.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                Items = new ObservableCollection<StudentItem>(data.Items);
                IsEmpty = !Items.Any();
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!CanLoadMore || IsBusy) return;
            await RunAsync(async () =>
            {
                CurrentPage++;
                var data = await _students.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminStudentForm");

        // Tap row → Detail page
        [RelayCommand]
        private Task DetailAsync(StudentItem s) =>
            Nav.GoToAsync("AdminStudentDetail", new Dictionary<string, object> { ["StudentId"] = s.StudentId });

        [RelayCommand]
        private Task EditAsync(StudentItem s) =>
            Nav.GoToAsync("AdminStudentForm", new Dictionary<string, object> { ["StudentId"] = s.StudentId });

        [RelayCommand]
        private async Task ToggleAsync(StudentItem s)
        {
            var r = await _students.ToggleAsync(s.StudentId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync(StudentItem s)
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {s.FullName}?")) return;
            var r = await _students.ResetPasswordAsync(s.StudentId);
            if (r.Success) await ShowAlertAsync("Password Reset", $"New password: {r.Data?.PlainPassword}");
            else SetError(r.Message);
        }

        // Only active records can be deleted
        [RelayCommand]
        private async Task DeleteAsync(StudentItem s)
        {
            if (!s.IsActive) return;
            if (!await ConfirmAsync("Delete Student", $"Delete '{s.FullName}'?")) return;
            var r = await _students.DeleteAsync(s.StudentId);
            if (r.Success) { Items.Remove(s); await ShowToastAsync("Student deleted."); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private void Filter(string filter) => SelectedFilter = filter;
        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}