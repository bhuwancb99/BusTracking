namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordConfigListViewModel : BaseViewModel
    {
        private readonly ICoordAppConfigService _config;

        [ObservableProperty] private ObservableCollection<AppConfigItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedPlatform = "Web";
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search configs…";
        public bool CanAdd => Can("appconfig.add");
        public bool CanEdit => Can("appconfig.edit");
        public bool CanDelete => Can("appconfig.delete");

        // No blank "All Platforms" entry — always one of Web / Mobile / Both.
        public List<string> PlatformOptions => ["Web", "Mobile", "Both"];

        public CoordConfigListViewModel(IAuthService auth, INavigationService nav, ICoordAppConfigService config)
            : base(auth, nav) { _config = config; Title = "App Config"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _config.GetAllAsync(
                    SelectedPlatform, SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage);
                Items = new ObservableCollection<AppConfigItem>(data.Items);
                IsEmpty = !Items.Any();
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        // Fired by the CollectionView when scrolling nears the end
        // (RemainingItemsThreshold on CoordConfigListPage.xaml) — appends the next page.
        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!CanLoadMore || IsBusy) return;
            await RunAsync(async () =>
            {
                CurrentPage++;
                var data = await _config.GetAllAsync(
                    SelectedPlatform, SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();

        partial void OnSelectedPlatformChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordConfigForm");

        [RelayCommand]
        private Task EditAsync(AppConfigItem c) =>
            Nav.GoToAsync("CoordConfigForm", new Dictionary<string, object> { ["ConfigId"] = c.ConfigId });

        [RelayCommand]
        private Task DetailAsync(AppConfigItem c) =>
            Nav.GoToAsync("CoordConfigForm", new Dictionary<string, object> { ["ConfigId"] = c.ConfigId });

        [RelayCommand]
        private async Task ToggleAsync(AppConfigItem c)
        {
            var r = await _config.ToggleAsync(c.ConfigId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(AppConfigItem c)
        {
            if (!await ConfirmAsync("Delete Config", $"Delete '{c.ConfigKey}'?")) return;
            var r = await _config.DeleteAsync(c.ConfigId);
            if (r.Success) { Items.Remove(c); await ShowToastAsync("Config deleted."); }
            else SetError(r.Message);
        }
    }
}
