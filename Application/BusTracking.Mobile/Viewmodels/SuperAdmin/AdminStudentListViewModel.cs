namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminStudentListViewModel : BaseViewModel
    {
        private readonly IStudentService _students;

        [ObservableProperty] private ObservableCollection<StudentItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _canLoadMore;

        public bool CanAdd => Can("student.add");
        public bool CanEdit => Can("student.edit");
        public bool CanDelete => Can("student.delete");

        public AdminStudentListViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "Students"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _students.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null);
                Items = new ObservableCollection<StudentItem>(data);
                IsEmpty = !Items.Any();
                CanLoadMore = data.Count == 20;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminStudentForm");
        [RelayCommand]
        private Task EditAsync(StudentItem s) =>
            Nav.GoToAsync("AdminStudentForm", new Dictionary<string, object> { ["StudentId"] = s.StudentId });
        [RelayCommand]
        private Task ViewAsync(StudentItem s) =>
            Nav.GoToAsync("AdminStudentDetail", new Dictionary<string, object> { ["StudentId"] = s.StudentId });

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
            if (r.Success) await ShowAlertAsync("Password Reset", r.Message);
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(StudentItem s)
        {
            if (!await ConfirmAsync("Delete Student", $"Delete '{s.FullName}'?")) return;
            var r = await _students.DeleteAsync(s.StudentId);
            if (r.Success) { Items.Remove(s); await ShowToastAsync("Student deleted."); }
            else SetError(r.Message);
        }
    }
}