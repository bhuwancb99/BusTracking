using BusTracking.Mobile.Viewmodels.Coordinator;

namespace BusTracking.Mobile.Views.Coordinator;

public partial class CoordDriverListPage : ViewBase<CoordDriverListViewModel>
{
    public CoordDriverListPage(CoordDriverListViewModel vm) : base(vm) => InitializeComponent();
}