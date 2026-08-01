namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminFeedbackListViewModel : BaseViewModel
    {
        private readonly IApiService _api;

        [ObservableProperty] private ObservableCollection<FeedbackItem> _items = [];
        [ObservableProperty] private string _selectedStatus = "All";
        [ObservableProperty] private bool _canLoadMore;

        public bool CanManage => true; // Super Admin always can manage status
        public List<string> StatusOptions => ["All", "Open", "InProgress", "Resolved", "Closed"];

        public AdminFeedbackListViewModel(IAuthService auth, INavigationService nav, IApiService api)
            : base(auth, nav) { _api = api; Title = "Help & Support"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var url = Constants.Admin.Feedback;
                if (!string.IsNullOrWhiteSpace(SelectedStatus) && !SelectedStatus.Equals("All", StringComparison.OrdinalIgnoreCase))
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
            Nav.GoToAsync("AdminFeedbackDetail",
                new Dictionary<string, object> { ["Feedback"] = item, ["FeedbackId"] = item.FeedbackId });

        [RelayCommand] private async Task LoadMoreAsync() { }

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
