namespace BusTracking.Mobile.Views.Driver;

public partial class DriverTrackingPage : ViewBase<DriverTrackingViewModel>
{
    public DriverTrackingPage(DriverTrackingViewModel vm) : base(vm)
        => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Fire-and-forget: requests permission then starts GPS.
        // Must run on the main thread — Permissions.RequestAsync throws
        // if invoked from a background/thread-pool thread (Task.Run).
        _ = MainThread.InvokeOnMainThreadAsync(async () => await ViewModel.StartGpsTimer());
    }

    protected override void OnDisappearing()
    {
        ViewModel.StopGpsTimer();
        base.OnDisappearing();
    }
}
