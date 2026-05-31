namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordParentDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IParentService _parents;
        [ObservableProperty] private int _userId;
        [ObservableProperty] private ParentItem? _parent;

        public bool CanEdit => Can("parent.edit");
        public bool CanDelete => Can("parent.delete");

        public CoordParentDetailViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; Title = "Parent Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UserId", out var id)) UserId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () => { Parent = await _parents.GetByIdAsync(UserId); });
        }

        [RelayCommand]
        private Task EditAsync()
        {
            if (!CanEdit) return Task.CompletedTask;
            // Coordinator has no parent-edit page; uses Admin endpoint via ParentService
            return ShowAlertAsync("Edit Parent", "Parent editing is managed by the Super Admin.");
        }

        [RelayCommand]
        private async Task ToggleAsync()
        {
            if (Parent is null) return;
            var r = await _parents.ToggleAsync(UserId);
            if (r.Success) { await ShowToastAsync(r.Message); await InitializeAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!CanDelete) return;
            if (!await ConfirmAsync("Delete Parent", $"Delete '{Parent?.FullName}'?")) return;
            var r = await _parents.DeleteAsync(UserId);
            if (r.Success) { await ShowToastAsync("Parent deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}