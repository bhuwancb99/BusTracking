namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminParentFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IParentService _parents;

        [ObservableProperty] private int? _userId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private string _studentCodes = "";   // comma-separated
        [ObservableProperty] private bool _isActive = true;

        public AdminParentFormViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UserId", out var id))
            {
                UserId = (int)id;
                IsEditMode = true;
                Title = "Edit Parent";
            }
            else Title = "Add Parent";
        }

        public override async Task InitializeAsync()
        {
            if (!IsEditMode || !UserId.HasValue) return;

            await RunAsync(async () =>
            {
                var p = await _parents.GetByIdAsync(UserId.Value);
                if (p is null) return;
                FullName = p.FullName;
                Email = p.Email;
                PhoneNumber = p.PhoneNumber ?? "";
                IsActive = p.IsActive;
                StudentCodes = string.Join(", ", p.Students.Select(s => s.StudentCode));
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email))
            {
                SetError("Full name and email are required."); return;
            }
            if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
            {
                SetError("Password is required for new parents."); return;
            }

            var codes = StudentCodes
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => c.Length > 0)
                .ToList();

            await RunAsync(async () =>
            {
                ApiResponse<object> r;

                if (IsEditMode)
                {
                    r = await _parents.UpdateAsync(UserId!.Value, new UpdateParentRequest
                    {
                        FullName = FullName,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        StudentCodes = codes,
                        IsActive = IsActive
                    });
                }
                else
                {
                    r = await _parents.CreateAsync(new CreateParentRequest
                    {
                        FullName = FullName,
                        Email = Email,
                        Password = Password,
                        PhoneNumber = PhoneNumber.Length > 0 ? PhoneNumber : null,
                        StudentCodes = codes,
                        IsActive = IsActive
                    });
                }

                if (r.Success)
                {
                    await ShowToastAsync(IsEditMode ? "Parent updated." : "Parent created.");
                    await Nav.GoBackAsync();
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}