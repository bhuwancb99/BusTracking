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
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private string _standard = "";
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private List<BusItem> _busOptions = [];
        [ObservableProperty] private List<StopItem> _stopOptions = [];
        [ObservableProperty] private BusItem? _selectedBus;
        [ObservableProperty] private StopItem? _selectedStop;

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
                BusOptions = await _buses.GetAllAsync();
                if (IsEditMode && StudentId.HasValue)
                {
                    var s = await _students.GetByIdAsync(StudentId.Value);
                    if (s is null) return;
                    FullName = s.FullName; Email = s.Email;
                    PhoneNumber = s.PhoneNumber ?? ""; Standard = s.Standard ?? "";
                    IsActive = s.IsActive;
                    SelectedBus = BusOptions.FirstOrDefault(b => b.BusId == s.BusId);
                    if (SelectedBus?.RouteId.HasValue == true)
                    {
                        StopOptions = await _routes.GetStopsAsync(SelectedBus.RouteId.Value);
                        SelectedStop = StopOptions.FirstOrDefault(st => st.StopId == s.StopId);
                    }
                }
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
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email))
            { SetError("Full name and email are required."); return; }
            if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
            { SetError("Password is required for new students."); return; }

            await RunAsync(async () =>
            {
                ApiResponse<object> r = IsEditMode
                    ? await _students.UpdateAsync(StudentId!.Value, new UpdateStudentRequest
                    {
                        FullName = FullName,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        Standard = Standard.Length > 0 ? Standard : null,
                        BusId = SelectedBus?.BusId,
                        StopId = SelectedStop?.StopId,
                        IsActive = IsActive
                    })
                    : await _students.CreateAsync(new CreateStudentRequest
                    {
                        FullName = FullName,
                        Email = Email,
                        Password = Password,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        Standard = Standard.Length > 0 ? Standard : null,
                        BusId = SelectedBus?.BusId,
                        StopId = SelectedStop?.StopId,
                        IsActive = IsActive
                    });

                if (r.Success) { await ShowToastAsync(IsEditMode ? "Student updated." : "Student created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
