namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordAttendanceViewModel : BaseViewModel
    {
        private readonly IAttendanceMobileService _attendanceService;
        private readonly IAcademicYearService _academicYearService;
        private readonly ICoordStandardService _standardService;
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

        public CoordAttendanceViewModel(
            IAuthService auth,
            INavigationService nav,
            IAttendanceMobileService attendanceService,
            IAcademicYearService academicYearService,
            ICoordStandardService standardService,
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
                var years = await _academicYearService.GetAcademicYearsAsync(isAdmin: false);
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
            var secs = await _sectionService.GetByStandardAsync(standardId, isCoordinator: true);
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

            var res = await _attendanceService.GetAttendanceReportAsync(
                SelectedYear.AcademicYearId,
                SelectedStandard.StandardId,
                SelectedSection?.SectionId > 0 ? SelectedSection.SectionId : null,
                SelectedDate,
                isCoordinator: true);

            if (res.Success && res.Data != null)
            {
                TotalStudentsCount = res.Data.TotalStudents;
                PresentCount = res.Data.PresentCount;
                AbsentCount = res.Data.AbsentCount;
                LateCount = res.Data.LateCount;
                AttendanceRate = $"{res.Data.PresentPercentage:F1}%";
                Students = new ObservableCollection<StudentAttendanceRowDto>(res.Data.AttendanceList ?? new());
            }
            else
            {
                TotalStudentsCount = 0;
                PresentCount = 0;
                AbsentCount = 0;
                LateCount = 0;
                AttendanceRate = "0.0%";
                Students = new ObservableCollection<StudentAttendanceRowDto>();
            }
            IsEmpty = !Students.Any();
        }
    }
}
