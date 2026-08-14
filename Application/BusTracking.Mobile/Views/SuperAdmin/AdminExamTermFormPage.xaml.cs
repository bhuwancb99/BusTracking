namespace BusTracking.Mobile.Views.SuperAdmin
{
    public partial class AdminExamTermFormPage : ViewBase<ExamTermFormViewModel>
    {
        public AdminExamTermFormPage(ExamTermFormViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ExamTermFormViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
