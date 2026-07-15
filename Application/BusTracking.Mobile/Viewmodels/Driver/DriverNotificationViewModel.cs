namespace BusTracking.Mobile.Viewmodels.Driver
{
    public partial class DriverNotificationViewModel : BaseViewModel
    {
        private readonly IDriverService _driverService;

        [ObservableProperty] private ObservableCollection<DriverNotificationItem> _items = [];
        [ObservableProperty] private bool _hasUnread;

        public DriverNotificationViewModel(
            IAuthService auth,
            INavigationService nav,
            IDriverService driverService) : base(auth, nav)
        {
            Title = "Notifications";
            this._driverService = driverService;
        }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var list = await _driverService.GetAllNotificationAsync();
                Items = new ObservableCollection<DriverNotificationItem>(list);
                IsEmpty = !Items.Any();
                HasUnread = Items.Any(n => !n.IsRead);
            });
        }

        [RelayCommand]
        private async Task MarkAllReadAsync()
        {
            await RunAsync(async () =>
            {
                var r = await _driverService.MarkAllReadAsync();
                if (r.Success)
                    await LoadAsync();
                else
                    SetError(r.Message);
            });
        }

        [RelayCommand]
        private async Task MarkReadAsync(DriverNotificationItem item)
        {
            if (item.IsRead) return;
            await RunAsync(async () =>
            {
                var r = await _driverService.MarkReadAsync(item.NotificationId);
                if (r.Success)
                {
                    item.IsRead = true;
                    HasUnread = Items.Any(n => !n.IsRead);
                }
                else
                    SetError(r.Message);
            });
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