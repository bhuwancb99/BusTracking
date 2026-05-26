using BusTracking.Mobile.Viewmodels.Coordinator;

namespace BusTracking.Mobile.Views.Coordinator;

public partial class CoordStudentListPage : ViewBase<CoordStudentListViewModel>
{
    public CoordStudentListPage(CoordStudentListViewModel vm) : base(vm) => InitializeComponent();
}