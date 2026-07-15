namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminConfigListViewModel : BaseViewModel
    {
        private readonly IAdminConfigService _config;

        [ObservableProperty] private ObservableCollection<AppConfigItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedPlatform = "Mobile";
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search configs…";
        public bool CanAdd => true;

        // No blank "All Platforms" entry — always one of Web / Mobile / Both.
        public List<string> PlatformOptions => ["Web", "Mobile", "Both"];

        public AdminConfigListViewModel(IAuthService auth, INavigationService nav, IAdminConfigService config)
            : base(auth, nav) { _config = config; Title = "App Configuration"; }

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
        // (RemainingItemsThreshold on AdminConfigListPage.xaml) — appends the next page.
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

        // Filter re-loads when platform picker changes
        partial void OnSelectedPlatformChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private void Filter(string platform) => SelectedPlatform = platform;

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminConfigForm");

        // Tap on row → Edit (AppConfig has no separate detail page)
        [RelayCommand]
        private Task DetailAsync(AppConfigItem c) =>
            Nav.GoToAsync("AdminConfigForm", new Dictionary<string, object> { ["ConfigId"] = c.ConfigId });

        [RelayCommand]
        private Task EditAsync(AppConfigItem c) =>
            Nav.GoToAsync("AdminConfigForm", new Dictionary<string, object> { ["ConfigId"] = c.ConfigId });

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
        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}