namespace BusTracking.Mobile.Views.Coordinator
{
    public partial class CoordExamScheduleFormPage : ViewBase<ExamScheduleFormViewModel>
    {
        public CoordExamScheduleFormPage(ExamScheduleFormViewModel vm) : base(vm)
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
