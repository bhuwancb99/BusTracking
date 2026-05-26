using BusTracking.Mobile.Viewmodels.Parent;

namespace BusTracking.Mobile.Views.Parent;

public partial class ParentDashboardPage : ViewBase<ParentDashboardViewModel>
{
    public ParentDashboardPage(ParentDashboardViewModel vm) : base(vm) => InitializeComponent();
}