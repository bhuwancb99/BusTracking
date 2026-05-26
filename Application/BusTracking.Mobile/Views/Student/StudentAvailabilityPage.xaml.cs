using BusTracking.Mobile.Viewmodels.Student;

namespace BusTracking.Mobile.Views.Student;

public partial class StudentAvailabilityPage : ViewBase<StudentAvailabilityViewModel>
{
    public StudentAvailabilityPage(StudentAvailabilityViewModel vm) : base(vm) => InitializeComponent();
}