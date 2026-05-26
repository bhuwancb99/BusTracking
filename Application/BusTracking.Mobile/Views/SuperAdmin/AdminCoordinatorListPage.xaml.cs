using BusTracking.Mobile.Viewmodels.SuperAdmin;

namespace BusTracking.Mobile.Views.SuperAdmin;

public partial class AdminCoordinatorListPage : ViewBase<AdminCoordinatorListViewModel>
{
    public AdminCoordinatorListPage(AdminCoordinatorListViewModel vm) : base(vm) => InitializeComponent();
}