using BusTracking.Mobile.Viewmodels.Coordinator;

namespace BusTracking.Mobile.Views.Coordinator;

public partial class CoordinatorDashboardPage : ViewBase<CoordinatorDashboardViewModel>
{
    public CoordinatorDashboardPage(CoordinatorDashboardViewModel vm) : base(vm) => InitializeComponent();
}