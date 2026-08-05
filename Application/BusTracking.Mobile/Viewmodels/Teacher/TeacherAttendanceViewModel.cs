namespace BusTracking.Mobile.Viewmodels.Teacher
{
    public partial class TeacherAttendanceViewModel : BaseViewModel
    {
        private readonly IAttendanceMobileService _attendanceService;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private int _selectedAcademicYearId = 1;
        [ObservableProperty] private int _selectedStandardId = 1;
        [ObservableProperty] private int _selectedSectionId = 1;
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
        [ObservableProperty] private bool _isFaceScanMode = false;
        [ObservableProperty] private int _presentCount = 0;
        [ObservableProperty] private int _absentCount = 0;
        [ObservableProperty] private int _totalStudentsCount = 0;

        [ObservableProperty] private ObservableCollection<StudentAttendanceRowDto> _students = new();

        public TeacherAttendanceViewModel(IAuthService auth, INavigationService nav, IAttendanceMobileService attendanceService, IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            Title = "Classroom Daily Attendance";
            _attendanceService = attendanceService;
            _academicYearService = academicYearService;
        }

        public override async Task InitializeAsync()
        {
            await LoadStudentsAsync();
        }

        [RelayCommand]
        public async Task LoadStudentsAsync()
        {
            await RunAsync(async () =>
            {
                var list = await _attendanceService.GetStudentsForAttendanceAsync(SelectedAcademicYearId, SelectedStandardId, SelectedSectionId, SelectedDate);
                Students = new ObservableCollection<StudentAttendanceRowDto>(list);
                UpdateCounts();
            });
        }

        [RelayCommand]
        private void ToggleStatus(StudentAttendanceRowDto student)
        {
            if (student == null) return;
            student.Status = student.Status switch
            {
                "Present" => "Absent",
                "Absent" => "Late",
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
        }

        [RelayCommand]
        private async Task SaveAttendanceAsync()
        {
            if (!Students.Any())
            {
                await ShowToastAsync("No students loaded for attendance.");
                return;
            }

            await RunAsync(async () =>
            {
                var dto = new ManualAttendanceBatchDto
                {
                    AcademicYearId = SelectedAcademicYearId,
                    StandardId = SelectedStandardId,
                    SectionId = SelectedSectionId,
                    Date = SelectedDate,
                    Items = Students.Select(s => new StudentAttendanceItemDto
                    {
                        StudentId = s.StudentId,
                        Status = s.Status
                    }).ToList()
                };

                var res = await _attendanceService.SaveManualAttendanceBatchAsync(dto);
                if (res.Success)
                {
                    await ShowToastAsync("Attendance saved successfully!");
                }
                else
                {
                    await ShowToastAsync(res.Message ?? "Failed to save attendance.");
                }
            });
        }

        [RelayCommand]
        private async Task StartFaceScanAsync()
        {
            try
            {
                FileResult? photo = null;
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    photo = await MediaPicker.Default.CapturePhotoAsync();
                }
                else
                {
                    photo = await MediaPicker.Default.PickPhotoAsync();
                }

                if (photo == null) return;

                await RunAsync(async () =>
                {
                    using var stream = await photo.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var bytes = ms.ToArray();
                    var base64 = Convert.ToBase64String(bytes);

                    var req = new FaceAttendanceScanRequestDto
                    {
                        AcademicYearId = SelectedAcademicYearId,
                        StandardId = SelectedStandardId,
                        SectionId = SelectedSectionId,
                        Date = SelectedDate,
                        Base64Image = base64
                    };

                    var res = await _attendanceService.ProcessFaceScanAttendanceAsync(req);
                    if (res.Success && res.Data != null)
                    {
                        Students = new ObservableCollection<StudentAttendanceRowDto>(res.Data.AllClassStudents);
                        UpdateCounts();
                        await ShowToastAsync($"Face scan complete! Matched {res.Data.MatchedStudentsCount} student(s).");
                    }
                    else
                    {
                        await ShowToastAsync(res.Message ?? "Face scan matching failed.");
                    }
                });
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Camera scan error: {ex.Message}");
            }
        }
    }
}
