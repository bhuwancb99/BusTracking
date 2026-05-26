using BusTracking.Mobile.Viewmodels.Coordinator;

namespace BusTracking.Mobile.Views.Coordinator;

public partial class CoordTripListPage : ViewBase<CoordTripListViewModel>
{
    public CoordTripListPage(CoordTripListViewModel vm) : base(vm) => InitializeComponent();
}