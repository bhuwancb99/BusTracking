namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordConfigFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ICoordAppConfigService _config;

        [ObservableProperty] private int? _configId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _configKey = "";
        [ObservableProperty] private string _configValue = "";
        [ObservableProperty] private string _description = "";
        [ObservableProperty] private string _platform = "Mobile";
        [ObservableProperty] private bool _isActive = true;

        public List<string> PlatformOptions => ["Mobile", "Web", "Both"];

        public CoordConfigFormViewModel(IAuthService auth, INavigationService nav, ICoordAppConfigService config)
            : base(auth, nav) { _config = config; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("ConfigId", out var id)) { ConfigId = (int)id; IsEditMode = true; Title = "Edit Config"; }
            else Title = "Add Config";
        }

        public override async Task InitializeAsync()
        {
            if (!IsEditMode || !ConfigId.HasValue) return;
            await RunAsync(async () =>
            {
                var c = await _config.GetByIdAsync(ConfigId.Value);
                if (c is null) return;
                ConfigKey = c.ConfigKey; ConfigValue = c.ConfigValue;
                Description = c.Description ?? ""; Platform = c.Platform; IsActive = c.IsActive;
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(ConfigKey) || string.IsNullOrWhiteSpace(ConfigValue))
            { SetError("Key and value are required."); return; }

            await RunAsync(async () =>
            {
                ApiResponse<object> r = IsEditMode
                    ? await _config.UpdateAsync(ConfigId!.Value, new UpdateAppConfigRequest
                    { ConfigKey = ConfigKey, ConfigValue = ConfigValue, Description = Description, Platform = Platform, IsActive = IsActive })
                    : await _config.CreateAsync(new CreateAppConfigRequest
                    { ConfigKey = ConfigKey, ConfigValue = ConfigValue, Description = Description, Platform = Platform, IsActive = IsActive });

                if (r.Success) { await ShowToastAsync(IsEditMode ? "Config updated." : "Config created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
