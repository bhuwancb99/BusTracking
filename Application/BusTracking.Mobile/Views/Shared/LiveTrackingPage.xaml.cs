namespace BusTracking.Mobile.Views.Shared;

public partial class LiveTrackingPage : ViewBase<LiveTrackingViewModel>
{
    public LiveTrackingPage(LiveTrackingViewModel vm) : base(vm)
    {
        InitializeComponent();

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

        // Inject the API key from AppConfig into the HTML at runtime
        // instead of hardcoding it in the .html file
        try
        {
            var html = await GoogleMapKeyHolder.GetMapHtmlAsync();
            MapWebView.Source = new HtmlWebViewSource { Html = html };
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
