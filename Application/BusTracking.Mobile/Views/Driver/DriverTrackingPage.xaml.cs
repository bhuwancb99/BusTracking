namespace BusTracking.Mobile.Views.Driver;

public partial class DriverTrackingPage : ViewBase<DriverTrackingViewModel>
{
    public DriverTrackingPage(DriverTrackingViewModel vm) : base(vm)
        => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = Task.Run(async () => await ViewModel.StartGpsTimer());
    }

    protected override void OnDisappearing()
    {
        ViewModel.StopGpsTimer();
        base.OnDisappearing();
    }
}
