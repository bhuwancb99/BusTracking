namespace BusTracking.Mobile.Views.Student
{
    public partial class StudentHomeworkListPage : ViewBase<StudentHomeworkListViewModel>
    {
        public StudentHomeworkListPage(StudentHomeworkListViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel != null)
            {
                await ViewModel.LoadStudentHomeworksInitialDataAsync();
            }
        }
    }
}
