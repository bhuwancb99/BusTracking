namespace BusTracking.Mobile.Viewmodels.Teacher
{
    public partial class TeacherNotificationViewModel : BaseViewModel
    {
        private readonly IApiService _api;

        [ObservableProperty] private ObservableCollection<NotificationItem> _items = [];
        [ObservableProperty] private DateTime _fromDate = DateTime.Today.AddDays(-30);
        [ObservableProperty] private DateTime _toDate = DateTime.Today;

        [ObservableProperty] private bool _isFromDateCalendarOpen;
        [RelayCommand] private void OpenFromDateCalendar() => IsFromDateCalendarOpen = true;

        [ObservableProperty] private bool _isToDateCalendarOpen;
        [RelayCommand] private void OpenToDateCalendar() => IsToDateCalendarOpen = true;

        [ObservableProperty] private int _pageNumber = 1;
        [ObservableProperty] private int _totalPages = 1;
        [ObservableProperty] private int _totalCount;
        [ObservableProperty] private bool _hasUnread;
        [ObservableProperty] private bool _hasMoreData = true;

        public TeacherNotificationViewModel(IAuthService auth, INavigationService nav, IApiService api)
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
            PageNumber = 1;
            await RunAsync(async () =>
            {
                var url = $"{Constants.Teacher.Notifications}?page={PageNumber}&pageSize=20";
                var r = await _api.GetAsync<PagedResult<NotificationItem>>(url);
                if (r.Success && r.Data != null)
                {
                    Items = new ObservableCollection<NotificationItem>(r.Data.Items ?? []);
                    TotalPages = Math.Max(1, r.Data.TotalPages);
                    TotalCount = r.Data.TotalCount;
                    HasMoreData = PageNumber < TotalPages;
                    IsEmpty = !Items.Any();
                    HasUnread = Items.Any(n => !n.IsRead);
                }
                else
                {
                    Items = [];
                    IsEmpty = true;
                    HasMoreData = false;
                    HasUnread = false;
                }
            });
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!HasMoreData || IsBusy) return;
            PageNumber++;
            await RunAsync(async () =>
            {
                var url = $"{Constants.Teacher.Notifications}?page={PageNumber}&pageSize=20";
                var r = await _api.GetAsync<PagedResult<NotificationItem>>(url);
                if (r.Success && r.Data != null)
                {
                    foreach (var item in r.Data.Items)
                    {
                        Items.Add(item);
                    }
                    HasMoreData = PageNumber < r.Data.TotalPages;
                }
            });
        }

        [RelayCommand]
        private async Task RefreshAsync() => await LoadAsync();

        [RelayCommand]
        private async Task FilterAsync() => await LoadAsync();

        [RelayCommand]
        private async Task MarkReadAsync(NotificationItem item)
        {
            if (item == null || item.IsRead) return;
            item.IsRead = true;
            HasUnread = Items.Any(n => !n.IsRead);
            await _api.PostAsync<object>($"{Constants.Teacher.Notifications}/{item.NotificationId}/read", new { });
        }

        [RelayCommand]
        private async Task MarkAllReadAsync()
        {
            if (!HasUnread) return;
            foreach (var item in Items) item.IsRead = true;
            HasUnread = false;
            await _api.PostAsync<object>($"{Constants.Teacher.Notifications}/read-all", new { });
        }
    }
}
