namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordDriverListViewModel : BaseViewModel
    {
        private readonly IDriverService _drivers;

        [ObservableProperty] private ObservableCollection<DriverItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        public string SearchPlaceholder => "Search drivers…";
        public bool CanLoadMore => false;
        public bool CanEdit => Can("driver.edit");

        public CoordDriverListViewModel(IAuthService auth, INavigationService nav, IDriverService drivers)
            : base(auth, nav) { _drivers = drivers; Title = "Drivers"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _drivers.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null);
                Items = new ObservableCollection<DriverItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private async Task LoadMoreAsync() { }
        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand]
        private Task DetailAsync(DriverItem d) =>
            Nav.GoToAsync("CoordDriverDetail", new Dictionary<string, object> { ["UserId"] = d.UserId });
    }
}
