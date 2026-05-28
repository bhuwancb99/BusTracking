namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminConfigListViewModel : BaseViewModel
    {
        private readonly IAdminConfigService _config;

        [ObservableProperty] private ObservableCollection<AppConfigItem> _items = [];
        [ObservableProperty] private string _selectedPlatform = "";

        public List<string> PlatformOptions => ["", "Mobile", "Web", "Both"];

        public AdminConfigListViewModel(IAuthService auth, INavigationService nav, IAdminConfigService config)
            : base(auth, nav) { _config = config; Title = "App Configuration"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _config.GetAllAsync(SelectedPlatform.Length > 0 ? SelectedPlatform : null);
                Items = new ObservableCollection<AppConfigItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminConfigForm");
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
    }
}
