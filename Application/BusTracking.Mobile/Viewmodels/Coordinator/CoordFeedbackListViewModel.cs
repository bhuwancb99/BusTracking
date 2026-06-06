namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordFeedbackListViewModel : BaseViewModel
    {
        private readonly IApiService _api;

        [ObservableProperty] private ObservableCollection<FeedbackItem> _items = [];
        [ObservableProperty] private string _selectedStatus = "";
        [ObservableProperty] private bool _canLoadMore;

        public bool CanManage => Can("helpsupport.manage");
        public List<string> StatusOptions => ["", "Open", "InProgress", "Resolved", "Closed"];

        public CoordFeedbackListViewModel(IAuthService auth, INavigationService nav, IApiService api)
            : base(auth, nav) { _api = api; Title = "Help & Support"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var url = Constants.Coordinator.Feedback;
                if (!string.IsNullOrEmpty(SelectedStatus))
                    url += $"?status={SelectedStatus}";

                var r = await _api.GetAsync<PagedResult<FeedbackItem>>(url);
                Items = new ObservableCollection<FeedbackItem>(r.Data?.Items ?? []);
                IsEmpty = !Items.Any();
                CanLoadMore = false;
            });
        }

        partial void OnSelectedStatusChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private Task DetailAsync(FeedbackItem item) =>
            Nav.GoToAsync("CoordFeedbackDetail",
                new Dictionary<string, object> { ["FeedbackId"] = item.FeedbackId });

        [RelayCommand] private async Task LoadMoreAsync() { }
    }
}
