namespace BusTracking.Mobile.Views.Teacher
{
    public partial class TeacherHomeworkListPage : ViewBase<TeacherHomeworkListViewModel>
    {
        public TeacherHomeworkListPage(TeacherHomeworkListViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel != null)
            {
                await ViewModel.LoadInitialDataAsync();
            }
        }
    }
}
