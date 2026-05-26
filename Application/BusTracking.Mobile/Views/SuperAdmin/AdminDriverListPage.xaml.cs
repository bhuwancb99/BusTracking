using BusTracking.Mobile.Viewmodels.SuperAdmin;

namespace BusTracking.Mobile.Views.SuperAdmin;

public partial class AdminDriverListPage : ViewBase<AdminDriverListViewModel>
{
    public AdminDriverListPage(AdminDriverListViewModel vm) : base(vm) => InitializeComponent();
}