using BusTracking.Mobile.Viewmodels.Coordinator;

namespace BusTracking.Mobile.Views.Coordinator;

public partial class CoordDriverFormPage : ViewBase<CoordDriverFormViewModel>
{
    public CoordDriverFormPage(CoordDriverFormViewModel vm) : base(vm) => InitializeComponent();
}
