using Foundation;
using UIKit;

namespace BusTracking.Mobile
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(
            UIApplication application, NSDictionary? launchOptions)
        {
            var result = base.FinishedLaunching(application, launchOptions);

            // Load Google Maps API key from AppConfig
            // This runs after DI is ready so we can resolve IAppConfigService
            _ = Task.Run(InitGoogleMapsAsync);

            return result;
        }

        private static async Task InitGoogleMapsAsync()
        {
            try
            {
                var config = IPlatformApplication.Current!.Services
                    .GetRequiredService<IAppConfigService>();

                var apiKey = await config.GetValueAsync("GoogleMapApiKey");

                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    // Must run on main thread
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        // If you later add native Google Maps iOS SDK:
                        // Google.Maps.MapServices.ProvideAPIKey(apiKey);

                        // For now inject the key into the WebView HTML at runtime
                        GoogleMapKeyHolder.ApiKey = apiKey;
                    });
                }
            }
            catch { /* config not yet available — WebView will use fallback key */ }
        }
    }
}
