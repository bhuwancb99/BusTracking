namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordStudentDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IStudentService _students;
        [ObservableProperty] private int _studentId;
        [ObservableProperty] private StudentItem? _student;

        public CoordStudentDetailViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "Student Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("StudentId", out var id)) StudentId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () => { Student = await _students.GetByIdAsync(StudentId); });
        }

        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("CoordStudentForm", new Dictionary<string, object> { ["StudentId"] = StudentId });
    }
}
