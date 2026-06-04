namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordSubAdminListViewModel : BaseViewModel
    {
        private readonly ICoordSubAdminService _service;

        [ObservableProperty] private ObservableCollection<CoordinatorItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        public string SearchPlaceholder => "Search bus coordinators…";
        public bool CanAdd => Can("subadmin.add");
        public bool CanEdit => Can("subadmin.edit");
        public bool CanDelete => Can("subadmin.delete");
        public bool CanLoadMore => false;

        [RelayCommand] private async Task LoadMoreAsync() { }
        [RelayCommand] private async Task SearchAsync() => await LoadAsync();

        public CoordSubAdminListViewModel(IAuthService auth, INavigationService nav, ICoordSubAdminService service)
            : base(auth, nav) { _service = service; Title = "Bus Coordinators"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _service.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText : null);
                Items = new ObservableCollection<CoordinatorItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordSubAdminForm");

        [RelayCommand]
        private Task DetailAsync(CoordinatorItem c) =>
            Nav.GoToAsync("CoordSubAdminDetail", new Dictionary<string, object> { ["CoordId"] = c.UserId });

        [RelayCommand]
        private Task EditAsync(CoordinatorItem c) =>
            Nav.GoToAsync("CoordSubAdminForm", new Dictionary<string, object> { ["CoordId"] = c.UserId });

        [RelayCommand]
        private async Task ToggleAsync(CoordinatorItem c)
        {
            var r = await _service.ToggleAsync(c.UserId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(CoordinatorItem c)
        {
            if (!await ConfirmAsync("Delete", $"Mark '{c.FullName}' as inactive?")) return;
            var r = await _service.DeleteAsync(c.UserId);
            if (r.Success) { Items.Remove(c); await ShowToastAsync("Marked inactive."); }
            else SetError(r.Message);
        }
    }
}
