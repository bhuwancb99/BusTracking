namespace BusTracking.Mobile.Views.Shared;

public partial class LiveTrackingPage : ViewBase<LiveTrackingViewModel>
{
    public LiveTrackingPage(LiveTrackingViewModel vm) : base(vm)
    {
        InitializeComponent();

        // Wire the JS bridge: ViewModel → WebView
        vm.SendToMap = js =>
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try { await MapWebView.EvaluateJavaScriptAsync(js); }
                catch { /* WebView not yet ready — next update will catch up */ }
            });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
    }
    protected override void OnDisappearing()
    {
        ViewModel.Cleanup();
        base.OnDisappearing();
    }
}
