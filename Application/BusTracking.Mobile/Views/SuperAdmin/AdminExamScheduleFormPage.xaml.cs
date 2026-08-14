namespace BusTracking.Mobile.Views.SuperAdmin
{
    public partial class AdminExamScheduleFormPage : ViewBase<ExamScheduleFormViewModel>
    {
        public AdminExamScheduleFormPage(ExamScheduleFormViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ExamScheduleFormViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
