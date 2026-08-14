namespace BusTracking.Mobile.Views.SuperAdmin
{
    public partial class AdminExamDatesheetPage : ViewBase<AdminExamDatesheetViewModel>
    {
        public AdminExamDatesheetPage(AdminExamDatesheetViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is AdminExamDatesheetViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
