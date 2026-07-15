namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminStandardFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IAdminStandardService _standardService;

        [ObservableProperty] private int? _standardId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _standardName = "";
        [ObservableProperty] private bool _isActive = true;

        public AdminStandardFormViewModel(IAuthService auth, INavigationService nav, IAdminStandardService standardService)
            : base(auth, nav)
        {
            _standardService = standardService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("StandardId", out var id))
            {
                StandardId = (int)id;
                IsEditMode = true;
                Title = "Edit Standard";
            }
            else
            {
                Title = "Add Standard";
            }
        }

        public override async Task InitializeAsync()
        {
            if (!IsEditMode || !StandardId.HasValue) return;
            await RunAsync(async () =>
            {
                var s = await _standardService.GetByIdAsync(StandardId.Value);
                if (s is null) return;
                StandardName = s.StandardName;
                IsActive = s.IsActive;
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(StandardName))
            {
                SetError("Standard name is required.");
                return;
            }

            await RunAsync(async () =>
            {
                ApiResponse<object> r;
                if (IsEditMode)
                {
                    r = await _standardService.UpdateAsync(StandardId!.Value, new UpdateStandardRequest
                    {
                        StandardName = StandardName,
                        IsActive = IsActive
                    });
                }
                else
                {
                    r = await _standardService.CreateAsync(new CreateStandardRequest
                    {
                        StandardName = StandardName,
                        IsActive = IsActive
                    });
                }

                if (r.Success)
                {
                    await ShowToastAsync(IsEditMode ? "Standard updated." : "Standard created.");
                    await Nav.GoBackAsync();
                }
                else
                {
                    SetError(r.Message);
                }
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
