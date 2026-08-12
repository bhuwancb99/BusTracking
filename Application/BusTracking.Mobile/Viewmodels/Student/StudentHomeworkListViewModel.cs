namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentHomeworkListViewModel : BaseViewModel
    {
        private readonly HomeworkMobileService _homeworkService;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty]
        private ObservableCollection<HomeworkDto> _homeworks = new();

        [ObservableProperty]
        private ObservableCollection<AcademicYearItem> _academicYears = new();

        [ObservableProperty]
        private AcademicYearItem? _selectedAcademicYear;

        [ObservableProperty]
        private DateTime _fromDate = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime _toDate = DateTime.Today;

        [ObservableProperty]
        private bool _isFromDateCalendarOpen;

        [ObservableProperty]
        private bool _isToDateCalendarOpen;

        [RelayCommand]
        public void OpenFromDateCalendar()
        {
            IsFromDateCalendarOpen = true;
        }

        [RelayCommand]
        public void OpenToDateCalendar()
        {
            IsToDateCalendarOpen = true;
        }



        public StudentHomeworkListViewModel(
            IAuthService auth,
            INavigationService nav,
            HomeworkMobileService homeworkService,
            IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            Title = "My Homework & Assignments";
            _homeworkService = homeworkService;
            _academicYearService = academicYearService;
        }

        public async Task LoadStudentHomeworksInitialDataAsync()
        {
            IsBusy = true;
            try
            {
                var activeYear = await _academicYearService.GetActiveAcademicYearAsync();
                if (activeYear != null)
                {
                    SelectedAcademicYear = activeYear;
                }
                await LoadHomeworksAsync();
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

        partial void OnFromDateChanged(DateTime value)
        {
            _ = LoadHomeworksAsync();
        }

        partial void OnToDateChanged(DateTime value)
        {
            _ = LoadHomeworksAsync();
        }

        [RelayCommand]
        public async Task LoadHomeworksAsync()
        {
            IsBusy = true;
            try
            {
                var activeYear = SelectedAcademicYear ?? await _academicYearService.GetActiveAcademicYearAsync();
                var res = await _homeworkService.GetStudentHomeworksAsync(activeYear?.AcademicYearId, FromDate, ToDate);
                if (res?.Success == true && res.Data != null)
                {
                    Homeworks = new ObservableCollection<HomeworkDto>(res.Data);
                }
                else
                {
                    Homeworks.Clear();
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
        public async Task SubmitSolutionAsync(HomeworkDto homework)
        {
            if (homework == null) return;

            string notes = await Shell.Current.DisplayPromptAsync(
                "Submit Solution",
                $"Enter optional notes/answer text for '{homework.Title}':",
                accept: "Next",
                cancel: "Cancel") ?? "";

            var action = await Shell.Current.DisplayActionSheetAsync(
                "Attach Solution File",
                "No Attachment (Text Only)",
                null,
                "Take Photo with Camera",
                "Choose Photo from Gallery",
                "Select PDF / Document File");

            Stream? stream = null;
            string? fileName = null;

            if (action != null && action != "No Attachment (Text Only)" && action != "Cancel")
            {
                FileResult? fileResult = null;
                try
                {
                    if (action == "Take Photo with Camera")
                    {
                        fileResult = await MediaPicker.Default.CapturePhotoAsync();
                    }
                    else if (action == "Choose Photo from Gallery")
                    {
                        var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
                        {
                            Title = "Select Solution Photo",
                            SelectionLimit = 1
                        });
                        fileResult = photos?.FirstOrDefault();
                    }
                    else if (action == "Select PDF / Document File")
                    {
                        fileResult = await FilePicker.Default.PickAsync(new PickOptions
                        {
                            PickerTitle = "Select Solution PDF / Document"
                        });
                    }

                    if (fileResult != null)
                    {
                        fileName = fileResult.FileName;
                        stream = await fileResult.OpenReadAsync();
                    }
                }
                catch (Exception ex)
                {
                    await ShowToastAsync($"File selection error: {ex.Message}");
                    return;
                }
            }

            IsBusy = true;
            try
            {
                var dto = new SubmitHomeworkDto
                {
                    HomeworkId = homework.HomeworkId,
                    SubmissionText = notes
                };

                var res = await _homeworkService.SubmitHomeworkAsync(dto, stream, fileName);
                if (res?.Success == true)
                {
                    await ShowToastAsync("Homework solution submitted successfully.");
                    await LoadHomeworksAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", res?.Message ?? "Failed to submit homework.", "OK");
                }
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Submit error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task OpenAttachmentAsync(HomeworkDto homework)
        {
            if (homework == null || string.IsNullOrWhiteSpace(homework.AttachmentUrl)) return;
            try
            {
                var fullUrl = homework.AttachmentUrl.StartsWith("http")
                    ? homework.AttachmentUrl
                    : $"{Constants.ApiBaseUrl.TrimEnd('/')}{homework.AttachmentUrl}";

                await Launcher.Default.OpenAsync(new Uri(fullUrl));
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Unable to open file: {ex.Message}");
            }
        }
    }
}
