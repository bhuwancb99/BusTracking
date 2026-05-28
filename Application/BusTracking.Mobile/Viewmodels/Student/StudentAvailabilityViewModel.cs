namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentAvailabilityViewModel : BaseViewModel
    {
        private readonly IStudentService _students;

        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
        [ObservableProperty] private bool _morningAvailable = true;
        [ObservableProperty] private bool _eveningAvailable = true;
        [ObservableProperty] private string _reason = "";

        public DateTime MinimumDate => DateTime.Today;

        public StudentAvailabilityViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "My Availability"; }

        [RelayCommand]
        private async Task SaveAsync()
        {
            await RunAsync(async () =>
            {
                var req = new
                {
                    Date = SelectedDate,
                    MorningAvailable = MorningAvailable,
                    EveningAvailable = EveningAvailable,
                    Reason = Reason
                };
                var r = await _students.SetAvailabilityAsync(req);
                if (r.Success) await ShowToastAsync("Availability saved.");
                else SetError(r.Message);
            });
        }
    }
}
