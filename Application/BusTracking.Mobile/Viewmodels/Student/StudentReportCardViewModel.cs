namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentReportCardViewModel : BaseViewModel
    {
        private readonly IExamService _examService;

        [ObservableProperty] private List<ExamTermItem> _examTerms = [];
        [ObservableProperty] private ExamTermItem? _selectedExamTerm;
        [ObservableProperty] private StudentReportCardItem? _reportCard;
        [ObservableProperty] private bool _hasReportCard;
        [ObservableProperty] private bool _isEmpty;

        public StudentReportCardViewModel(IAuthService auth, INavigationService nav, IExamService examService)
            : base(auth, nav)
        {
            _examService = examService;
            Title = "Report Card";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                ExamTerms = await _examService.GetExamTermsAsync();
                SelectedExamTerm = ExamTerms.FirstOrDefault(t => t.IsActive) ?? ExamTerms.FirstOrDefault();
                if (SelectedExamTerm != null)
                {
                    await LoadReportCardAsync(SelectedExamTerm.ExamTermId);
                }
            });
        }

        partial void OnSelectedExamTermChanged(ExamTermItem? value)
        {
            if (value != null)
            {
                _ = LoadReportCardAsync(value.ExamTermId);
            }
        }

        private async Task LoadReportCardAsync(int examTermId)
        {
            await RunAsync(async () =>
            {
                ReportCard = await _examService.GetStudentReportCardAsync(examTermId);
                HasReportCard = ReportCard != null;
                IsEmpty = !HasReportCard;
            });
        }
    }
}
