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

        [ObservableProperty] private ObservableCollection<StudentAttendanceRowDto> _students = [];
        [ObservableProperty] private int _totalStudentsCount;
        [ObservableProperty] private int _presentCount;
        [ObservableProperty] private int _absentCount;
        [ObservableProperty] private int _lateCount;

        public AdminAttendanceViewModel(
            IAuthService auth,
            INavigationService nav,
            IAttendanceMobileService attendanceService,
            IAcademicYearService academicYearService,
            IAdminStandardService standardService,
            ISectionService sectionService)
            : base(auth, nav)
        {
            Title = "Daily Attendance";
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
                _selectedYear = AcademicYears.FirstOrDefault(y => y.IsCurrent) ?? AcademicYears.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedYear));

                var stds = await _standardService.GetAllAsync(null, 1);
                Standards = new ObservableCollection<StandardItem>(stds.Items);
                _selectedStandard = Standards.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedStandard));

                if (SelectedStandard != null)
                {
                    var secs = await _sectionService.GetByStandardAsync(SelectedStandard.StandardId, isAdmin: true);
                    Sections = new ObservableCollection<SectionItem>(secs);
                    _selectedSection = Sections.FirstOrDefault();
                    OnPropertyChanged(nameof(SelectedSection));
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

        partial void OnSelectedSectionChanged(SectionItem? value)
        {
            _ = LoadStudentsWithLoaderAsync();
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            _ = LoadStudentsWithLoaderAsync();
        }

        partial void OnSelectedYearChanged(AcademicYearItem? value)
        {
            _ = LoadStudentsWithLoaderAsync();
        }

        private async Task LoadSectionsAndStudentsAsync(int standardId)
        {
            IsBusy = true;
            try
            {
                var secs = await _sectionService.GetByStandardAsync(standardId, isAdmin: true);
                Sections = new ObservableCollection<SectionItem>(secs);
                _selectedSection = Sections.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedSection));

                await FetchStudentsAsync();
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        private async Task LoadStudentsWithLoaderAsync()
        {
            IsBusy = true;
            try { await FetchStudentsAsync(); }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        private async Task FetchStudentsAsync()
        {
            if (SelectedYear is null || SelectedStandard is null) return;

            var list = await _attendanceService.GetStudentsForAttendanceAsync(
                SelectedYear.AcademicYearId,
                SelectedStandard.StandardId,
                SelectedSection?.SectionId,
                SelectedDate,
                isAdmin: true);

            Students = new ObservableCollection<StudentAttendanceRowDto>(list);
            IsEmpty = !Students.Any();
            UpdateCounts();
        }

        [RelayCommand]
        private void ToggleStatus(StudentAttendanceRowDto student)
        {
            if (student == null) return;
            student.Status = student.Status switch
            {
                "Present" => "Absent",
                "Absent" => "Late",
                "Late" => "Present",
                _ => "Present"
            };
            UpdateCounts();
        }

        [RelayCommand]
        private void MarkAllPresent()
        {
            foreach (var s in Students) s.Status = "Present";
            UpdateCounts();
        }

        private void UpdateCounts()
        {
            TotalStudentsCount = Students.Count;
            PresentCount = Students.Count(s => s.Status == "Present");
            AbsentCount = Students.Count(s => s.Status == "Absent");
            LateCount = Students.Count(s => s.Status == "Late");
        }

        [RelayCommand]
        private async Task SaveAttendanceAsync()
        {
            if (SelectedYear is null || SelectedStandard is null)
            {
                await ShowAlertAsync("Error", "Please select Academic Year and Standard.");
                return;
            }

            if (!Students.Any())
            {
                await ShowAlertAsync("Warning", "No students available to mark attendance.");
                return;
            }

            var batch = new ManualAttendanceBatchDto
            {
                AcademicYearId = SelectedYear.AcademicYearId,
                StandardId = SelectedStandard.StandardId,
                SectionId = SelectedSection?.SectionId,
                AttendanceDate = SelectedDate,
                Records = Students.Select(s => new StudentAttendanceItemDto
                {
                    StudentId = s.StudentId,
                    Status = s.Status,
                    IsFaceScanned = s.IsFaceScanned,
                    MatchConfidence = s.MatchConfidence
                }).ToList()
            };

            IsBusy = true;
            try
            {
                var r = await _attendanceService.SaveManualAttendanceBatchAsync(batch, isAdmin: true);
                if (r.Success)
                    await ShowToastAsync("Attendance saved successfully!");
                else
                    SetError(r.Message);
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task StartFaceScanAsync()
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo == null) return;

                using var stream = await photo.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());

                if (SelectedYear == null || SelectedStandard == null) return;

                var req = new FaceAttendanceScanRequestDto
                {
                    AcademicYearId = SelectedYear.AcademicYearId,
                    StandardId = SelectedStandard.StandardId,
                    SectionId = SelectedSection?.SectionId,
                    AttendanceDate = SelectedDate,
                    Base64Image = base64
                };

                IsBusy = true;
                try
                {
                    var res = await _attendanceService.ProcessFaceScanAttendanceAsync(req, isAdmin: true);
                    if (res.Success && res.Data != null)
                    {
                        var scanResult = res.Data;
                        await ShowAlertAsync("Face Scan Result", $"Scanned {scanResult.TotalFacesDetected} face(s). Matched: {scanResult.MatchedCount}. Unmatched: {scanResult.UnmatchedCount}");
                        await FetchStudentsAsync();
                    }
                    else
                    {
                        SetError(res.Message);
                    }
                }
                finally { IsBusy = false; }
            }
            catch (Exception ex) { SetError(ex.Message); }
        }
    }
}
