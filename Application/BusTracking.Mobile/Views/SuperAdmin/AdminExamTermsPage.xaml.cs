namespace BusTracking.Mobile.Views.SuperAdmin
{
    public partial class AdminExamTermsPage : ViewBase<AdminExamTermsViewModel>
    {
        public AdminExamTermsPage(AdminExamTermsViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AdminExamTermsViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
