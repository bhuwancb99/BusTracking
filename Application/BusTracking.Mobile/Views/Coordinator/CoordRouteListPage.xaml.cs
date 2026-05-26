using BusTracking.Mobile.Viewmodels.Coordinator;

namespace BusTracking.Mobile.Views.Coordinator;

public partial class CoordRouteListPage : ViewBase<CoordRouteListViewModel>
{
    public CoordRouteListPage(CoordRouteListViewModel vm) : base(vm) => InitializeComponent();
}