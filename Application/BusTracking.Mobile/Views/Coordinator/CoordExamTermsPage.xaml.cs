namespace BusTracking.Mobile.Views.Coordinator
{
    public partial class CoordExamTermsPage : ViewBase<CoordExamTermsViewModel>
    {
        public CoordExamTermsPage(CoordExamTermsViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is CoordExamTermsViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
