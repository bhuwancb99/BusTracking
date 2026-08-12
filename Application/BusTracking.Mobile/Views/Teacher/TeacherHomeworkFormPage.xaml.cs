namespace BusTracking.Mobile.Views.Teacher
{
    public partial class TeacherHomeworkFormPage : ViewBase<TeacherHomeworkFormViewModel>
    {
        public TeacherHomeworkFormPage(TeacherHomeworkFormViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel != null)
            {
                await ViewModel.LoadFormDataAsync();
            }
        }
    }
}
