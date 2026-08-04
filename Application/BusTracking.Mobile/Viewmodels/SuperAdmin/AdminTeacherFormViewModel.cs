namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    [QueryProperty(nameof(TeacherId), "TeacherId")]
    [QueryProperty(nameof(Teacher), "Teacher")]
    public partial class AdminTeacherFormViewModel : BaseViewModel
    {
        private readonly ITeacherService _teacherService;

        [ObservableProperty] private int _teacherId;
        [ObservableProperty] private TeacherItem? _teacher;
        [ObservableProperty] private bool _isEditMode;

        // Form Fields
        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _userName = "";
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _phoneNumber = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _newPassword = "";
        [ObservableProperty] private string _employeeCode = "";
        [ObservableProperty] private string _qualification = "";
        [ObservableProperty] private string _designation = "";
        [ObservableProperty] private string _department = "";
        [ObservableProperty] private DateTime _joiningDate = DateTime.Today;
        [ObservableProperty] private string _gender = "Male";
        [ObservableProperty] private string _emergencyContact = "";
        [ObservableProperty] private bool _isActive = true;

        public List<string> GenderOptions => ["Male", "Female", "Other"];

        public AdminTeacherFormViewModel(IAuthService auth, INavigationService nav, ITeacherService teacherService)
            : base(auth, nav)
        {
            _teacherService = teacherService;
            Title = "Add Teacher";
        }

        partial void OnTeacherIdChanged(int value)
        {
            if (value > 0)
            {
                IsEditMode = true;
                Title = "Edit Teacher";
                _ = LoadTeacherDetailsAsync(value);
            }
        }

        partial void OnTeacherChanged(TeacherItem? value)
        {
            if (value != null)
            {
                PopulateFields(value);
            }
        }

        private async Task LoadTeacherDetailsAsync(int id)
        {
            await RunAsync(async () =>
            {
                var t = await _teacherService.GetTeacherByIdAsync(id, isCoordinator: false);
                if (t != null)
                {
                    PopulateFields(t);
                }
            });
        }

        private void PopulateFields(TeacherItem t)
        {
            TeacherId = t.TeacherId;
            FullName = t.FullName;
            UserName = t.UserName;
            Email = t.Email ?? "";
            PhoneNumber = t.PhoneNumber ?? "";
            EmployeeCode = t.EmployeeCode ?? "";
            Qualification = t.Qualification ?? "";
            Designation = t.Designation ?? "";
            Department = t.Department ?? "";
            JoiningDate = t.JoiningDate ?? DateTime.Today;
            Gender = string.IsNullOrWhiteSpace(t.Gender) ? "Male" : t.Gender;
            EmergencyContact = t.EmergencyContact ?? "";
            IsActive = t.IsActive;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                SetError("Full Name is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(UserName))
            {
                SetError("Username is required.");
                return;
            }
            if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
            {
                SetError("Password is required for new teacher account.");
                return;
            }

            await RunAsync(async () =>
            {
                if (IsEditMode)
                {
                    var effectivePwd = !string.IsNullOrWhiteSpace(NewPassword) ? NewPassword.Trim() : (string.IsNullOrWhiteSpace(Password) ? null : Password.Trim());
                    var req = new UpdateTeacherRequest
                    {
                        TeacherId = TeacherId,
                        FullName = FullName.Trim(),
                        UserName = UserName.Trim(),
                        Email = Email.Trim(),
                        PhoneNumber = PhoneNumber.Trim(),
                        Password = effectivePwd,
                        EmployeeCode = EmployeeCode.Trim(),
                        Qualification = Qualification.Trim(),
                        Designation = Designation.Trim(),
                        Department = Department.Trim(),
                        JoiningDate = JoiningDate,
                        Gender = Gender,
                        EmergencyContact = EmergencyContact.Trim(),
                        IsActive = IsActive
                    };
                    var res = await _teacherService.UpdateTeacherAsync(TeacherId, req, isCoordinator: false);
                    if (res.Success)
                    {
                        await Nav.GoBackAsync();
                    }
                    else
                    {
                        SetError(res.Message);
                    }
                }
                else
                {
                    var req = new CreateTeacherRequest
                    {
                        FullName = FullName.Trim(),
                        UserName = UserName.Trim(),
                        Email = Email.Trim(),
                        PhoneNumber = PhoneNumber.Trim(),
                        Password = Password.Trim(),
                        EmployeeCode = EmployeeCode.Trim(),
                        Qualification = Qualification.Trim(),
                        Designation = Designation.Trim(),
                        Department = Department.Trim(),
                        JoiningDate = JoiningDate,
                        Gender = Gender,
                        EmergencyContact = EmergencyContact.Trim(),
                        IsActive = IsActive
                    };
                    var res = await _teacherService.CreateTeacherAsync(req, isCoordinator: false);
                    if (res.Success)
                    {
                        await Nav.GoBackAsync();
                    }
                    else
                    {
                        SetError(res.Message);
                    }
                }
            });
        }
    }
}
