namespace BusTracking.Mobile.Views.Coordinator
{
    public partial class CoordExamTermFormPage : ViewBase<ExamTermFormViewModel>
    {
        public CoordExamTermFormPage(ExamTermFormViewModel vm) : base(vm)
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
