using BusTracking.Mobile.Viewmodels.Coordinator;

namespace BusTracking.Mobile.Views.Coordinator;

public partial class CoordBusListPage : ViewBase<CoordBusListViewModel>
{
    public CoordBusListPage(CoordBusListViewModel vm) : base(vm) => InitializeComponent();
}