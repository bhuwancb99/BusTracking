namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordTeacherListViewModel : BaseViewModel
    {
        private readonly ITeacherService _teacherService;

        [ObservableProperty] private ObservableCollection<TeacherItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _canLoadMore;
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private string _selectedFilter = "Active";

        public string SearchPlaceholder => "Search teachers by name or emp code…";
        public List<string> FilterOptions => ["Active", "Inactive", "Both"];

        public bool CanAdd => Can("teachers.add") || Can("teachers.manage");
        public bool CanEdit => Can("teachers.edit") || Can("teachers.manage");
        public bool CanDelete => Can("teachers.delete") || Can("teachers.manage");

        public CoordTeacherListViewModel(IAuthService auth, INavigationService nav, ITeacherService teacherService)
            : base(auth, nav)
        {
            _teacherService = teacherService;
            Title = "Teachers";
        }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        partial void OnSelectedFilterChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _teacherService.GetTeachersAsync(
                    CurrentPage,
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null,
                    SelectedFilter,
                    isCoordinator: true);

                Items = new ObservableCollection<TeacherItem>(data.Items);
                IsEmpty = !Items.Any();
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!CanLoadMore || IsBusy) return;
            await RunAsync(async () =>
            {
                CurrentPage++;
                var data = await _teacherService.GetTeachersAsync(
                    CurrentPage,
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null,
                    SelectedFilter,
                    isCoordinator: true);

                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordTeacherForm");

        [RelayCommand]
        private Task SelectAsync(TeacherItem item) =>
            Nav.GoToAsync("CoordTeacherDetail", new Dictionary<string, object> { ["Teacher"] = item, ["TeacherId"] = item.TeacherId });

        [RelayCommand]
        private Task EditAsync(TeacherItem item) =>
            Nav.GoToAsync("CoordTeacherForm", new Dictionary<string, object> { ["Teacher"] = item, ["TeacherId"] = item.TeacherId });

        [RelayCommand]
        private async Task ToggleStatusAsync(TeacherItem item)
        {
            await RunAsync(async () =>
            {
                var res = await _teacherService.ToggleTeacherStatusAsync(item.TeacherId, isCoordinator: true);
                if (res.Success)
                {
                    item.IsActive = !item.IsActive;
                    await LoadAsync();
                }
                else
                {
                    SetError(res.Message);
                }
            });
        }
    }
}
