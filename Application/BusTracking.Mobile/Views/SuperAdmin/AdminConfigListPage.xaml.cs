using BusTracking.Mobile.Viewmodels.SuperAdmin;

namespace BusTracking.Mobile.Views.SuperAdmin;

public partial class AdminConfigListPage : ViewBase<AdminConfigListViewModel>
{
    public AdminConfigListPage(AdminConfigListViewModel vm) : base(vm) => InitializeComponent();
}