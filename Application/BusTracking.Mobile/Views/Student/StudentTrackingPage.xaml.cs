namespace BusTracking.Mobile.Views.Student;

public partial class StudentTrackingPage : ViewBase<StudentTrackingViewModel>
{

    /// <summary>
    /// StudentTrackingPage
    /// </summary>
    /// <param name="vm"></param>
    public StudentTrackingPage(StudentTrackingViewModel vm) : base(vm) => InitializeComponent();

    /// <summary>
    /// OnDisappearing
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing(); 
        ViewModel.StopPolling();
    }
}