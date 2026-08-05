namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminSectionFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ISectionService _sectionService;
        private readonly ITeacherService _teacherService;

        [ObservableProperty] private int _standardId;
        [ObservableProperty] private string _standardName = "";
        [ObservableProperty] private string _sectionName = "B";
        [ObservableProperty] private ObservableCollection<TeacherItem> _teachers = [];
        [ObservableProperty] private TeacherItem? _selectedTeacher;

        public AdminSectionFormViewModel(IAuthService auth, INavigationService nav, ISectionService sectionService, ITeacherService teacherService)
            : base(auth, nav)
        {
            _sectionService = sectionService;
            _teacherService = teacherService;
            Title = "Add Section";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("StandardId", out var stdId)) StandardId = (int)stdId;
            if (query.TryGetValue("StandardName", out var stdName)) StandardName = (string)stdName;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var tData = await _teacherService.GetTeachersAsync(1);
                Teachers = new ObservableCollection<TeacherItem>(tData.Items ?? new());
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(SectionName))
            {
                SetError("Section name is required.");
                return;
            }

            await RunAsync(async () =>
            {
                var r = await _sectionService.CreateAsync(new CreateSectionRequest
                {
                    StandardId = StandardId,
                    SectionName = SectionName.Trim(),
                    ClassTeacherId = SelectedTeacher?.TeacherId
                }, isCoordinator: false);

                if (r.Success)
                {
                    await ShowToastAsync("Section created successfully.");
                    await Nav.GoBackAsync();
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
