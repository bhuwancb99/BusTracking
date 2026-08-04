namespace BusTracking.Mobile.Views.Coordinator
{
    public partial class CoordAcademicYearPage : ViewBase<CoordAcademicYearViewModel>
    {
        public CoordAcademicYearPage(CoordAcademicYearViewModel vm) : base(vm)
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
