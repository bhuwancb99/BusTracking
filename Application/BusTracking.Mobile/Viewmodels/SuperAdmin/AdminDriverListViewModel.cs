namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminDriverListViewModel : BaseViewModel
    {
        private readonly IDriverService _drivers;

        [ObservableProperty] private ObservableCollection<DriverItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _canLoadMore;
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private string _selectedFilter = "Active";

        public string SearchPlaceholder => "Search drivers…";
        public List<string> FilterOptions => ["Active", "Inactive", "Both"];

        public bool CanAdd => Can("driver.add");
        public bool CanEdit => Can("driver.edit");
        public bool CanDelete => Can("driver.delete");

        public AdminDriverListViewModel(IAuthService auth, INavigationService nav, IDriverService drivers)
            : base(auth, nav) { _drivers = drivers; Title = "Drivers"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        partial void OnSelectedFilterChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _drivers.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                Items = new ObservableCollection<DriverItem>(data.Items);
                IsEmpty = !Items.Any();
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!CanLoadMore || IsBusy) return;
            await RunAsync(async () =>
            {
                CurrentPage++;
                var data = await _drivers.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminDriverForm");

        // Tap row → Detail page
        [RelayCommand]
        private Task DetailAsync(DriverItem d) =>
            Nav.GoToAsync("AdminDriverDetail", new Dictionary<string, object> { ["UserId"] = d.UserId });

        [RelayCommand]
        private Task EditAsync(DriverItem d) =>
            Nav.GoToAsync("AdminDriverForm", new Dictionary<string, object> { ["UserId"] = d.UserId });

        [RelayCommand]
        private async Task ToggleAsync(DriverItem d)
        {
            var r = await _drivers.ToggleAsync(d.UserId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync(DriverItem d)
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {d.FullName}?")) return;
            var r = await _drivers.ResetPasswordAsync(d.UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", $"New password: {r.Data?.PlainPassword}");
            else SetError(r.Message);
        }

        // Only active records can be deleted
        [RelayCommand]
        private async Task DeleteAsync(DriverItem d)
        {
            if (!d.IsActive) return;
            if (!await ConfirmAsync("Delete Driver", $"Delete '{d.FullName}'?")) return;
            var r = await _drivers.DeleteAsync(d.UserId);
            if (r.Success) { Items.Remove(d); await ShowToastAsync("Driver deleted."); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private void Filter(string filter) => SelectedFilter = filter;
    }
}