namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordClassMappingListViewModel : BaseViewModel
    {
        private readonly IClassMappingService _mappingService;
        private readonly IAcademicYearService _yearService;
        private readonly ICoordStandardService _standardService;

        [ObservableProperty] private ObservableCollection<AcademicYearItem> _academicYears = [];
        [ObservableProperty] private AcademicYearItem? _selectedYear;
        [ObservableProperty] private ObservableCollection<StandardItem> _standards = [];
        [ObservableProperty] private StandardItem? _selectedStandard;
        [ObservableProperty] private ObservableCollection<ClassMappingItem> _items = [];

        public bool CanAdd => true;

        public CoordClassMappingListViewModel(
            IAuthService auth,
            INavigationService nav,
            IClassMappingService mappingService,
            IAcademicYearService yearService,
            ICoordStandardService standardService)
            : base(auth, nav)
        {
            _mappingService = mappingService;
            _yearService = yearService;
            _standardService = standardService;
            Title = "Class Subject & Teacher Mapping";
        }

        public override async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                var years = await _yearService.GetAcademicYearsAsync(true);
                AcademicYears = new ObservableCollection<AcademicYearItem>(years);
                _selectedYear = AcademicYears.FirstOrDefault(y => y.IsCurrent) ?? AcademicYears.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedYear));

                var stds = await _standardService.GetAllAsync(null, 1);
                Standards = new ObservableCollection<StandardItem>(stds.Items);
                _selectedStandard = Standards.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedStandard));

                await FetchMappingsAsync();
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        partial void OnSelectedYearChanged(AcademicYearItem? value) => _ = FetchMappingsWithLoaderAsync();
        partial void OnSelectedStandardChanged(StandardItem? value) => _ = FetchMappingsWithLoaderAsync();

        private async Task FetchMappingsWithLoaderAsync()
        {
            IsBusy = true;
            try { await FetchMappingsAsync(); }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        private async Task FetchMappingsAsync()
        {
            var data = await _mappingService.GetAllAsync(SelectedYear?.AcademicYearId, SelectedStandard?.StandardId, isCoordinator: true);
            Items = new ObservableCollection<ClassMappingItem>(data);
            IsEmpty = !Items.Any();
        }

        [RelayCommand]
        private Task AddAsync() => Nav.GoToAsync("CoordClassMappingForm");

        [RelayCommand]
        private Task EditAsync(ClassMappingItem item) =>
            Nav.GoToAsync("CoordClassMappingForm", new Dictionary<string, object>
            {
                ["MappingId"] = item.ClassSubjectTeacherId,
                ["Item"] = item
            });

        [RelayCommand]
        private async Task DeleteAsync(ClassMappingItem item)
        {
            if (!await ConfirmAsync("Unassign Mapping", $"Unassign '{item.SubjectName}' from '{item.TeacherName}'?")) return;
            var r = await _mappingService.DeleteAsync(item.ClassSubjectTeacherId, isCoordinator: true);
            if (r.Success) { Items.Remove(item); IsEmpty = !Items.Any(); await ShowToastAsync("Mapping unassigned."); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try { await FetchMappingsAsync(); }
            finally { IsRefreshing = false; }
        }
    }
}
