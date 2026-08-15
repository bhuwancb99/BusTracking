namespace BusTracking.Mobile.Viewmodels.Teacher
{
    public partial class TeacherDatesheetViewModel : BaseViewModel
    {
        private readonly IExamService _examService;
        private readonly IAdminStandardService _standardService;

        [ObservableProperty]
        private ObservableCollection<ExamTermItem> _examTerms = [];

        [ObservableProperty]
        private ExamTermItem? _selectedExamTerm;

        [ObservableProperty]
        private ObservableCollection<StandardItem> _standards = [];

        [ObservableProperty]
        private StandardItem? _selectedStandard;

        [ObservableProperty]
        private ObservableCollection<ExamScheduleItem> _schedules = [];

        [ObservableProperty]
        private bool _hasSchedules;

        public TeacherDatesheetViewModel(IAuthService auth, INavigationService nav, IExamService examService, IAdminStandardService standardService)
            : base(auth, nav)
        {
            _examService = examService;
            _standardService = standardService;
            Title = "Exam Datesheets";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var terms = await _examService.GetExamTermsAsync();
                ExamTerms = new ObservableCollection<ExamTermItem>(terms);
                SelectedExamTerm = ExamTerms.FirstOrDefault(t => t.IsActive) ?? ExamTerms.FirstOrDefault();

                var stdRes = await _standardService.GetAllAsync();
                Standards = new ObservableCollection<StandardItem>(stdRes?.Items ?? []);
                SelectedStandard = Standards.FirstOrDefault();

                await LoadSchedulesAsync();
            });
        }

        [RelayCommand]
        public async Task LoadSchedulesAsync()
        {
            IsRefreshing = true;
            HasError = false;
            try
            {
                int? termId = SelectedExamTerm?.ExamTermId;
                int? stdId = SelectedStandard?.StandardId;

                var list = await _examService.GetExamSchedulesAsync(termId, stdId);
                Schedules = new ObservableCollection<ExamScheduleItem>(list);
                HasSchedules = Schedules.Any();
                IsEmpty = !HasSchedules;
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        partial void OnSelectedExamTermChanged(ExamTermItem? value)
        {
            if (value != null)
            {
                _ = LoadSchedulesAsync();
            }
        }

        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            if (value != null)
            {
                _ = LoadSchedulesAsync();
            }
        }
    }
}
