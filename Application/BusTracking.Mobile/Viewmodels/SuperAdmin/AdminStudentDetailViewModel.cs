namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminStudentDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IStudentService _students;

        [ObservableProperty] private int _studentId;
        [ObservableProperty] private StudentItem? _student;

        public AdminStudentDetailViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "Student Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("StudentId", out var id)) StudentId = (int)id;
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                Student = await _students.GetByIdAsync(StudentId);
            });
        }

        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("AdminStudentForm", new Dictionary<string, object> { ["StudentId"] = StudentId });

        [RelayCommand]
        private async Task ToggleAsync()
        {
            var r = await _students.ToggleAsync(StudentId);
            if (r.Success) { await ShowToastAsync(r.Message); await LoadAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {Student?.FullName}?")) return;
            var r = await _students.ResetPasswordAsync(StudentId);
            if (r.Success) await ShowAlertAsync("Password Reset", $"New password: {r.Data?.PlainPassword}");
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!await ConfirmAsync("Delete Student", $"Delete '{Student?.FullName}'?")) return;
            var r = await _students.DeleteAsync(StudentId);
            if (r.Success) { await ShowToastAsync("Student deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}

