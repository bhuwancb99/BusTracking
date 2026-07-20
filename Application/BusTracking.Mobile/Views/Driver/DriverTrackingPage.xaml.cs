namespace BusTracking.Mobile.Views.Driver;

public partial class DriverTrackingPage : ViewBase<DriverTrackingViewModel>
{
    private readonly IAppConfigService _appConfig;

    public DriverTrackingPage(DriverTrackingViewModel vm, IAppConfigService appConfig) : base(vm)
    {
        InitializeComponent();
        _appConfig = appConfig;

        // Wire JS bridge: ViewModel → WebView
        vm.SendToMap = js =>
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (MapWebView != null)
                        await MapWebView.EvaluateJavaScriptAsync(js);
                }
                catch { /* WebView not yet ready */ }
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
