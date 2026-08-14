namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentExamsReportCardsViewModel : BaseViewModel
    {
        private readonly IExamService _examService;

        [ObservableProperty]
        private ObservableCollection<ExamTermItem> _examTerms = [];

        [ObservableProperty]
        private ExamTermItem? _selectedExamTerm;

        [ObservableProperty]
        private ObservableCollection<ExamScheduleItem> _examSchedules = [];

        [ObservableProperty]
        private StudentReportCardItem? _reportCard;

        [ObservableProperty]
        private bool _hasReportCard;

        [ObservableProperty]
        private bool _hasSchedules;

        [ObservableProperty]
        private bool _showFullReportCard;

        public StudentExamsReportCardsViewModel(IAuthService auth, INavigationService nav, IExamService examService)
            : base(auth, nav)
        {
            _examService = examService;
            Title = "Exams & Report Cards";
        }

        public override async Task InitializeAsync()
        {
            await LoadExamTermsAsync();
        }

        public override async Task RefreshOnReturnAsync()
        {
            if (SelectedExamTerm != null)
            {
                await LoadTermDataAsync();
            }
        }

        private async Task LoadExamTermsAsync()
        {
            await RunAsync(async () =>
            {
                var terms = await _examService.GetExamTermsAsync();
                ExamTerms = new ObservableCollection<ExamTermItem>(terms);
                SelectedExamTerm = ExamTerms.FirstOrDefault();
            });
        }

        partial void OnSelectedExamTermChanged(ExamTermItem? value)
        {
            if (value != null)
            {
                _ = LoadTermDataAsync();
            }
        }

        [RelayCommand]
        private async Task LoadTermDataAsync()
        {
            if (SelectedExamTerm == null) return;

            await RunAsync(async () =>
            {
                // Fetch Datesheet Schedules
                var schedules = await _examService.GetExamSchedulesAsync(SelectedExamTerm.ExamTermId);
                ExamSchedules = new ObservableCollection<ExamScheduleItem>(schedules);
                HasSchedules = ExamSchedules.Any();

                // Fetch Report Card
                try
                {
                    ReportCard = await _examService.GetStudentReportCardAsync(SelectedExamTerm.ExamTermId);
                    HasReportCard = ReportCard != null && (ReportCard.TotalMaxMarks > 0 || ReportCard.Subjects.Any());
                }
                catch
                {
                    ReportCard = null;
                    HasReportCard = false;
                }
            });
        }

        [RelayCommand]
        private async Task ViewFullReportCardAsync()
        {
            if (ReportCard == null) return;
            var param = new Dictionary<string, object> { { "ReportCard", ReportCard } };
            await Nav.GoToAsync("StudentFullReportCard", param);
        }
    }
}
