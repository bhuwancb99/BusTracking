namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordAcademicYearViewModel : BaseViewModel
    {
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private ObservableCollection<AcademicYearItem> _academicYears = new();
        [ObservableProperty] private bool _isRefreshing;

        public bool CanAdd => Can("academicyear.add");
        public bool CanEdit => Can("academicyear.edit");

        public CoordAcademicYearViewModel(IAuthService auth, INavigationService nav, IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _academicYearService = academicYearService;
            Title = "Academic Years";
        }

        [RelayCommand]
        private async Task LoadAcademicYearsAsync()
        {
            await RunAsync(async () =>
            {
                var list = await _academicYearService.GetAcademicYearsAsync(isCoordinator: true);
                AcademicYears = new ObservableCollection<AcademicYearItem>(list);
            });
            IsRefreshing = false;
        }

        [RelayCommand]
        private async Task AddAsync()
        {
            if (!CanAdd) return;
            await Nav.GoToAsync("CoordAcademicYearForm");
        }

        [RelayCommand]
        private async Task EditAsync(AcademicYearItem item)
        {
            if (!CanEdit || item == null) return;
            var param = new Dictionary<string, object> { { "item", item } };
            await Nav.GoToAsync("CoordAcademicYearForm", param);
        }

        [RelayCommand]
        private async Task SetActiveSessionAsync(AcademicYearItem item)
        {
            if (!CanEdit || item == null || item.IsCurrent) return;

            bool confirm = await ConfirmAsync("Active Session", $"Set {item.YearName} as the current active academic session?", "Set Active", "Cancel");
            if (!confirm) return;

            await RunAsync(async () =>
            {
                var res = await _academicYearService.SetActiveAcademicYearAsync(item.AcademicYearId, isCoordinator: true);
                if (res.Success)
                {
                    await ShowToastAsync("Active session updated successfully.");
                    await LoadAcademicYearsAsync();
                }
                else
                {
                    SetError(res.Message);
                }
            });
        }
    }
}
