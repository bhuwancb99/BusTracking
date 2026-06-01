namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordParentListViewModel : BaseViewModel
    {
        private readonly IParentService _parents;

        [ObservableProperty] private ObservableCollection<ParentItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        public string SearchPlaceholder => "Search parents…";
        public bool CanLoadMore => false;
        public bool CanAdd    => Can("parent.add");
        public bool CanEdit   => Can("parent.edit");
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

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _parents.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null);
                Items = new ObservableCollection<ParentItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private async Task LoadMoreAsync() { }
        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync()    => Nav.GoToAsync("CoordParentForm");
        [RelayCommand]
        private Task DetailAsync(ParentItem p) =>
            Nav.GoToAsync("CoordParentDetail", new Dictionary<string, object> { ["UserId"] = p.UserId });
    }
}
