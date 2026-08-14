namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminMarksEntryViewModel : TeacherMarksEntryViewModel
    {
        public AdminMarksEntryViewModel(IAuthService auth, INavigationService nav,
            IExamService examService, IAdminStandardService standardService, ISectionService sectionService)
            : base(auth, nav, examService, standardService, sectionService)
        {
            Title = "Admin Marks Entry";
        }
    }
}
