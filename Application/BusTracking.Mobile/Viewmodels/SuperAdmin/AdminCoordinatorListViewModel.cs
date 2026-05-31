namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminCoordinatorListViewModel : BaseViewModel
    {
        private readonly ICoordinatorService _coords;

        [ObservableProperty] private ObservableCollection<CoordinatorItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedFilter = "Active";

        // CoordinatorService returns all results in one call — no pagination
        public bool CanLoadMore => false;

        public string SearchPlaceholder => "Search coordinators…";
        public bool CanAdd => Can("coordinator.add");
        public bool CanEdit => Can("coordinator.edit");
        public bool CanDelete => Can("coordinator.delete");

        public List<string> FilterOptions => ["Active", "Inactive", "Both"];

        public AdminCoordinatorListViewModel(IAuthService auth, INavigationService nav, ICoordinatorService coords)
            : base(auth, nav) { _coords = coords; Title = "Bus Coordinators"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        partial void OnSelectedFilterChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                bool? isActive = SelectedFilter switch
                {
                    "Active" => true,
                    "Inactive" => false,
                    _ => null
                };
                var data = await _coords.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText : null, isActive);
                Items = new ObservableCollection<CoordinatorItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        // No-op: service does not support pagination
        [RelayCommand] private Task LoadMoreAsync() => Task.CompletedTask;

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminCoordinatorForm");

        [RelayCommand]
        private Task TapAsync(CoordinatorItem c) =>
            Nav.GoToAsync("AdminCoordinatorDetail", new Dictionary<string, object> { ["UserId"] = c.UserId });

        [RelayCommand]
        private Task EditAsync(CoordinatorItem c) =>
            Nav.GoToAsync("AdminCoordinatorForm", new Dictionary<string, object> { ["UserId"] = c.UserId });

        [RelayCommand]
        private async Task ToggleAsync(CoordinatorItem c)
        {
            var r = await _coords.ToggleAsync(c.UserId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync(CoordinatorItem c)
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {c.FullName}?")) return;
            var r = await _coords.ResetPasswordAsync(c.UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", r.Message);
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(CoordinatorItem c)
        {
            if (!c.IsActive) return;
            if (!await ConfirmAsync("Delete", $"Delete coordinator '{c.FullName}'?")) return;
            var r = await _coords.DeleteAsync(c.UserId);
            if (r.Success) { Items.Remove(c); await ShowToastAsync("Coordinator deleted."); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private void Filter(string filter) => SelectedFilter = filter;
    }
}
