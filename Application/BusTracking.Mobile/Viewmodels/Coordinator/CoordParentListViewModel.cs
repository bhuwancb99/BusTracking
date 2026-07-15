namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordParentListViewModel : BaseViewModel
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

        public CoordParentListViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; Title = "Parents"; }

        public override async Task InitializeAsync()
        {
            OnPropertyChanged(nameof(CanAdd));
            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(CanDelete));
            await LoadAsync();
        }

        public override async Task RefreshOnReturnAsync() => await LoadAsync();

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
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordParentForm");
        [RelayCommand]
        private Task DetailAsync(ParentItem p) =>
            Nav.GoToAsync("CoordParentDetail", new Dictionary<string, object> { ["UserId"] = p.UserId });

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
