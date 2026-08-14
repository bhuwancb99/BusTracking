namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordExamDatesheetViewModel : BaseViewModel
    {
        private readonly IExamService _examService;
        private readonly ICoordStandardService _standardService;

        [ObservableProperty]
        private ObservableCollection<ExamScheduleItem> _schedules = [];

        [ObservableProperty]
        private ObservableCollection<ExamTermItem> _examTerms = [];

        [ObservableProperty]
        private ExamTermItem? _selectedExamTerm;

        [ObservableProperty]
        private ObservableCollection<StandardItem> _standards = [];

        [ObservableProperty]
        private StandardItem? _selectedStandard;

        public CoordExamDatesheetViewModel(IAuthService auth, INavigationService nav, IExamService examService, ICoordStandardService standardService)
            : base(auth, nav)
        {
            _examService = examService;
            _standardService = standardService;
            Title = "Exam Datesheets";
        }

        public override async Task InitializeAsync()
        {
            await LoadExamTermsAsync();
            await LoadStandardsAsync();
            await LoadSchedulesAsync();
        }

        public override async Task RefreshOnReturnAsync()
        {
            await LoadSchedulesAsync();
        }

        private async Task LoadExamTermsAsync()
        {
            try
            {
                var terms = await _examService.GetExamTermsAsync();
                ExamTerms = new ObservableCollection<ExamTermItem>(terms);
                SelectedExamTerm ??= terms.FirstOrDefault();
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

        [RelayCommand]
        private async Task LoadSchedulesAsync()
        {
            IsRefreshing = true;
            HasError = false;
            try
            {
                int? termId = SelectedExamTerm?.ExamTermId;
                int? stdId = SelectedStandard?.StandardId;

                var list = await _examService.GetExamSchedulesAsync(termId, stdId);
                Schedules = new ObservableCollection<ExamScheduleItem>(list);
                IsEmpty = !Schedules.Any();
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
            if (value != null && SelectedStandard == null)
            {
                SelectedStandard = Standards.FirstOrDefault();
            }
            _ = LoadSchedulesAsync();
        }

        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            _ = LoadSchedulesAsync();
        }

        [RelayCommand]
        private async Task AddScheduleAsync()
        {
            await Nav.GoToAsync("CoordExamScheduleForm");
        }

        [RelayCommand]
        private async Task EditScheduleAsync(ExamScheduleItem item)
        {
            if (item == null) return;
            var param = new Dictionary<string, object> { { "ExamSchedule", item } };
            await Nav.GoToAsync("CoordExamScheduleForm", param);
        }

        [RelayCommand]
        private async Task DeleteScheduleAsync(ExamScheduleItem item)
        {
            if (item == null) return;
            if (!await ConfirmAsync("Delete Exam Schedule", $"Are you sure you want to delete the schedule for '{item.SubjectName}'?")) return;

            var res = await _examService.DeleteExamScheduleAsync(item.ExamScheduleId);
            if (res.Success)
            {
                Schedules.Remove(item);
                IsEmpty = !Schedules.Any();
            }
            else
            {
                SetError(res.Message);
            }
        }
    }
}
