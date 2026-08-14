namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    [QueryProperty(nameof(ExamSchedule), "ExamSchedule")]
    public partial class ExamScheduleFormViewModel : BaseViewModel
    {
        private readonly IExamService _examService;
        private readonly IAdminStandardService _standardService;
        private readonly ISectionService _sectionService;
        private readonly ISubjectService _subjectService;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty]
        private ExamScheduleItem? _examSchedule;

        [ObservableProperty]
        private ObservableCollection<AcademicYearLookupItem> _academicYears = [];

        [ObservableProperty]
        private AcademicYearLookupItem? _selectedAcademicYear;

        [ObservableProperty]
        private ObservableCollection<ExamTermItem> _examTerms = [];

        [ObservableProperty]
        private ExamTermItem? _selectedExamTerm;

        [ObservableProperty]
        private ObservableCollection<StandardItem> _standards = [];

        [ObservableProperty]
        private StandardItem? _selectedStandard;

        [ObservableProperty]
        private ObservableCollection<SectionItem> _sections = [];

        [ObservableProperty]
        private SectionItem? _selectedSection;

        [ObservableProperty]
        private ObservableCollection<SubjectLookupItem> _subjects = [];

        [ObservableProperty]
        private SubjectLookupItem? _selectedSubject;

        [ObservableProperty]
        private DateTime _examDate = DateTime.Today;

        [ObservableProperty]
        private bool _isExamDateCalendarOpen;

        [ObservableProperty]
        private TimeSpan _startTime = new TimeSpan(9, 0, 0);

        [ObservableProperty]
        private TimeSpan _endTime = new TimeSpan(12, 0, 0);

        [ObservableProperty]
        private string _maxMarksText = "100";

        [ObservableProperty]
        private string _passMarksText = "33";

        [ObservableProperty]
        private string _roomNumber = "Room: TBA";

        [ObservableProperty]
        private bool _isSaving;

        public ExamScheduleFormViewModel(
            IAuthService auth,
            INavigationService nav,
            IExamService examService,
            IAdminStandardService standardService,
            ISectionService sectionService,
            ISubjectService subjectService,
            IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _examService = examService;
            _standardService = standardService;
            _sectionService = sectionService;
            _subjectService = subjectService;
            _academicYearService = academicYearService;
            Title = "Add Exam Schedule";
        }

        public override async Task InitializeAsync()
        {
            await LoadAcademicYearsAsync();
            await LoadStandardsAsync();
            await LoadSubjectsAsync();

            if (ExamSchedule != null)
            {
                Title = "Edit Exam Schedule";
                ExamDate = ExamSchedule.ExamDate;
                StartTime = ExamSchedule.StartTime;
                EndTime = ExamSchedule.EndTime;
                MaxMarksText = ExamSchedule.MaxMarks.ToString();
                PassMarksText = ExamSchedule.PassMarks.ToString();
                RoomNumber = ExamSchedule.RoomNumber ?? "";

                if (SelectedAcademicYear != null)
                {
                    await LoadExamTermsAsync(SelectedAcademicYear.AcademicYearId);
                }
                SelectedExamTerm = ExamTerms.FirstOrDefault(t => t.ExamTermId == ExamSchedule.ExamTermId);

                SelectedStandard = Standards.FirstOrDefault(s => s.StandardId == ExamSchedule.StandardId);
                if (SelectedStandard != null)
                {
                    await LoadSectionsAsync(SelectedStandard.StandardId);
                    SelectedSection = Sections.FirstOrDefault(sec => sec.SectionId == ExamSchedule.SectionId);
                }

                SelectedSubject = Subjects.FirstOrDefault(sub => sub.SubjectId == ExamSchedule.SubjectId);
            }
        }

        [RelayCommand]
        private void OpenExamDateCalendar()
        {
            IsExamDateCalendarOpen = true;
        }

        private async Task LoadAcademicYearsAsync()
        {
            try
            {
                var years = await _academicYearService.GetAcademicYearsAsync();
                var items = years.Select(y => new AcademicYearLookupItem
                {
                    AcademicYearId = y.AcademicYearId,
                    YearName = y.YearName,
                    IsCurrent = y.IsCurrent
                }).ToList();

                AcademicYears = new ObservableCollection<AcademicYearLookupItem>(items);
                SelectedAcademicYear ??= items.FirstOrDefault(y => y.IsCurrent) ?? items.FirstOrDefault();
            }
            catch { }
        }

        private async Task LoadExamTermsAsync(int academicYearId)
        {
            try
            {
                var terms = await _examService.GetExamTermsAsync(academicYearId);
                ExamTerms = new ObservableCollection<ExamTermItem>(terms);
                SelectedExamTerm ??= ExamTerms.FirstOrDefault();
            }
            catch { }
        }

        private async Task LoadStandardsAsync()
        {
            try
            {
                var paged = await _standardService.GetAllAsync(null, 1);
                Standards = new ObservableCollection<StandardItem>(paged.Items);
                SelectedStandard ??= Standards.FirstOrDefault();
            }
            catch { }
        }

        private async Task LoadSectionsAsync(int standardId)
        {
            try
            {
                var secs = await _sectionService.GetByStandardAsync(standardId);
                Sections = new ObservableCollection<SectionItem>(secs);
            }
            catch { }
        }

        private async Task LoadSubjectsAsync()
        {
            try
            {
                var paged = await _subjectService.GetAllAsync();
                var items = new List<SubjectLookupItem>();
                foreach (var s in paged.Items)
                {
                    items.Add(new SubjectLookupItem
                    {
                        SubjectId = s.SubjectId,
                        SubjectName = s.SubjectName
                    });
                }
                Subjects = new ObservableCollection<SubjectLookupItem>(items);
                SelectedSubject ??= Subjects.FirstOrDefault();
            }
            catch { }
        }

        partial void OnSelectedAcademicYearChanged(AcademicYearLookupItem? value)
        {
            if (value != null)
            {
                _ = LoadExamTermsAsync(value.AcademicYearId);
            }
        }

        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            if (value != null)
            {
                _ = LoadSectionsAsync(value.StandardId);
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (SelectedAcademicYear == null || SelectedExamTerm == null || SelectedStandard == null || SelectedSubject == null)
            {
                HasError = true;
                ErrorMessage = "Please select Academic Year, Exam Term, Standard, and Subject.";
                return;
            }

            if (!decimal.TryParse(MaxMarksText, out var maxMarks) || !decimal.TryParse(PassMarksText, out var passMarks))
            {
                HasError = true;
                ErrorMessage = "Invalid Max Marks or Pass Marks.";
                return;
            }

            IsSaving = true;
            HasError = false;

            try
            {
                if (ExamSchedule == null)
                {
                    var req = new CreateExamScheduleRequest
                    {
                        AcademicYearId = SelectedAcademicYear.AcademicYearId,
                        ExamTermId = SelectedExamTerm.ExamTermId,
                        StandardId = SelectedStandard.StandardId,
                        SectionId = SelectedSection?.SectionId,
                        SubjectId = SelectedSubject.SubjectId,
                        ExamDate = ExamDate,
                        StartTime = StartTime,
                        EndTime = EndTime,
                        MaxMarks = maxMarks,
                        PassMarks = passMarks,
                        RoomNumber = RoomNumber?.Trim()
                    };
                    var res = await _examService.CreateExamScheduleAsync(req);
                    if (res.Success)
                    {
                        await Nav.GoBackAsync();
                    }
                    else
                    {
                        HasError = true;
                        ErrorMessage = res.Message;
                    }
                }
                else
                {
                    var req = new UpdateExamScheduleRequest
                    {
                        ExamTermId = SelectedExamTerm.ExamTermId,
                        StandardId = SelectedStandard.StandardId,
                        SectionId = SelectedSection?.SectionId,
                        SubjectId = SelectedSubject.SubjectId,
                        ExamDate = ExamDate,
                        StartTime = StartTime,
                        EndTime = EndTime,
                        MaxMarks = maxMarks,
                        PassMarks = passMarks,
                        RoomNumber = RoomNumber?.Trim()
                    };
                    var res = await _examService.UpdateExamScheduleAsync(ExamSchedule.ExamScheduleId, req);
                    if (res.Success)
                    {
                        await Nav.GoBackAsync();
                    }
                    else
                    {
                        HasError = true;
                        ErrorMessage = res.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Nav.GoBackAsync();
        }
    }
}
