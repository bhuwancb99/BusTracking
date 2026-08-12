namespace BusTracking.Mobile.Viewmodels.Teacher
{
    [QueryProperty(nameof(Homework), "Homework")]
    public partial class TeacherHomeworkSubmissionsViewModel : BaseViewModel
    {
        private readonly HomeworkMobileService _homeworkService;

        [ObservableProperty]
        private HomeworkDto? _homework;

        [ObservableProperty]
        private ObservableCollection<HomeworkSubmissionDto> _submissions = new();

        public TeacherHomeworkSubmissionsViewModel(
            IAuthService auth,
            INavigationService nav,
            HomeworkMobileService homeworkService)
            : base(auth, nav)
        {
            Title = "Submissions Evaluation";
            _homeworkService = homeworkService;
        }

        partial void OnHomeworkChanged(HomeworkDto? value)
        {
            if (value != null)
            {
                _ = LoadSubmissionsAsync(value.HomeworkId);
            }
        }

        [RelayCommand]
        public async Task LoadSubmissionsAsync(int homeworkId)
        {
            if (homeworkId <= 0) return;
            IsBusy = true;
            try
            {
                var res = await _homeworkService.GetSubmissionsAsync(homeworkId);
                if (res?.Success == true && res.Data != null)
                {
                    Submissions = new ObservableCollection<HomeworkSubmissionDto>(res.Data);
                }
                else
                {
                    Submissions.Clear();
                    await ShowToastAsync(res?.Message ?? "No submissions found.");
                }
            }
            catch (Exception ex)
            {
                await ShowToastAsync($"Error loading submissions: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task EvaluateAsync(HomeworkSubmissionDto submission)
        {
            if (submission == null) return;

            if (Application.Current?.Windows[0].Page is Page page)
            {
                var popup = new EvaluateSubmissionPopup(submission);
                var result = await page.ShowPopupAsync<EvaluateHomeworkSubmissionDto?>(popup);
                var evalResult = result?.Result;
                if (evalResult != null)
                {
                    IsBusy = true;
                    try
                    {
                        var res = await _homeworkService.EvaluateSubmissionAsync(evalResult);
                        if (res?.Success == true)
                        {
                            submission.MarksObtained = evalResult.MarksObtained;
                            submission.TeacherRemarks = evalResult.TeacherRemarks;
                            submission.Status = "Evaluated";
                            await ShowToastAsync("Submission evaluated successfully.");
                            if (Homework != null)
                            {
                                await LoadSubmissionsAsync(Homework.HomeworkId);
                            }
                        }
                        else
                        {
                            await Shell.Current.DisplayAlertAsync("Error", res?.Message ?? "Failed to evaluate submission.", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        await ShowToastAsync($"Evaluation error: {ex.Message}");
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
            }
        }
    }
}
