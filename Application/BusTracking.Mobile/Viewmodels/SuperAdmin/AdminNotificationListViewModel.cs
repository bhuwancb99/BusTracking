namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminNotificationListViewModel : BaseViewModel
    {
        private readonly IApiService _api;

        [ObservableProperty] private ObservableCollection<NotificationItem> _items = [];

        public AdminNotificationListViewModel(IAuthService auth, INavigationService nav, IApiService api)
            : base(auth, nav)
        {
            _api = api;
            Title = "Notifications";
        }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var r = await _api.GetAsync<List<NotificationItem>>(
                    Constants.Admin.Notifications);
                Items = new ObservableCollection<NotificationItem>(r.Data ?? []);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand]
        private async Task MarkAllReadAsync()
        {
            await RunAsync(async () =>
            {
                var r = await _api.PostAsync<object>(Constants.Admin.NotifReadAll);
                if (r.Success) await LoadAsync();
                else SetError(r.Message);
            });
        }

        [RelayCommand]
        private async Task DetailAsync(NotificationItem item)
        {
            if (!item.IsRead)
                await _api.PostAsync<object>(
                    string.Format(Constants.Admin.NotifRead, item.NotificationId));

            await Nav.GoToAsync("AdminNotificationDetail",
                new Dictionary<string, object> { ["Item"] = item });
        }

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
