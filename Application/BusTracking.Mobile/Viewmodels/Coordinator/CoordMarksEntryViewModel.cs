namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordMarksEntryViewModel : TeacherMarksEntryViewModel
    {
        public CoordMarksEntryViewModel(IAuthService auth, INavigationService nav,
            IExamService examService, IAdminStandardService standardService, ISectionService sectionService)
            : base(auth, nav, examService, standardService, sectionService)
        {
            Title = "Coordinator Marks Entry";
        }
    }
}
