namespace BusTracking.Mobile.Views.Parent;

public partial class ParentTrackingPage : ViewBase<ParentTrackingViewModel>
{
    public ParentTrackingPage(ParentTrackingViewModel vm) : base(vm) => InitializeComponent();
    protected override void OnDisappearing() { base.OnDisappearing(); ViewModel.StopPolling(); }
}