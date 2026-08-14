namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentDatesheetViewModel : BaseViewModel
    {
        private readonly IExamService _examService;

        [ObservableProperty] private List<ExamTermItem> _examTerms = [];
        [ObservableProperty] private ExamTermItem? _selectedExamTerm;
        [ObservableProperty] private List<ExamScheduleItem> _examSchedules = [];
        [ObservableProperty] private bool _isEmpty;

        public StudentDatesheetViewModel(IAuthService auth, INavigationService nav, IExamService examService)
            : base(auth, nav)
        {
            _examService = examService;
            Title = "Exam Datesheet";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                ExamTerms = await _examService.GetExamTermsAsync();
                SelectedExamTerm = ExamTerms.FirstOrDefault(t => t.IsActive) ?? ExamTerms.FirstOrDefault();
                if (SelectedExamTerm != null)
                {
                    await LoadSchedulesAsync(SelectedExamTerm.ExamTermId);
                }
            });
        }

        partial void OnSelectedExamTermChanged(ExamTermItem? value)
        {
            if (value != null)
            {
                _ = LoadSchedulesAsync(value.ExamTermId);
            }
        }

        private async Task LoadSchedulesAsync(int examTermId)
        {
            await RunAsync(async () =>
            {
                ExamSchedules = await _examService.GetExamSchedulesAsync(examTermId);
                IsEmpty = ExamSchedules.Count == 0;
            });
        }
    }
}
