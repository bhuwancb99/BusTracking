namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminClassMappingFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IClassMappingService _mappingService;
        private readonly IAcademicYearService _yearService;
        private readonly IAdminStandardService _standardService;
        private readonly ISectionService _sectionService;
        private readonly ISubjectService _subjectService;
        private readonly ITeacherService _teacherService;

        [ObservableProperty] private int _mappingId;
        [ObservableProperty] private bool _isEditMode;

        [ObservableProperty] private ObservableCollection<AcademicYearItem> _academicYears = [];
        [ObservableProperty] private AcademicYearItem? _selectedYear;

        [ObservableProperty] private ObservableCollection<StandardItem> _standards = [];
        [ObservableProperty] private StandardItem? _selectedStandard;

        [ObservableProperty] private ObservableCollection<SectionItem> _sections = [];
        [ObservableProperty] private SectionItem? _selectedSection;

        [ObservableProperty] private ObservableCollection<SubjectItem> _subjects = [];
        [ObservableProperty] private SubjectItem? _selectedSubject;

        [ObservableProperty] private ObservableCollection<TeacherItem> _teachers = [];
        [ObservableProperty] private TeacherItem? _selectedTeacher;

        public AdminClassMappingFormViewModel(
            IAuthService auth,
            INavigationService nav,
            IClassMappingService mappingService,
            IAcademicYearService yearService,
            IAdminStandardService standardService,
            ISectionService sectionService,
            ISubjectService subjectService,
            ITeacherService teacherService)
            : base(auth, nav)
        {
            _mappingService = mappingService;
            _yearService = yearService;
            _standardService = standardService;
            _sectionService = sectionService;
            _subjectService = subjectService;
            _teacherService = teacherService;
            Title = "Assign Subject & Teacher";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("MappingId", out var id))
            {
                MappingId = (int)id;
                IsEditMode = true;
                Title = "Edit Subject & Teacher Mapping";
            }
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var years = await _yearService.GetAcademicYearsAsync(false);
                AcademicYears = new ObservableCollection<AcademicYearItem>(years);
                SelectedYear = AcademicYears.FirstOrDefault(y => y.IsCurrent) ?? AcademicYears.FirstOrDefault();

                var stds = await _standardService.GetAllAsync(null, 1);
                Standards = new ObservableCollection<StandardItem>(stds.Items);

                var subs = await _subjectService.GetAllAsync(null, 1, isCoordinator: false);
                Subjects = new ObservableCollection<SubjectItem>(subs.Items);

                var tData = await _teacherService.GetTeachersAsync(1);
                Teachers = new ObservableCollection<TeacherItem>(tData.Items ?? new());
            });
        }

        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            if (value != null) _ = LoadSectionsAsync(value.StandardId);
        }

        private async Task LoadSectionsAsync(int standardId)
        {
            var data = await _sectionService.GetByStandardAsync(standardId, isCoordinator: false);
            Sections = new ObservableCollection<SectionItem>(data);
            SelectedSection = Sections.FirstOrDefault();
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (SelectedYear is null) { SetError("Academic Session is required."); return; }
            if (SelectedStandard is null) { SetError("Standard is required."); return; }
            if (SelectedSection is null) { SetError("Section is required."); return; }
            if (SelectedSubject is null) { SetError("Subject is required."); return; }
            if (SelectedTeacher is null) { SetError("Teacher is required."); return; }

            await RunAsync(async () =>
            {
                var req = new AssignClassMappingRequest
                {
                    AcademicYearId = SelectedYear.AcademicYearId,
                    StandardId = SelectedStandard.StandardId,
                    SectionId = SelectedSection.SectionId,
                    SubjectId = SelectedSubject.SubjectId,
                    TeacherId = SelectedTeacher.TeacherId
                };

                var r = await _mappingService.AssignAsync(req, isCoordinator: false);

                if (r.Success)
                {
                    await ShowToastAsync("Subject & Teacher assigned.");
                    await Nav.GoBackAsync();
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
