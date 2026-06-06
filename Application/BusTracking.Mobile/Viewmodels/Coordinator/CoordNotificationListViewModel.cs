namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordNotificationListViewModel : BaseViewModel
    {
        private readonly IApiService _api;

        [ObservableProperty] private ObservableCollection<NotificationItem> _items = [];
        [ObservableProperty] private bool _canLoadMore;

        public CoordNotificationListViewModel(IAuthService auth, INavigationService nav, IApiService api)
            : base(auth, nav) { _api = api; Title = "Notifications"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var r = await _api.GetAsync<List<NotificationItem>>(
                    Constants.Coordinator.CoordNotifications);
                Items = new ObservableCollection<NotificationItem>(r.Data ?? []);
                IsEmpty = !Items.Any();
                CanLoadMore = false;
            });
        }

        [RelayCommand]
        private async Task MarkAllReadAsync()
        {
            var r = await _api.PostAsync<object>(Constants.Coordinator.CoordNotifMarkAllRead);
            if (r.Success) await LoadAsync();
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DetailAsync(NotificationItem item)
        {
            // Mark as read then navigate to detail
            if (!item.IsRead)
                await _api.PostAsync<object>(
                    string.Format(Constants.Coordinator.CoordNotifMarkRead, item.NotificationId));

            await Nav.GoToAsync("CoordNotificationDetail",
                new Dictionary<string, object> { ["Item"] = item });
        }

        [RelayCommand] private async Task LoadMoreAsync() { }
    }
}
