namespace BusTracking.Mobile.Viewmodels.Teacher
{
    public partial class TeacherAttendanceViewModel : BaseViewModel
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
        [ObservableProperty] private int _presentCount = 0;
        [ObservableProperty] private int _absentCount = 0;
        [ObservableProperty] private int _lateCount = 0;
        [ObservableProperty] private int _totalStudentsCount = 0;

        [ObservableProperty] private string _statusBannerMessage = "📝 Mark attendance manually or tap 📷 Face Scan";
        [ObservableProperty] private bool _hasExistingAttendance = false;

        [ObservableProperty] private ObservableCollection<StudentAttendanceRowDto> _students = new();

        public TeacherAttendanceViewModel(
            IAuthService auth,
            INavigationService nav,
            IAttendanceMobileService attendanceService,
            IAcademicYearService academicYearService,
            IAdminStandardService standardService,
            ISectionService sectionService)
            : base(auth, nav)
        {
            Title = "Mark Classroom Attendance";
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
                if (stds != null && stds.Items != null)
                {
                    Standards = new ObservableCollection<StandardItem>(stds.Items);
                    SelectedStandard = Standards.FirstOrDefault();
                    if (SelectedStandard != null)
                    {
                        await LoadSectionsAsync(SelectedStandard.StandardId);
                    }
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
            _ = LoadStudentsAsync();
        }

        partial void OnSelectedYearChanged(AcademicYearItem? value)
        {
            _ = LoadStudentsAsync();
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            _ = LoadStudentsAsync();
        }

        private async Task LoadSectionsAsync(int standardId)
        {
            var secs = await _sectionService.GetByStandardAsync(standardId, isAdmin: false);
            Sections = new ObservableCollection<SectionItem>(secs ?? new());
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
        public async Task LoadStudentsAsync()
        {
            IsBusy = true;
            try
            {
                await FetchStudentsAsync();
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        private async Task FetchStudentsAsync()
        {
            if (SelectedYear == null || SelectedStandard == null) return;

            int yearId = SelectedYear.AcademicYearId;
            int stdId = SelectedStandard.StandardId;
            int? secId = SelectedSection?.SectionId > 0 ? SelectedSection.SectionId : null;

            var list = await _attendanceService.GetStudentsForAttendanceAsync(yearId, stdId, secId, SelectedDate);
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
            foreach (var s in Students)
            {
                s.Status = "Present";
            }
            UpdateCounts();
        }

        private void UpdateCounts()
        {
            TotalStudentsCount = Students.Count;
            PresentCount = Students.Count(s => s.Status == "Present");
            AbsentCount = Students.Count(s => s.Status == "Absent");
            LateCount = Students.Count(s => s.Status == "Late");

            HasExistingAttendance = Students.Any(s => s.IsFaceScanned);
            if (HasExistingAttendance)
            {
                StatusBannerMessage = "ℹ️ Attendance Recorded — Tap any student to modify & update";
            }
            else
            {
                StatusBannerMessage = "📝 Mark attendance manually or tap 📷 Face Scan";
            }
        }

        [RelayCommand]
        private async Task SaveAttendanceAsync()
        {
            if (!Students.Any())
            {
                await ShowAlertAsync("Warning", "No students loaded for attendance.");
                return;
            }

            int yearId = SelectedYear?.AcademicYearId ?? 1;
            int stdId = SelectedStandard?.StandardId ?? 1;
            int? secId = SelectedSection?.SectionId > 0 ? SelectedSection.SectionId : null;

            var dto = new ManualAttendanceBatchDto
            {
                AcademicYearId = yearId,
                StandardId = stdId,
                SectionId = secId,
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
                var res = await _attendanceService.SaveManualAttendanceBatchAsync(dto);
                if (res.Success)
                {
                    await ShowToastAsync("Attendance saved successfully!");
                    await FetchStudentsAsync();
                }
                else
                {
                    SetError(res.Message ?? "Failed to save attendance.");
                }
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task StartFaceScanAsync()
        {
            try
            {
                PermissionStatus cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                }

                FileResult? photo = null;
                if (cameraStatus == PermissionStatus.Granted && MediaPicker.Default.IsCaptureSupported)
                {
                    photo = await MediaPicker.Default.CapturePhotoAsync();
                }

                if (photo == null)
                {
                    photo = await MediaPicker.Default.PickPhotoAsync();
                }

                if (photo == null) return;

                IsBusy = true;
                try
                {
                    using var stream = await photo.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var bytes = ms.ToArray();
                    var base64 = Convert.ToBase64String(bytes);

                    int yearId = SelectedYear?.AcademicYearId ?? 1;
                    int stdId = SelectedStandard?.StandardId ?? 1;
                    int? secId = SelectedSection?.SectionId > 0 ? SelectedSection.SectionId : null;

                    var req = new FaceAttendanceScanRequestDto
                    {
                        AcademicYearId = yearId,
                        StandardId = stdId,
                        SectionId = secId,
                        AttendanceDate = SelectedDate,
                        Base64Image = base64
                    };

                    var res = await _attendanceService.ProcessFaceScanAttendanceAsync(req);
                    if (res.Success && res.Data != null)
                    {
                        Students = new ObservableCollection<StudentAttendanceRowDto>(res.Data.AllClassStudents ?? new());
                        UpdateCounts();
                        await ShowAlertAsync("Face Scan Complete", $"Scanned {res.Data.TotalFacesDetected} face(s). Matched: {res.Data.MatchedCount}.");
                    }
                    else
                    {
                        await ShowAlertAsync("Face Scan Result", res.Message ?? "No face matches found.");
                    }
                }
                finally { IsBusy = false; }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Camera Error", $"Unable to process camera scan: {ex.Message}");
            }
        }
    }
}
