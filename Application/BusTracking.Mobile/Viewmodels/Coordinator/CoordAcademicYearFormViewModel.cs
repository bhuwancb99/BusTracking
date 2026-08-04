namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordAcademicYearFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private int? _academicYearId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _yearName = string.Empty;
        [ObservableProperty] private DateTime _startDate = new(DateTime.Today.Year, 4, 1);
        [ObservableProperty] private DateTime _endDate = new(DateTime.Today.Year + 1, 3, 31);
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private bool _setAsCurrent = true;

        [ObservableProperty] private bool _isStartDateCalendarOpen;
        [ObservableProperty] private bool _isEndDateCalendarOpen;

        public CoordAcademicYearFormViewModel(IAuthService auth, INavigationService nav, IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _academicYearService = academicYearService;
            Title = "Add Academic Year";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("item", out var obj) && obj is AcademicYearItem item)
            {
                AcademicYearId = item.AcademicYearId;
                YearName = item.YearName;
                StartDate = item.StartDate;
                EndDate = item.EndDate;
                IsActive = item.IsActive;
                SetAsCurrent = item.IsCurrent;
                IsEditMode = true;
                Title = "Edit Academic Year";
            }
            else
            {
                IsEditMode = false;
                Title = "Add Academic Year";
            }
        }

        [RelayCommand]
        private void OpenStartDateCalendar() => IsStartDateCalendarOpen = true;

        [RelayCommand]
        private void OpenEndDateCalendar() => IsEndDateCalendarOpen = true;

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(YearName))
            {
                SetError("Academic Year Name is required.");
                return;
            }

            if (EndDate <= StartDate)
            {
                SetError("End Date must be after Start Date.");
                return;
            }

            await RunAsync(async () =>
            {
                bool finalActive = SetAsCurrent || IsActive;

                var item = new AcademicYearItem
                {
                    AcademicYearId = AcademicYearId ?? 0,
                    YearName = YearName.Trim(),
                    StartDate = StartDate.Date,
                    EndDate = EndDate.Date,
                    IsActive = finalActive,
                    IsCurrent = SetAsCurrent
                };

                ApiResponse<AcademicYearItem> res;
                if (IsEditMode)
                {
                    res = await _academicYearService.UpdateAcademicYearAsync(item, isCoordinator: true);
                }
                else
                {
                    res = await _academicYearService.CreateAcademicYearAsync(item, isCoordinator: true);
                }

                if (res.Success)
                {
                    await ShowToastAsync(IsEditMode ? "Academic Year updated successfully." : "Academic Year created successfully.");
                    await Nav.GoBackAsync();
                }
                else
                {
                    SetError(res.Message);
                }
            });
        }

        [RelayCommand]
        private Task CancelAsync() => Nav.GoBackAsync();
    }
}
