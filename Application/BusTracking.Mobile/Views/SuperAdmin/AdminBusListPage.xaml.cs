using BusTracking.Mobile.Viewmodels.SuperAdmin;

namespace BusTracking.Mobile.Views.SuperAdmin;

public partial class AdminBusListPage : ViewBase<AdminBusListViewModel>
{
    public AdminBusListPage(AdminBusListViewModel vm) : base(vm) => InitializeComponent();
}