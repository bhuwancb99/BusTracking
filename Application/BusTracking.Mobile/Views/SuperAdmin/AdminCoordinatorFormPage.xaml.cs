using BusTracking.Mobile.Viewmodels.SuperAdmin;

namespace BusTracking.Mobile.Views.SuperAdmin;

public partial class AdminCoordinatorFormPage : ViewBase<AdminCoordinatorFormViewModel>
{
    public AdminCoordinatorFormPage(AdminCoordinatorFormViewModel vm) : base(vm) => InitializeComponent();
}