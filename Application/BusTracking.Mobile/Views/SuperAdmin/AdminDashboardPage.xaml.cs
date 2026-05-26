using BusTracking.Mobile.Viewmodels.SuperAdmin;

namespace BusTracking.Mobile.Views.SuperAdmin;

public partial class AdminDashboardPage : ViewBase<AdminDashboardViewModel>
{
    public AdminDashboardPage(AdminDashboardViewModel vm) : base(vm) => InitializeComponent();
}