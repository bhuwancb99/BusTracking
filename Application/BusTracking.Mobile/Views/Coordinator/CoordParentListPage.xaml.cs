using BusTracking.Mobile.Viewmodels.Coordinator;

namespace BusTracking.Mobile.Views.Coordinator;

public partial class CoordParentListPage : ViewBase<CoordParentListViewModel>
{
    public CoordParentListPage(CoordParentListViewModel vm) : base(vm) => InitializeComponent();
}