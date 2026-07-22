namespace BusTracking.Mobile
{
    public partial class App : Application
    {
        private readonly IAppConfigService _config;

        public App(IAppConfigService config)
        {
            InitializeComponent();
            _config = config;

            // Load Google Maps API key from DB into GoogleMapKeyHolder
            // Runs once at app start — key is then available for all WebViews
            _ = Task.Run(LoadGoogleMapKeyAsync);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = Handler?.MauiContext?.Services.GetRequiredService<AppShell>()
                        ?? IPlatformApplication.Current!.Services.GetRequiredService<AppShell>();
            return new Window(shell);
        }

        private async Task LoadGoogleMapKeyAsync()
        {
            try
            {
                var isUseMap = await _config.GetValueAsync("IsUseGoogleMap");
                if (!string.IsNullOrWhiteSpace(isUseMap))
                    GoogleMapKeyHolder.IsUseGoogleMap = isUseMap;

                var key = await _config.GetValueAsync("GoogleMapApiKey");
                if (!string.IsNullOrWhiteSpace(key))
                    GoogleMapKeyHolder.ApiKey = key;
            }
            catch { /* non-fatal — map will use default until next launch */ }
        }
    }
}
