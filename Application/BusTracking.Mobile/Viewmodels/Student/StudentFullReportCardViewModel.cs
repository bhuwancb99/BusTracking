namespace BusTracking.Mobile.Viewmodels.Student
{
    [QueryProperty(nameof(ReportCard), "ReportCard")]
    public partial class StudentFullReportCardViewModel : BaseViewModel
    {
        [ObservableProperty]
        private StudentReportCardItem? _reportCard;

        public StudentFullReportCardViewModel(IAuthService auth, INavigationService nav)
            : base(auth, nav)
        {
            Title = "Report Card Details";
        }
    }
}
