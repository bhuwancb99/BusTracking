namespace BusTracking.Mobile.Views.Shared;

public partial class LiveTrackingPage : ViewBase<LiveTrackingViewModel>
{
    private readonly IAppConfigService _appConfig;

    public LiveTrackingPage(LiveTrackingViewModel vm, IAppConfigService appConfig) : base(vm)
    {
        InitializeComponent();
        _appConfig = appConfig;

        // Wire JS bridge: ViewModel → WebView
        vm.SendToMap = js =>
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try { await MapWebView.EvaluateJavaScriptAsync(js); }
                catch { /* WebView not yet ready — next update will catch up */ }
            });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Safe check: reload API key if it failed to load during initial startup
        if (string.IsNullOrWhiteSpace(GoogleMapKeyHolder.ApiKey))
        {
            try
            {
                var key = await _appConfig.GetValueAsync("GoogleMapApiKey");
                if (!string.IsNullOrWhiteSpace(key))
                    GoogleMapKeyHolder.ApiKey = key;
            }
            catch { /* offline/error fallback */ }
        }

        // Inject the API key from AppConfig into the HTML at runtime
        // instead of hardcoding it in the .html file
        try
        {
            var html = await GoogleMapKeyHolder.GetMapHtmlAsync();
            MapWebView.Source = new HtmlWebViewSource
            {
                Html = html,
                BaseUrl = Constants.ApiBaseUrl
            };
        }
        catch
        {
            // Fallback: load html file directly (key will be the placeholder)
            MapWebView.Source = "tracking_map.html";
        }
    }

    protected override void OnDisappearing()
    {
        ViewModel.Cleanup();
        base.OnDisappearing();
    }
}
