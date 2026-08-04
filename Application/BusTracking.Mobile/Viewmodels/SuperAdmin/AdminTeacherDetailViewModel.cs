namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    [QueryProperty(nameof(TeacherId), "TeacherId")]
    [QueryProperty(nameof(Teacher), "Teacher")]
    public partial class AdminTeacherDetailViewModel : BaseViewModel
    {
        private readonly ITeacherService _teacherService;

        [ObservableProperty] private int _teacherId;
        [ObservableProperty] private TeacherItem? _teacher;

        public bool CanEdit => true;
        public bool CanDelete => true;

        public AdminTeacherDetailViewModel(IAuthService auth, INavigationService nav, ITeacherService teacherService)
            : base(auth, nav)
        {
            _teacherService = teacherService;
            Title = "Teacher Details";
        }

        partial void OnTeacherIdChanged(int value)
        {
            if (value > 0 && Teacher == null)
            {
                _ = LoadDetailsAsync(value);
            }
        }

        private async Task LoadDetailsAsync(int id)
        {
            await RunAsync(async () =>
            {
                Teacher = await _teacherService.GetTeacherByIdAsync(id, isCoordinator: false);
            });
        }

        [RelayCommand]
        private Task EditAsync()
        {
            if (Teacher != null)
            {
                return Nav.GoToAsync("AdminTeacherForm", new Dictionary<string, object> { ["Teacher"] = Teacher, ["TeacherId"] = Teacher.TeacherId });
            }
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (Teacher == null) return;
            bool confirm = await ConfirmAsync("Delete Teacher", $"Are you sure you want to delete {Teacher.FullName}?", "Delete", "Cancel");
            if (!confirm) return;

            await RunAsync(async () =>
            {
                var res = await _teacherService.DeleteTeacherAsync(Teacher.TeacherId, isCoordinator: false);
                if (res.Success)
                {
                    await ShowToastAsync("Teacher account deleted.");
                    await Nav.GoBackAsync();
                }
                else
                {
                    SetError(res.Message);
                }
            });
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (Teacher == null) return;
            bool confirm = await ConfirmAsync("Reset Password", $"Are you sure you want to reset password for {Teacher.FullName}?", "Reset", "Cancel");
            if (!confirm) return;

            await RunAsync(async () =>
            {
                var res = await _teacherService.ResetPasswordAsync(Teacher.TeacherId, isCoordinator: false);
                if (res.Success && res.Data != null)
                {
                    var pwd = !string.IsNullOrWhiteSpace(res.Data.Password) ? res.Data.Password : (res.Data.PlainPassword ?? "");
                    await ShowAlertAsync("Password Reset Successful", $"Username: {res.Data.UserName}\nNew Password: {pwd}");
                }
                else
                {
                    SetError(res.Message);
                }
            });
        }

        [RelayCommand]
        private async Task ToggleStatusAsync()
        {
            if (Teacher == null) return;
            await RunAsync(async () =>
            {
                var res = await _teacherService.ToggleTeacherStatusAsync(Teacher.TeacherId, isCoordinator: false);
                if (res.Success)
                {
                    Teacher.IsActive = !Teacher.IsActive;
                    OnPropertyChanged(nameof(Teacher));
                    await ShowToastAsync(res.Message);
                }
                else
                {
                    SetError(res.Message);
                }
            });
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (TeacherId > 0)
            {
                await LoadDetailsAsync(TeacherId);
            }
        }
    }
}
