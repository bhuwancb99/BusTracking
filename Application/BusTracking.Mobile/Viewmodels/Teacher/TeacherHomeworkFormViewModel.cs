namespace BusTracking.Mobile.Viewmodels.Teacher
{
    [QueryProperty(nameof(Homework), "Homework")]
    public partial class TeacherHomeworkFormViewModel : BaseViewModel
    {
        private readonly HomeworkMobileService _homeworkService;
        private readonly IAcademicYearService _academicYearService;
        private readonly IAdminStandardService _standardService;
        private readonly ISectionService _sectionService;
        private readonly IApiService _apiService;

        [ObservableProperty]
        private HomeworkDto? _homework;

        [ObservableProperty]
        private bool _isEditMode;

        public string PageTitle => IsEditMode ? "Edit Homework" : "Create Homework";

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
        private ObservableCollection<SubjectItem> _subjects = new();

        [ObservableProperty]
        private SubjectItem? _selectedSubject;

        [ObservableProperty]
        private string _homeworkTitle = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private DateTime _dueDate = DateTime.Today.AddDays(1);

        [ObservableProperty]
        private bool _isDueDateCalendarOpen;

        [RelayCommand]
        public void OpenDueDateCalendar()
        {
            IsDueDateCalendarOpen = true;
        }

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private string? _selectedFileName;


        private Stream? _fileStream;

        public TeacherHomeworkFormViewModel(
            IAuthService auth,
            INavigationService nav,
            HomeworkMobileService homeworkService,
            IAcademicYearService academicYearService,
            IAdminStandardService standardService,
            ISectionService sectionService,
            IApiService apiService)
            : base(auth, nav)
        {
            Title = "Homework Form";
            _homeworkService = homeworkService;
            _academicYearService = academicYearService;
            _standardService = standardService;
            _sectionService = sectionService;
            _apiService = apiService;
        }

        partial void OnHomeworkChanged(HomeworkDto? value)
        {
            if (value != null)
            {
                IsEditMode = true;
                HomeworkTitle = value.Title;
                Description = value.Description;
                DueDate = value.DueDate;
                IsActive = value.IsActive;
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        public async Task LoadFormDataAsync()
        {
            IsBusy = true;
            try
            {
                var years = await _academicYearService.GetAcademicYearsAsync();
                if (years != null) AcademicYears = new ObservableCollection<AcademicYearItem>(years);

                var stds = await _standardService.GetAllAsync();
                if (stds?.Items != null) Standards = new ObservableCollection<StandardItem>(stds.Items.Where(s => s.IsActive));

                var subsRes = await _apiService.GetAsync<PagedResult<SubjectItem>>(Constants.Teacher.Subjects);
                if (subsRes?.Data?.Items != null)
                {
                    Subjects = new ObservableCollection<SubjectItem>(subsRes.Data.Items.Where(s => s.IsActive));
                }
                else
                {
                    var fallbackSubs = await _apiService.GetAsync<PagedResult<SubjectItem>>(Constants.Admin.Subjects);
                    if (fallbackSubs?.Data?.Items != null)
                        Subjects = new ObservableCollection<SubjectItem>(fallbackSubs.Data.Items.Where(s => s.IsActive));
                }

                if (IsEditMode && Homework != null)
                {
                    SelectedAcademicYear = AcademicYears.FirstOrDefault(y => y.AcademicYearId == Homework.AcademicYearId);
                    SelectedStandard = Standards.FirstOrDefault(s => s.StandardId == Homework.StandardId);
                    if (Homework.SubjectId.HasValue)
                        SelectedSubject = Subjects.FirstOrDefault(s => s.SubjectId == Homework.SubjectId.Value);

                    if (SelectedStandard != null)
                    {
                        await LoadSectionsAsync(SelectedStandard.StandardId);
                        if (Homework.SectionId.HasValue)
                            SelectedSection = Sections.FirstOrDefault(sec => sec.SectionId == Homework.SectionId.Value);
                    }
                }
                else
                {
                    SelectedAcademicYear = AcademicYears.FirstOrDefault(y => y.IsCurrent) ?? AcademicYears.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Error loading form: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            _ = LoadSectionsAsync(value?.StandardId);
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
            if (secList != null) Sections = new ObservableCollection<SectionItem>(secList);
            else Sections.Clear();
        }

        [RelayCommand]
        public async Task PickFileAsync()
        {
            try
            {
                var action = await Shell.Current.DisplayActionSheetAsync(
                    "Attach File to Homework",
                    "Cancel",
                    null,
                    "Take Photo with Camera",
                    "Choose Photo from Gallery",
                    "Select Document / PDF File");

                if (action == null || action == "Cancel") return;

                FileResult? fileResult = null;

                if (action == "Take Photo with Camera")
                {
                    try
                    {
                        fileResult = await MediaPicker.Default.CapturePhotoAsync();
                    }
                    catch (Exception ex)
                    {
                        await ShowToastAsync($"Camera error: {ex.Message}");
                        return;
                    }
                }
                else if (action == "Choose Photo from Gallery")
                {
                    try
                    {
                        var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
                        {
                            Title = "Select Homework Photo",
                            SelectionLimit = 1
                        });
                        fileResult = photos?.FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        await ShowToastAsync($"Gallery error: {ex.Message}");
                        return;
                    }
                }
                else if (action == "Select Document / PDF File")
                {
                    try
                    {
                        fileResult = await FilePicker.Default.PickAsync(new PickOptions
                        {
                            PickerTitle = "Select Homework Document (PDF / Doc)"
                        });
                    }
                    catch (Exception ex)
                    {
                        await ShowToastAsync($"Document picker error: {ex.Message}");
                        return;
                    }
                }

                if (fileResult != null)
                {
                    SelectedFileName = fileResult.FileName;
                    _fileStream = await fileResult.OpenReadAsync();
                }
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"File selection error: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task SaveHomeworkAsync()
        {
            if (SelectedAcademicYear == null || SelectedStandard == null || string.IsNullOrWhiteSpace(HomeworkTitle) || string.IsNullOrWhiteSpace(Description))
            {
                await ShowToastAsync("Please fill in Academic Session, Standard, Title, and Instructions.");
                return;
            }

            IsBusy = true;
            try
            {
                if (IsEditMode && Homework != null)
                {
                    var dto = new UpdateHomeworkDto
                    {
                        HomeworkId = Homework.HomeworkId,
                        AcademicYearId = SelectedAcademicYear.AcademicYearId,
                        StandardId = SelectedStandard.StandardId,
                        SectionId = SelectedSection?.SectionId,
                        SubjectId = SelectedSubject?.SubjectId,
                        Title = HomeworkTitle.Trim(),
                        Description = Description.Trim(),
                        DueDate = DueDate,
                        IsActive = IsActive
                    };

                    var res = await _homeworkService.UpdateHomeworkAsync(Homework.HomeworkId, dto, _fileStream, SelectedFileName);
                    if (res?.Success == true)
                    {
                        await ShowToastAsync("Homework updated successfully.");
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlertAsync("Error", res?.Message ?? "Failed to update homework.", "OK");
                    }
                }
                else
                {
                    var dto = new CreateHomeworkDto
                    {
                        AcademicYearId = SelectedAcademicYear.AcademicYearId,
                        StandardId = SelectedStandard.StandardId,
                        SectionId = SelectedSection?.SectionId,
                        SubjectId = SelectedSubject?.SubjectId,
                        Title = HomeworkTitle.Trim(),
                        Description = Description.Trim(),
                        DueDate = DueDate,
                        IsActive = IsActive
                    };

                    var res = await _homeworkService.CreateHomeworkAsync(dto, _fileStream, SelectedFileName);
                    if (res?.Success == true)
                    {
                        await ShowToastAsync(IsActive ? "Homework created & push notification sent." : "Homework saved as Draft/Inactive.");
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlertAsync("Error", res?.Message ?? "Failed to create homework.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Save error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
