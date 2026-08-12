namespace BusTracking.Mobile.Viewmodels.Teacher
{
    public partial class TeacherHomeworkListViewModel : BaseViewModel
    {
        private readonly HomeworkMobileService _homeworkService;
        private readonly IAcademicYearService _academicYearService;
        private readonly IAdminStandardService _standardService;
        private readonly ISectionService _sectionService;

        [ObservableProperty]
        private ObservableCollection<AcademicYearItem> _academicYears = new();

        [ObservableProperty]
        private AcademicYearItem? _selectedAcademicYear;

        [ObservableProperty]
        private ObservableCollection<StandardItem> _standards = new();

        [ObservableProperty]
        private StandardItem? _selectedStandard;

        [ObservableProperty]
        private ObservableCollection<SectionItem> _sections = new();

        [ObservableProperty]
        private SectionItem? _selectedSection;

        [ObservableProperty]
        private ObservableCollection<HomeworkDto> _homeworks = new();

        [ObservableProperty]
        private bool _hasFilter = false;

        public TeacherHomeworkListViewModel(
            IAuthService auth,
            INavigationService nav,
            HomeworkMobileService homeworkService,
            IAcademicYearService academicYearService,
            IAdminStandardService standardService,
            ISectionService sectionService)
            : base(auth, nav)
        {
            Title = "Homework Assignments";
            _homeworkService = homeworkService;
            _academicYearService = academicYearService;
            _standardService = standardService;
            _sectionService = sectionService;
        }

        public async Task LoadInitialDataAsync()
        {
            IsBusy = true;
            try
            {
                var yearsRes = await _academicYearService.GetAcademicYearsAsync();
                if (yearsRes != null)
                {
                    AcademicYears = new ObservableCollection<AcademicYearItem>(yearsRes);
                    SelectedAcademicYear = AcademicYears.FirstOrDefault(y => y.IsCurrent) ?? AcademicYears.FirstOrDefault();
                }

                var stdRes = await _standardService.GetAllAsync();
                if (stdRes?.Items != null)
                {
                    Standards = new ObservableCollection<StandardItem>(stdRes.Items.Where(s => s.IsActive));
                    SelectedStandard = Standards.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Initialization error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSelectedAcademicYearChanged(AcademicYearItem? value)
        {
            if (value != null && SelectedStandard != null && SelectedSection != null)
            {
                _ = LoadHomeworksAsync();
            }
        }

        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            _ = LoadSectionsAndHomeworksAsync(value?.StandardId);
        }

        private async Task LoadSectionsAndHomeworksAsync(int? standardId)
        {
            await LoadSectionsAsync(standardId);
            if (SelectedSection == null && Sections.Count > 0)
            {
                SelectedSection = Sections.FirstOrDefault();
            }
            else if (SelectedAcademicYear != null && SelectedStandard != null && SelectedSection != null)
            {
                await LoadHomeworksAsync();
            }
        }

        partial void OnSelectedSectionChanged(SectionItem? value)
        {
            if (value != null && SelectedAcademicYear != null && SelectedStandard != null)
            {
                _ = LoadHomeworksAsync();
            }
        }

        private async Task LoadSectionsAsync(int? standardId)
        {
            if (!standardId.HasValue || standardId.Value <= 0)
            {
                Sections.Clear();
                SelectedSection = null;
                return;
            }

            var secList = await _sectionService.GetByStandardAsync(standardId.Value);
            if (secList != null)
            {
                Sections = new ObservableCollection<SectionItem>(secList);
            }
            else
            {
                Sections.Clear();
            }
        }

        [RelayCommand]
        public async Task LoadHomeworksAsync()
        {
            if (SelectedAcademicYear == null || SelectedStandard == null || SelectedSection == null)
            {
                HasFilter = false;
                Homeworks.Clear();
                return;
            }

            IsBusy = true;
            try
            {
                var res = await _homeworkService.GetTeacherHomeworksAsync(
                    SelectedAcademicYear.AcademicYearId,
                    SelectedStandard.StandardId,
                    SelectedSection.SectionId);

                if (res?.Success == true && res.Data != null)
                {
                    Homeworks = new ObservableCollection<HomeworkDto>(res.Data);
                    HasFilter = true;
                }
                else
                {
                    Homeworks.Clear();
                    HasFilter = true;
                }
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Error loading homework: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task GoToCreateAsync()
        {
            await Shell.Current.GoToAsync("TeacherHomeworkForm");
        }

        [RelayCommand]
        public async Task GoToEditAsync(HomeworkDto homework)
        {
            if (homework == null) return;
            var param = new Dictionary<string, object> { { "Homework", homework } };
            await Shell.Current.GoToAsync("TeacherHomeworkForm", param);
        }

        [RelayCommand]
        public async Task GoToSubmissionsAsync(HomeworkDto homework)
        {
            if (homework == null) return;
            var param = new Dictionary<string, object> { { "Homework", homework } };
            await Shell.Current.GoToAsync("TeacherHomeworkSubmissions", param);
        }

        [RelayCommand]
        public async Task DeleteHomeworkAsync(HomeworkDto homework)
        {
            if (homework == null) return;
            bool confirmed = await ConfirmAsync(
                "Delete Homework",
                $"Are you sure you want to delete assignment '{homework.Title}'?",
                "Yes, Delete",
                "Cancel");

            if (!confirmed) return;

            IsBusy = true;
            try
            {
                var res = await _homeworkService.DeleteHomeworkAsync(homework.HomeworkId);
                if (res?.Success == true)
                {
                    Homeworks.Remove(homework);
                    await ShowToastAsync("Homework deleted successfully.");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Cannot Delete", res?.Message ?? "Failed to delete homework.", "OK");
                }
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Delete error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
