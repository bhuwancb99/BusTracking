namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordParentListViewModel : BaseViewModel
    {
        private readonly IParentService _parents;

        [ObservableProperty] private ObservableCollection<ParentItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _canLoadMore;
        [ObservableProperty] private int _currentPage = 1;

        public bool CanView => Can("parent.view");
        public bool CanAdd => Can("parent.add");
        public bool CanEdit => Can("parent.edit");
        public bool CanDelete => Can("parent.delete");

        public CoordParentListViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; Title = "Parents"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _parents.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null, 1);
                Items = new ObservableCollection<ParentItem>(data);
                IsEmpty = !Items.Any();
                CanLoadMore = data.Count == 20;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();

        [RelayCommand]
        private Task ViewAsync(ParentItem p)
        {
            if (!CanView) return Task.CompletedTask;
            return Nav.GoToAsync("CoordParentDetail", new Dictionary<string, object> { ["UserId"] = p.UserId });
        }

        [RelayCommand]
        private async Task DeleteAsync(ParentItem p)
        {
            if (!CanDelete) return;
            if (!await ConfirmAsync("Delete Parent", $"Delete '{p.FullName}'?")) return;
            var r = await _parents.DeleteAsync(p.UserId);
            if (r.Success) { Items.Remove(p); await ShowToastAsync("Parent deleted."); }
            else SetError(r.Message);
        }
    }
}