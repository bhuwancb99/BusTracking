namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminAttendanceViewModel : BaseViewModel
    {
        private readonly IAttendanceMobileService _attendanceService;
        private readonly IAcademicYearService _academicYearService;
        private readonly IAdminStandardService _standardService;
        private readonly ISectionService _sectionService;

        [ObservableProperty] private ObservableCollection<AcademicYearItem> _academicYears = [];
        [ObservableProperty] private AcademicYearItem? _selectedYear;
        [ObservableProperty] private ObservableCollection<StandardItem> _standards = [];
        [ObservableProperty] private StandardItem? _selectedStandard;
        [ObservableProperty] private ObservableCollection<SectionItem> _sections = [];
        [ObservableProperty] private SectionItem? _selectedSection;
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
        [ObservableProperty] private bool _isCalendarOpen;

        [ObservableProperty] private ObservableCollection<StudentAttendanceRowDto> _students = [];
        [ObservableProperty] private int _totalStudentsCount;
        [ObservableProperty] private int _presentCount;
        [ObservableProperty] private int _absentCount;
        [ObservableProperty] private int _lateCount;
        [ObservableProperty] private string _attendanceRate = "0.0%";

        public AdminAttendanceViewModel(
            IAuthService auth,
            INavigationService nav,
            IAttendanceMobileService attendanceService,
            IAcademicYearService academicYearService,
            IAdminStandardService standardService,
            ISectionService sectionService)
            : base(auth, nav)
        {
            Title = "Classroom Daily Attendance";
            _attendanceService = attendanceService;
            _academicYearService = academicYearService;
            _standardService = standardService;
            _sectionService = sectionService;
        }

        public override async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                var years = await _academicYearService.GetAcademicYearsAsync(isAdmin: true);
                AcademicYears = new ObservableCollection<AcademicYearItem>(years);
                SelectedYear = AcademicYears.FirstOrDefault(y => y.IsCurrent) ?? AcademicYears.FirstOrDefault();

                var stds = await _standardService.GetAllAsync(null, 1);
                Standards = new ObservableCollection<StandardItem>(stds.Items);
                SelectedStandard = Standards.FirstOrDefault();

                if (SelectedStandard != null)
                {
                    await LoadSectionsAsync(SelectedStandard.StandardId);
                }

                await FetchStudentsAsync();
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            if (value != null) _ = LoadSectionsAndStudentsAsync(value.StandardId);
        }

        private async Task LoadSectionsAsync(int standardId)
        {
            var secList = new List<SectionItem> { new SectionItem { SectionId = 0, SectionName = "-- All Sections --" } };
            var secs = await _sectionService.GetByStandardAsync(standardId, isAdmin: true);
            if (secs != null) secList.AddRange(secs);
            Sections = new ObservableCollection<SectionItem>(secList);
            SelectedSection = Sections.FirstOrDefault();
        }

        private async Task LoadSectionsAndStudentsAsync(int standardId)
        {
            IsBusy = true;
            try
            {
                await LoadSectionsAsync(standardId);
                await FetchStudentsAsync();
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand] private void OpenCalendar() => IsCalendarOpen = true;
        [RelayCommand] private void CloseCalendar() => IsCalendarOpen = false;

        [RelayCommand]
        private async Task FetchAttendanceAsync()
        {
            IsBusy = true;
            try { await FetchStudentsAsync(); }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task ResetFiltersAsync()
        {
            SelectedDate = DateTime.Today;
            SelectedYear = AcademicYears.FirstOrDefault(y => y.IsCurrent) ?? AcademicYears.FirstOrDefault();
            SelectedStandard = Standards.FirstOrDefault();
            if (SelectedStandard != null) await LoadSectionsAsync(SelectedStandard.StandardId);
            await FetchAttendanceAsync();
        }

        private async Task FetchStudentsAsync()
        {
            if (SelectedYear is null || SelectedStandard is null) return;

            var list = await _attendanceService.GetStudentsForAttendanceAsync(
                SelectedYear.AcademicYearId,
                SelectedStandard.StandardId,
                SelectedSection?.SectionId > 0 ? SelectedSection.SectionId : null,
                SelectedDate,
                isAdmin: true);

            Students = new ObservableCollection<StudentAttendanceRowDto>(list);
            IsEmpty = !Students.Any();
            UpdateCounts();
        }

        private void UpdateCounts()
        {
            TotalStudentsCount = Students.Count;
            PresentCount = Students.Count(s => s.Status == "Present");
            AbsentCount = Students.Count(s => s.Status == "Absent");
            LateCount = Students.Count(s => s.Status == "Late");

            if (TotalStudentsCount > 0)
            {
                double rate = ((double)PresentCount / TotalStudentsCount) * 100.0;
                AttendanceRate = $"{rate:F1}%";
            }
            else
            {
                AttendanceRate = "0.0%";
            }
        }
    }
}
