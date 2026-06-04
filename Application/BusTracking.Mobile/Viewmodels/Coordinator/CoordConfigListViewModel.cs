namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordConfigListViewModel : BaseViewModel
    {
        private readonly ICoordAppConfigService _config;

        [ObservableProperty] private ObservableCollection<AppConfigItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedPlatform = "";

        public string SearchPlaceholder => "Search configs…";
        public bool CanAdd => Can("appconfig.add");
        public bool CanEdit => Can("appconfig.edit");
        public bool CanDelete => Can("appconfig.delete");
        public bool CanLoadMore => false;
        public List<string> PlatformOptions => ["", "Mobile", "Web", "Both"];

        [RelayCommand] private async Task LoadMoreAsync() { }
        [RelayCommand] private async Task SearchAsync() => await LoadAsync();

        public CoordConfigListViewModel(IAuthService auth, INavigationService nav, ICoordAppConfigService config)
            : base(auth, nav) { _config = config; Title = "App Config"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var platform = SelectedPlatform.Length > 0 ? SelectedPlatform : null;
                var data = await _config.GetAllAsync(platform);
                if (!string.IsNullOrWhiteSpace(SearchText))
                    data = data.Where(c => c.ConfigKey.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                          c.ConfigValue.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
                Items = new ObservableCollection<AppConfigItem>(data);
                IsEmpty = !Items.Any();
            });
        }

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
