#pragma warning disable CA1416

namespace BusTracking.Mobile.Views.Driver;

public partial class DriverTrackingPage : ViewBase<DriverTrackingViewModel>
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

    public DriverTrackingPage(DriverTrackingViewModel vm, IAppConfigService appConfig) : base(vm)
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

                try
                {
                    if (MapWebView != null)
                        await MapWebView.EvaluateJavaScriptAsync(js);
                }
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
                        Console.WriteLine($"[DRIVER MAP ERROR CONSOLE] {errorMsg}");
                        System.Diagnostics.Debug.WriteLine($"[DRIVER MAP ERROR CONSOLE] {errorMsg}");

                        // 2. Database Logger Table Entry
                        var logService = IPlatformApplication.Current?.Services.GetService<IMobileLogService>();
                        if (logService != null)
                        {
                            await logService.LogEventAsync(
                                message: $"Driver Map Load/Auth Error: {errorMsg}",
                                moduleName: "DriverTracking",
                                actionName: "GoogleMapAuthError",
                                additionalDetails: $"GoogleMapApiKey: {GoogleMapKeyHolder.ApiKey}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DRIVER MAP ERROR PARSE EX] {ex.Message}");
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

        // Safe check: reload API key & IsUseGoogleMap from AppConfig
        try
        {
            var isUseMap = await _appConfig.GetValueAsync("IsUseGoogleMap");
            if (!string.IsNullOrWhiteSpace(isUseMap))
                GoogleMapKeyHolder.IsUseGoogleMap = isUseMap;

            var key = await _appConfig.GetValueAsync("GoogleMapApiKey");
            if (!string.IsNullOrWhiteSpace(key))
                GoogleMapKeyHolder.ApiKey = key;
        }
        catch { /* offline/error fallback */ }

        // Inject the API key from AppConfig into the HTML at runtime
        _webViewReady = false;
        try
        {
            var html = await GoogleMapKeyHolder.GetMapHtmlAsync();
            if (MapWebView != null)
            {
                MapWebView.Source = new HtmlWebViewSource
                {
                    Html = html,
                    BaseUrl = Constants.ApiBaseUrl
                };
            }
        }
        catch
        {
            if (MapWebView != null) MapWebView.Source = "tracking_map.html";
        }

        // Fire-and-forget: requests permission then starts GPS.
        _ = MainThread.InvokeOnMainThreadAsync(async () => await ViewModel.StartGpsTimer());
    }

    protected override void OnDisappearing()
    {
        ViewModel.StopGpsTimer();
        base.OnDisappearing();
    }

    private void OnHeaderPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                if (e.TotalY < -15)
                {
                    if (BindingContext is DriverTrackingViewModel vm && !vm.IsSheetExpanded)
                    {
                        vm.IsSheetExpanded = true;
                    }
                }
                else if (e.TotalY > 15)
                {
                    if (BindingContext is DriverTrackingViewModel vm && vm.IsSheetExpanded)
                    {
                        vm.IsSheetExpanded = false;
                    }
                }
                break;
        }
    }
}
