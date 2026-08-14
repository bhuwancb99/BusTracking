namespace BusTracking.Mobile.Views.Coordinator
{
    public partial class CoordExamDatesheetPage : ViewBase<CoordExamDatesheetViewModel>
    {
        public CoordExamDatesheetPage(CoordExamDatesheetViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is CoordExamDatesheetViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
