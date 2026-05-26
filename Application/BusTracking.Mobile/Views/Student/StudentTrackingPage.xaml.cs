using BusTracking.Mobile.Viewmodels.Student;

namespace BusTracking.Mobile.Views.Student;

public partial class StudentTrackingPage : ViewBase<StudentTrackingViewModel>
{
    public StudentTrackingPage(StudentTrackingViewModel vm) : base(vm) => InitializeComponent();
    protected override void OnDisappearing() { base.OnDisappearing(); ViewModel.StopPolling(); }
}