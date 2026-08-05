namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordSubjectFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ISubjectService _subjectService;

        [ObservableProperty] private int _subjectId;
        [ObservableProperty] private string _subjectName = "";
        [ObservableProperty] private string _subjectCode = "";
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private bool _isEditMode;

        public CoordSubjectFormViewModel(IAuthService auth, INavigationService nav, ISubjectService subjectService)
            : base(auth, nav)
        {
            _subjectService = subjectService;
            Title = "Add Subject";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("SubjectId", out var id))
            {
                SubjectId = (int)id;
                IsEditMode = true;
                Title = "Edit Subject";
            }
        }

        public override async Task InitializeAsync()
        {
            if (IsEditMode)
            {
                await RunAsync(async () =>
                {
                    var s = await _subjectService.GetByIdAsync(SubjectId, isCoordinator: true);
                    if (s != null)
                    {
                        SubjectName = s.SubjectName;
                        SubjectCode = s.SubjectCode ?? "";
                        IsActive = s.IsActive;
                    }
                });
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(SubjectName)) { SetError("Subject Name is required."); return; }

            await RunAsync(async () =>
            {
                ApiResponse<object> r;
                if (IsEditMode)
                {
                    r = await _subjectService.UpdateAsync(SubjectId, new UpdateSubjectRequest
                    {
                        SubjectName = SubjectName.Trim(),
                        SubjectCode = SubjectCode.Trim(),
                        IsActive = IsActive
                    }, isCoordinator: true);
                }
                else
                {
                    r = await _subjectService.CreateAsync(new CreateSubjectRequest
                    {
                        SubjectName = SubjectName.Trim(),
                        SubjectCode = SubjectCode.Trim()
                    }, isCoordinator: true);
                }

                if (r.Success)
                {
                    await ShowToastAsync(IsEditMode ? "Subject updated." : "Subject created.");
                    await Nav.GoBackAsync();
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
