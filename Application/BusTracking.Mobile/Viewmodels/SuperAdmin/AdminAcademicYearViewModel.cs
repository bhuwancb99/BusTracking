namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminAcademicYearViewModel : BaseViewModel
    {
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private ObservableCollection<AcademicYearItem> _academicYears = new();
        [ObservableProperty] private bool _isRefreshing;

        public AdminAcademicYearViewModel(IAuthService auth, INavigationService nav, IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _academicYearService = academicYearService;
            Title = "Academic Years";
        }

        public override async Task InitializeAsync() => await LoadAcademicYearsAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAcademicYearsAsync();

        [RelayCommand]
        private async Task LoadAcademicYearsAsync()
        {
            await RunAsync(async () =>
            {
                var list = await _academicYearService.GetAcademicYearsAsync(isCoordinator: false);
                // Must update ObservableCollection on UI thread — background thread updates
                // silently fail in release/direct-run (race condition masked by debugger timing)
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AcademicYears = new ObservableCollection<AcademicYearItem>(list);
                });
            });
            await MainThread.InvokeOnMainThreadAsync(() => IsRefreshing = false);
        }

        [RelayCommand]
        private async Task AddAsync()
        {
            await Nav.GoToAsync("AdminAcademicYearForm");
        }

        [RelayCommand]
        private async Task EditAsync(AcademicYearItem item)
        {
            if (item == null) return;
            var param = new Dictionary<string, object> { { "item", item } };
            await Nav.GoToAsync("AdminAcademicYearForm", param);
        }

        [RelayCommand]
        private async Task SetActiveSessionAsync(AcademicYearItem item)
        {
            if (item == null || item.IsCurrent) return;

            bool confirm = await ConfirmAsync("Active Session", $"Set {item.YearName} as the current active academic session?", "Set Active", "Cancel");
            if (!confirm) return;

            bool success = false;
            await RunAsync(async () =>
            {
                var res = await _academicYearService.SetActiveAcademicYearAsync(item.AcademicYearId, isCoordinator: false);
                if (res.Success)
                {
                    success = true;
                }
                else
                {
                    SetError(res.Message);
                }
            });

            // Reload OUTSIDE RunAsync so IsBusy guard does not block the inner call
            if (success)
            {
                await ShowToastAsync("Active session updated successfully.");
                await LoadAcademicYearsAsync();
            }
        }
    }
}
