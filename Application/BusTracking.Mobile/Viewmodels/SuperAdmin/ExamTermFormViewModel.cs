namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    [QueryProperty(nameof(ExamTerm), "ExamTerm")]
    public partial class ExamTermFormViewModel : BaseViewModel
    {
        private readonly IExamService _examService;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty]
        private ExamTermItem? _examTerm;

        [ObservableProperty]
        private ObservableCollection<AcademicYearLookupItem> _academicYears = [];

        [ObservableProperty]
        private AcademicYearLookupItem? _selectedAcademicYear;

        [ObservableProperty]
        private string _termName = "";

        [ObservableProperty]
        private string _description = "";

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today.AddDays(14);

        [ObservableProperty]
        private bool _isStartDateCalendarOpen;

        [ObservableProperty]
        private bool _isEndDateCalendarOpen;

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private bool _isSaving;

        public ExamTermFormViewModel(IAuthService auth, INavigationService nav, IExamService examService, IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _examService = examService;
            _academicYearService = academicYearService;
            Title = "Add Exam Term";
        }

        public override async Task InitializeAsync()
        {
            await LoadAcademicYearsAsync();

            if (ExamTerm != null)
            {
                Title = "Edit Exam Term";
                TermName = ExamTerm.TermName;
                Description = ExamTerm.Description ?? "";
                StartDate = ExamTerm.StartDate;
                EndDate = ExamTerm.EndDate;
                IsActive = ExamTerm.IsActive;
                SelectedAcademicYear = AcademicYears.FirstOrDefault(y => y.AcademicYearId == ExamTerm.AcademicYearId);
            }
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

        [RelayCommand]
        private void OpenStartDateCalendar()
        {
            IsStartDateCalendarOpen = true;
        }

        [RelayCommand]
        private void OpenEndDateCalendar()
        {
            IsEndDateCalendarOpen = true;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(TermName))
            {
                HasError = true;
                ErrorMessage = "Term Name is required.";
                return;
            }

            if (SelectedAcademicYear == null)
            {
                HasError = true;
                ErrorMessage = "Please select an Academic Year.";
                return;
            }

            IsSaving = true;
            HasError = false;

            try
            {
                if (ExamTerm == null)
                {
                    var req = new CreateExamTermRequest
                    {
                        AcademicYearId = SelectedAcademicYear.AcademicYearId,
                        TermName = TermName.Trim(),
                        Description = Description?.Trim(),
                        StartDate = StartDate,
                        EndDate = EndDate,
                        IsActive = IsActive
                    };
                    var res = await _examService.CreateExamTermAsync(req);
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
                    var req = new UpdateExamTermRequest
                    {
                        AcademicYearId = SelectedAcademicYear.AcademicYearId,
                        TermName = TermName.Trim(),
                        Description = Description?.Trim(),
                        StartDate = StartDate,
                        EndDate = EndDate,
                        IsActive = IsActive
                    };
                    var res = await _examService.UpdateExamTermAsync(ExamTerm.ExamTermId, req);
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
