namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminParentDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IParentService _parents;

        [ObservableProperty] private int _userId;
        [ObservableProperty] private ParentItem? _parent;

        public bool CanEdit => Can("parent.edit");
        public bool CanDelete => Can("parent.delete");

        public AdminParentDetailViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; Title = "Parent Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("ParentId", out var id)) UserId = (int)id;
            else if (query.TryGetValue("UserId", out var uid)) UserId = (int)uid;
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                Parent = await _parents.GetByIdAsync(UserId);
            });
        }

        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("AdminParentForm", new Dictionary<string, object> { ["UserId"] = UserId });

        [RelayCommand]
        private async Task ToggleAsync()
        {
            var r = await _parents.ToggleAsync(UserId);
            if (r.Success) { await ShowToastAsync(r.Message); await LoadAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {Parent?.FullName}?")) return;
            var r = await _parents.ResetPasswordAsync(UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", r.Message);
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!await ConfirmAsync("Delete Parent", $"Delete '{Parent?.FullName}'?")) return;
            var r = await _parents.DeleteAsync(UserId);
            if (r.Success) { await ShowToastAsync("Parent deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}
