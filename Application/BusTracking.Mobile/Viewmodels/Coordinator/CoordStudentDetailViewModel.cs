namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordStudentDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IStudentService _students;
        [ObservableProperty] private int _studentId;
        [ObservableProperty] private StudentItem? _student;

        public bool CanEdit => Can("student.edit");
        public bool CanDelete => Can("student.delete");

        public CoordStudentDetailViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "Student Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("StudentId", out var id)) StudentId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                Student = await _students.GetByIdAsync(StudentId);
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CanDelete));
            });
        }

        [RelayCommand]
        private Task EditAsync()
        {
            if (!CanEdit) return Task.CompletedTask;
            return Nav.GoToAsync("CoordStudentForm", new Dictionary<string, object> { ["StudentId"] = StudentId });
        }

        [RelayCommand]
        private async Task ToggleAsync()
        {
            if (Student is null) return;
            var r = await _students.ToggleAsync(StudentId);
            if (r.Success) { await ShowToastAsync(r.Message); await InitializeAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!CanDelete) return;
            if (!await ConfirmAsync("Delete Student", $"Delete '{Student?.FullName}'?")) return;
            var r = await _students.DeleteAsync(StudentId);
            if (r.Success) { await ShowToastAsync("Student deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}