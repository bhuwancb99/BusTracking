using BusTracking.Mobile.Viewmodels.SuperAdmin;

namespace BusTracking.Mobile.Views.SuperAdmin;

public partial class AdminTripListPage : ViewBase<AdminTripListViewModel>
{
    public AdminTripListPage(AdminTripListViewModel vm) : base(vm) => InitializeComponent();
}