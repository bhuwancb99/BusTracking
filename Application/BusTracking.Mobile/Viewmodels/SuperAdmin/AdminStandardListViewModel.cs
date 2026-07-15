namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminStandardListViewModel : BaseViewModel
    {
        private readonly IAdminStandardService _standardService;

        [ObservableProperty] private ObservableCollection<StandardItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search standards…";
        public bool CanAdd => true;

        public AdminStandardListViewModel(IAuthService auth, INavigationService nav, IAdminStandardService standardService)
            : base(auth, nav)
        {
            _standardService = standardService;
            Title = "Standard Master";
        }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _standardService.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage);
                Items = new ObservableCollection<StandardItem>(data.Items);
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
                var data = await _standardService.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminStandardForm");

        [RelayCommand]
        private Task DetailAsync(StandardItem s) =>
            Nav.GoToAsync("AdminStandardForm", new Dictionary<string, object> { ["StandardId"] = s.StandardId });

        [RelayCommand]
        private Task EditAsync(StandardItem s) =>
            Nav.GoToAsync("AdminStandardForm", new Dictionary<string, object> { ["StandardId"] = s.StandardId });

        [RelayCommand]
        private async Task ToggleAsync(StandardItem s)
        {
            var r = await _standardService.ToggleAsync(s.StandardId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(StandardItem s)
        {
            if (!await ConfirmAsync("Delete Standard", $"Delete class '{s.StandardName}'?")) return;
            var r = await _standardService.DeleteAsync(s.StandardId);
            if (r.Success) { Items.Remove(s); await ShowToastAsync("Standard deleted."); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try { await LoadAsync(); }
            finally { IsRefreshing = false; }
        }
    }
}
