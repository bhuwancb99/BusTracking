namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminSectionFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ISectionService _sectionService;
        private readonly IAdminStandardService _standardService;
        private readonly ITeacherService _teacherService;

        [ObservableProperty] private int _sectionId;
        [ObservableProperty] private int _standardId;
        [ObservableProperty] private string _standardName = "";
        [ObservableProperty] private string _sectionName = "";
        [ObservableProperty] private ObservableCollection<StandardItem> _standards = [];
        [ObservableProperty] private StandardItem? _selectedStandard;
        [ObservableProperty] private ObservableCollection<TeacherItem> _teachers = [];
        [ObservableProperty] private TeacherItem? _selectedTeacher;
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private bool _isEditMode;

        public AdminSectionFormViewModel(
            IAuthService auth,
            INavigationService nav,
            ISectionService sectionService,
            IAdminStandardService standardService,
            ITeacherService teacherService)
            : base(auth, nav)
        {
            _sectionService = sectionService;
            _standardService = standardService;
            _teacherService = teacherService;
            Title = "Add Section";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("StandardId", out var stdId)) StandardId = Convert.ToInt32(stdId);
            if (query.TryGetValue("StandardName", out var stdName)) StandardName = Convert.ToString(stdName) ?? "";

            if (query.TryGetValue("SectionId", out var secId) && Convert.ToInt32(secId) > 0)
            {
                SectionId = Convert.ToInt32(secId);
                IsEditMode = true;
                Title = "Edit Section";
            }
            else
            {
                SectionId = 0;
                IsEditMode = false;
                Title = "Add Section";
                SectionName = "";
            }
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var stdsData = await _standardService.GetAllAsync(null, 1);
                Standards = new ObservableCollection<StandardItem>(stdsData.Items ?? new());

                if (StandardId > 0)
                {
                    SelectedStandard = Standards.FirstOrDefault(s => s.StandardId == StandardId) ?? Standards.FirstOrDefault();
                }
                else
                {
                    SelectedStandard = Standards.FirstOrDefault();
                }

                var tData = await _teacherService.GetTeachersAsync(1);
                Teachers = new ObservableCollection<TeacherItem>(tData.Items ?? new());

                if (IsEditMode && SectionId > 0)
                {
                    var res = await _sectionService.GetByIdAsync(SectionId, isCoordinator: false);
                    if (res.Success && res.Data != null)
                    {
                        SectionName = res.Data.SectionName;
                        IsActive = res.Data.IsActive;
                        SelectedStandard = Standards.FirstOrDefault(s => s.StandardId == res.Data.StandardId) ?? SelectedStandard;
                        if (res.Data.ClassTeacherId.HasValue)
                        {
                            SelectedTeacher = Teachers.FirstOrDefault(t => t.TeacherId == res.Data.ClassTeacherId.Value);
                        }
                    }
                }
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            var targetStandardId = SelectedStandard?.StandardId ?? StandardId;
            if (targetStandardId <= 0)
            {
                SetError("Please select a Class / Standard.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SectionName))
            {
                SetError("Section name is required.");
                return;
            }

            await RunAsync(async () =>
            {
                if (IsEditMode)
                {
                    var req = new UpdateSectionRequest
                    {
                        SectionId = SectionId,
                        SectionName = SectionName.Trim().ToUpper(),
                        ClassTeacherId = SelectedTeacher?.TeacherId,
                        IsActive = IsActive
                    };

                    var r = await _sectionService.UpdateAsync(SectionId, req, isCoordinator: false);
                    if (r.Success)
                    {
                        await ShowToastAsync("Section updated successfully.");
                        await Nav.GoBackAsync();
                    }
                    else SetError(r.Message);
                }
                else
                {
                    var req = new CreateSectionRequest
                    {
                        StandardId = targetStandardId,
                        SectionName = SectionName.Trim().ToUpper(),
                        ClassTeacherId = SelectedTeacher?.TeacherId
                    };

                    var r = await _sectionService.CreateAsync(req, isCoordinator: false);
                    if (r.Success)
                    {
                        await ShowToastAsync("Section created successfully.");
                        await Nav.GoBackAsync();
                    }
                    else SetError(r.Message);
                }
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
