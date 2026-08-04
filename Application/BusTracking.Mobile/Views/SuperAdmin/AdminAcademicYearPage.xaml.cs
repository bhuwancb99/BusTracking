namespace BusTracking.Mobile.Views.SuperAdmin
{
    public partial class AdminAcademicYearPage : ViewBase<AdminAcademicYearViewModel>
    {
        public AdminAcademicYearPage(AdminAcademicYearViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel?.AcademicYears.Count == 0)
            {
                ViewModel.LoadAcademicYearsCommand.Execute(null);
            }
        }
    }
}
