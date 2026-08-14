namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminExamTermsViewModel : BaseViewModel
    {
        private readonly IExamService _examService;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty]
        private ObservableCollection<ExamTermItem> _examTerms = [];

        [ObservableProperty]
        private ObservableCollection<AcademicYearLookupItem> _academicYears = [];

        [ObservableProperty]
        private AcademicYearLookupItem? _selectedAcademicYear;

        public AdminExamTermsViewModel(IAuthService auth, INavigationService nav, IExamService examService, IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _examService = examService;
            _academicYearService = academicYearService;
            Title = "Exam Terms";
        }

        public override async Task InitializeAsync()
        {
            await LoadAcademicYearsAsync();
        }

        public override async Task RefreshOnReturnAsync()
        {
            await LoadExamTermsAsync();
        }

        private async Task LoadAcademicYearsAsync()
        {
            try
            {
                var years = await _academicYearService.GetAcademicYearsAsync(isAdmin: true);
                var items = years.Select(y => new AcademicYearLookupItem
                {
                    AcademicYearId = y.AcademicYearId,
                    YearName = y.YearName,
                    IsCurrent = y.IsCurrent
                }).ToList();

                AcademicYears = new ObservableCollection<AcademicYearLookupItem>(items);
                SelectedAcademicYear = items.FirstOrDefault(y => y.IsCurrent) ?? items.FirstOrDefault();
                await LoadExamTermsAsync();
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }

        [RelayCommand]
        private async Task LoadExamTermsAsync()
        {
            if (SelectedAcademicYear == null) return;
            IsRefreshing = true;
            ClearError();

            try
            {
                var data = await _examService.GetExamTermsAsync(SelectedAcademicYear.AcademicYearId);
                ExamTerms = new ObservableCollection<ExamTermItem>(data);
                IsEmpty = !ExamTerms.Any();
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        partial void OnSelectedAcademicYearChanged(AcademicYearLookupItem? value)
        {
            _ = LoadExamTermsAsync();
        }

        [RelayCommand]
        private async Task AddTermAsync()
        {
            await Nav.GoToAsync("AdminExamTermForm");
        }

        [RelayCommand]
        private async Task EditTermAsync(ExamTermItem item)
        {
            if (item == null) return;
            var param = new Dictionary<string, object> { { "ExamTerm", item } };
            await Nav.GoToAsync("AdminExamTermForm", param);
        }

        [RelayCommand]
        private async Task DeleteTermAsync(ExamTermItem item)
        {
            if (item == null) return;
            if (!await ConfirmAsync("Delete Exam Term", $"Are you sure you want to delete '{item.TermName}'?")) return;

            var res = await _examService.DeleteExamTermAsync(item.ExamTermId);
            if (res.Success)
            {
                ExamTerms.Remove(item);
                IsEmpty = !ExamTerms.Any();
            }
            else
            {
                SetError(res.Message);
            }
        }
    }
}
