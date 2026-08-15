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
        [ObservableProperty] private int _totalStudentsCount = 0;
        [ObservableProperty] private bool _hasStudents = false;

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

        [RelayCommand]
        private void OpenDateCalendar()
        {
            IsCalendarOpen = true;
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
            HasStudents = Students.Count > 0;
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
                    Status = s.Status
                }).ToList()
            };

            IsBusy = true;
            try
            {
                var res = await _attendanceService.SaveManualAttendanceBatchAsync(dto);
                if (res.Success)
                {
                    await ShowAlertAsync("Success", "Classroom attendance saved successfully.");
                    await FetchStudentsAsync();
                }
                else
                {
                    await ShowAlertAsync("Error", res.Message ?? "Failed to save attendance.");
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task StartFaceScanAsync()
        {
            if (SelectedYear == null || SelectedStandard == null) return;

            try
            {
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                {
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                }

                if (cameraStatus != PermissionStatus.Granted)
                {
                    await ShowAlertAsync("Permission Required", "Camera permission is required to capture student photo for face recognition attendance.");
                    return;
                }

                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo == null) return;

                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Photo = Convert.ToBase64String(imageBytes);

                int yearId = SelectedYear.AcademicYearId;
                int stdId = SelectedStandard.StandardId;
                int? secId = SelectedSection?.SectionId > 0 ? SelectedSection.SectionId : null;

                var req = new FaceAttendanceScanRequestDto
                {
                    AcademicYearId = yearId,
                    StandardId = stdId,
                    SectionId = secId,
                    AttendanceDate = SelectedDate,
                    Base64CapturedPhoto = base64Photo
                };

                IsBusy = true;
                var res = await _attendanceService.ProcessFaceScanAttendanceAsync(req);
                if (res.Success)
                {
                    await ShowAlertAsync("Face Scan Completed", res.Message ?? "Attendance updated via Face Matching.");
                    await FetchStudentsAsync();
                }
                else
                {
                    await ShowAlertAsync("Face Scan Warning", res.Message ?? "No face match found.");
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Face recognition error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
