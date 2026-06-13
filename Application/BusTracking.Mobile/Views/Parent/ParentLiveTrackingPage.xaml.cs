namespace BusTracking.Mobile.Views.Parent;

public partial class ParentLiveTrackingPage : ViewBase<ParentLiveTrackingViewModel>
{
    public ParentLiveTrackingPage(ParentLiveTrackingViewModel vm) : base(vm)
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
    }

    // Cleanup SignalR subscriptions when page is left — correct place per project pattern
    protected override void OnDisappearing()
    {
        ViewModel.Cleanup();
        base.OnDisappearing();
    }
}
