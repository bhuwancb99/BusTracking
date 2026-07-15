namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminParentListViewModel : BaseViewModel
    {
        private readonly IParentService _parents;

        [ObservableProperty] private ObservableCollection<ParentItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedFilter = "Active";   // Active | Inactive | Both
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search parents…";
        public List<string> FilterOptions => ["Active", "Inactive", "Both"];
        public bool CanAdd => Can("parent.add");
        public bool CanEdit => Can("parent.edit");
        public bool CanDelete => Can("parent.delete");

        public AdminParentListViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; Title = "Parents"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();   // ← reload after Add/Edit

        // Re-load when filter chip changes
        partial void OnSelectedFilterChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _parents.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                Items = new ObservableCollection<ParentItem>(data.Items);
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
                var data = await _parents.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminParentForm");
        [RelayCommand]
        private Task EditAsync(ParentItem p) =>
            Nav.GoToAsync("AdminParentForm", new Dictionary<string, object> { ["UserId"] = p.UserId });

        [RelayCommand]
        private async Task ToggleAsync(ParentItem p)
        {
            var r = await _parents.ToggleAsync(p.UserId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync(ParentItem p)
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {p.FullName}?")) return;
            var r = await _parents.ResetPasswordAsync(p.UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", $"New password: {r.Data?.PlainPassword}");
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(ParentItem p)
        {
            if (!await ConfirmAsync("Delete Parent", $"Delete '{p.FullName}'?")) return;
            var r = await _parents.DeleteAsync(p.UserId);
            if (r.Success) { Items.Remove(p); await ShowToastAsync("Parent deleted."); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private Task DetailAsync(ParentItem p) =>
            Nav.GoToAsync("AdminParentDetail", new Dictionary<string, object> { ["ParentId"] = p.UserId });

        [RelayCommand]
        private void Filter(string filter) => SelectedFilter = filter;
        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}