namespace BusTracking.Mobile.Viewmodels.Teacher
{
    public partial class TeacherMarksEntryViewModel : BaseViewModel
    {
        private readonly IExamService _examService;
        private readonly IAdminStandardService _standardService;
        private readonly ISectionService _sectionService;

        [ObservableProperty] private List<ExamTermItem> _examTerms = [];
        [ObservableProperty] private ExamTermItem? _selectedExamTerm;
        [ObservableProperty] private List<StandardItem> _standards = [];
        [ObservableProperty] private StandardItem? _selectedStandard;
        [ObservableProperty] private List<SectionItem> _sections = [];
        [ObservableProperty] private SectionItem? _selectedSection;
        [ObservableProperty] private List<ExamScheduleItem> _examSchedules = [];
        [ObservableProperty] private ExamScheduleItem? _selectedSchedule;

        [ObservableProperty] private List<StudentMarksGridItem> _studentGrid = [];
        [ObservableProperty] private bool _hasGridData;
        [ObservableProperty] private bool _isEmpty;
        [ObservableProperty] private bool _isGridLoaded;

        // Feedback Banner
        [ObservableProperty] private bool _showSuccessBanner;
        [ObservableProperty] private string _successMessage = "";

        public TeacherMarksEntryViewModel(IAuthService auth, INavigationService nav,
            IExamService examService, IAdminStandardService standardService, ISectionService sectionService)
            : base(auth, nav)
        {
            _examService = examService;
            _standardService = standardService;
            _sectionService = sectionService;
            Title = "Marks Entry";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                ExamTerms = await _examService.GetExamTermsAsync();
                SelectedExamTerm = ExamTerms.FirstOrDefault(t => t.IsActive) ?? ExamTerms.FirstOrDefault();

                var stdRes = await _standardService.GetAllAsync();
                Standards = stdRes?.Items ?? [];
                SelectedStandard = Standards.FirstOrDefault();

                if (SelectedStandard != null)
                {
                    await LoadSectionsAndSchedulesAsync(SelectedStandard.StandardId);
                }
            });
        }


        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            if (value != null)
            {
                _ = LoadSectionsAndSchedulesAsync(value.StandardId);
            }
            else
            {
                Sections = [];
                SelectedSection = null;
                ExamSchedules = [];
                SelectedSchedule = null;
            }
        }

        partial void OnSelectedExamTermChanged(ExamTermItem? value)
        {
            if (SelectedStandard != null && value != null)
            {
                _ = LoadSchedulesAsync(value.ExamTermId, SelectedStandard.StandardId);
            }
        }

        private async Task LoadSectionsAndSchedulesAsync(int standardId)
        {
            var list = await _sectionService.GetByStandardAsync(standardId);
            Sections = list ?? new();
            SelectedSection = Sections.FirstOrDefault();

            if (SelectedExamTerm != null)
            {
                await LoadSchedulesAsync(SelectedExamTerm.ExamTermId, standardId);
            }
        }

        private async Task LoadSchedulesAsync(int examTermId, int standardId)
        {
            ExamSchedules = await _examService.GetExamSchedulesAsync(examTermId, standardId);
            SelectedSchedule = ExamSchedules.FirstOrDefault();
        }

        [RelayCommand]
        private async Task SearchGridAsync()
        {
            if (SelectedSchedule == null)
            {
                SetError("Please select Academic Term, Class, and Exam Subject.");
                return;
            }

            int? secId = (SelectedSection != null && SelectedSection.SectionId > 0) ? SelectedSection.SectionId : null;

            await RunAsync(async () =>
            {
                StudentGrid = await _examService.GetStudentMarksGridAsync(SelectedSchedule.ExamScheduleId, secId);
                if (StudentGrid != null)
                {
                    var max = SelectedSchedule.MaxMarks > 0 ? SelectedSchedule.MaxMarks : 100;
                    foreach (var item in StudentGrid)
                    {
                        item.MaxMarks = max;
                        item.RecalculateGrade();
                    }
                }

                IsGridLoaded = true;
                HasGridData = StudentGrid.Count > 0;
                IsEmpty = StudentGrid.Count == 0;
                ShowSuccessBanner = false;
            });
        }

        [RelayCommand]
        private async Task SaveMarksAsync()
        {
            if (SelectedSchedule == null || StudentGrid.Count == 0) return;

            await RunAsync(async () =>
            {
                var req = new SaveStudentMarksGridRequest
                {
                    ExamScheduleId = SelectedSchedule.ExamScheduleId,
                    Marks = StudentGrid.Select(g => new SaveStudentMarksItemRequest
                    {
                        StudentId = g.StudentId,
                        MarksObtained = g.MarksObtained ?? g.TotalObtained,
                        TheoryMarks = g.TheoryMarks,
                        PracticalMarks = g.PracticalMarks,
                        IsAbsent = g.IsAbsent,
                        Remarks = g.Remarks
                    }).ToList()
                };

                var r = await _examService.SaveStudentMarksGridAsync(req);
                if (r.Success)
                {
                    SuccessMessage = "Marks saved successfully!";
                    ShowSuccessBanner = true;
                    await Task.Delay(4000);
                    ShowSuccessBanner = false;
                }
                else
                {
                    SetError(r.Message);
                }
            });
        }
    }
}
