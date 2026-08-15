namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordExamDatesheetViewModel : BaseViewModel
    {
        private readonly IExamService _examService;
        private readonly ICoordStandardService _standardService;
        private readonly ISectionService _sectionService;

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

        [ObservableProperty]
        private ObservableCollection<SectionItem> _sections = [];

        [ObservableProperty]
        private SectionItem? _selectedSection;

        [ObservableProperty] private bool _canAdd;
        [ObservableProperty] private bool _canEdit;
        [ObservableProperty] private bool _canDelete;

        public CoordExamDatesheetViewModel(IAuthService auth, INavigationService nav, IExamService examService, ICoordStandardService standardService, ISectionService sectionService)
            : base(auth, nav)
        {
            _examService = examService;
            _standardService = standardService;
            _sectionService = sectionService;
            Title = "Exam Datesheets";
        }

        public override async Task InitializeAsync()
        {
            CanAdd = Can("examschedule.add");
            CanEdit = Can("examschedule.edit");
            CanDelete = Can("examschedule.delete");

            await RunAsync(async () =>
            {
                var terms = await _examService.GetExamTermsAsync();
                ExamTerms = new ObservableCollection<ExamTermItem>(terms);
                SelectedExamTerm = ExamTerms.FirstOrDefault(t => t.IsActive) ?? ExamTerms.FirstOrDefault();

                var paged = await _standardService.GetAllAsync(null, 1);
                Standards = new ObservableCollection<StandardItem>(paged.Items);
                SelectedStandard = Standards.FirstOrDefault();

                if (SelectedStandard != null)
                {
                    await LoadSectionsAsync(SelectedStandard.StandardId);
                }
            });
        }

        public override async Task RefreshOnReturnAsync()
        {
            if (SelectedExamTerm != null && SelectedStandard != null && SelectedSection != null)
            {
                await LoadSchedulesAsync();
            }
        }

        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            Schedules = [];
            IsEmpty = false;

            if (value != null)
            {
                _ = LoadSectionsAsync(value.StandardId);
            }
            else
            {
                Sections = [];
                SelectedSection = null;
            }
        }

        private async Task LoadSectionsAsync(int standardId)
        {
            var list = await _sectionService.GetByStandardAsync(standardId, isAdmin: false);
            Sections = new ObservableCollection<SectionItem>(list ?? []);
            SelectedSection = Sections.FirstOrDefault();
        }

        [RelayCommand]
        private async Task LoadSchedulesAsync()
        {
            HasError = false;

            if (SelectedExamTerm == null || SelectedStandard == null || SelectedSection == null)
            {
                HasError = true;
                ErrorMessage = "Please select Exam Term, Class / Standard, and Section to view the datesheet schedule.";
                Schedules = [];
                IsEmpty = false;
                return;
            }

            IsRefreshing = true;
            try
            {
                int termId = SelectedExamTerm.ExamTermId;
                int stdId = SelectedStandard.StandardId;
                int secId = SelectedSection.SectionId;

                var list = await _examService.GetExamSchedulesAsync(termId, stdId, secId);
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
