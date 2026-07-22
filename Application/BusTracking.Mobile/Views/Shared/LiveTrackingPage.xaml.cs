namespace BusTracking.Mobile.Views.Shared;

public partial class LiveTrackingPage : ViewBase<LiveTrackingViewModel>
{
    private readonly IAppConfigService _appConfig;

    // Guards against the race where setRouteStops()/moveBus() are invoked
    // before the WebView has finished navigating to the map HTML — without
    // this, EvaluateJavaScriptAsync silently throws (caught below) and the
    // call — e.g. the very first setRouteStops that places the bus at the
    // start stop — is lost forever instead of being retried.
    private bool _webViewReady;
    private readonly List<string> _pendingJs = new();
    private readonly object _pendingJsLock = new();

    public LiveTrackingPage(LiveTrackingViewModel vm, IAppConfigService appConfig) : base(vm)
    {
        InitializeComponent();
        _appConfig = appConfig;

        // Wire JS bridge: ViewModel → WebView
        vm.SendToMap = js =>
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!_webViewReady)
                {
                    lock (_pendingJsLock) { _pendingJs.Add(js); }
                    return;
                }

                try { await MapWebView.EvaluateJavaScriptAsync(js); }
                catch { /* WebView not yet ready — will be retried on Navigated */ }
            });

        if (MapWebView != null)
        {
            MapWebView.Navigating += async (s, e) =>
            {
                if (e.Url != null && e.Url.StartsWith("applog://maperror", StringComparison.OrdinalIgnoreCase))
                {
                    e.Cancel = true;
                    try
                    {
                        var uri = new Uri(e.Url);
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        var errorMsg = query["message"] ?? "Google Maps error / limit reached.";

                        // 1. Console Log
                        Console.WriteLine($"[MAP ERROR CONSOLE] {errorMsg}");
                        System.Diagnostics.Debug.WriteLine($"[MAP ERROR CONSOLE] {errorMsg}");

                        // 2. Database Logger Table Entry
                        var logService = IPlatformApplication.Current?.Services.GetService<IMobileLogService>();
                        if (logService != null)
                        {
                            await logService.LogEventAsync(
                                message: $"Map Load/Auth Error: {errorMsg}",
                                moduleName: "LiveTracking",
                                actionName: "GoogleMapAuthError",
                                additionalDetails: $"GoogleMapApiKey: {GoogleMapKeyHolder.ApiKey}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MAP ERROR PARSE EX] {ex.Message}");
                    }
                }
            };

            MapWebView.Navigated += async (s, e) =>
            {
                _webViewReady = true;

                List<string> toFlush;
                lock (_pendingJsLock)
                {
                    toFlush = new List<string>(_pendingJs);
                    _pendingJs.Clear();
                }

                foreach (var js in toFlush)
                {
                    try { await MapWebView.EvaluateJavaScriptAsync(js); }
                    catch { /* ignore, best-effort replay */ }
                }
            };
        }
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
        _webViewReady = false;
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
