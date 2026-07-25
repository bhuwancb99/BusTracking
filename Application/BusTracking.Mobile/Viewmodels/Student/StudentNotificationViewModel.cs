namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentNotificationViewModel : BaseViewModel
    {
        private readonly IApiService _api;

        [ObservableProperty] private ObservableCollection<NotificationItem> _items = [];
        [ObservableProperty] private DateTime _fromDate = DateTime.Today;
        [ObservableProperty] private DateTime _toDate = DateTime.Today;
        [ObservableProperty] private int _pageNumber = 1;
        [ObservableProperty] private int _totalPages = 1;
        [ObservableProperty] private int _totalCount;
        [ObservableProperty] private bool _hasUnread;
        [ObservableProperty] private bool _hasMoreData = true;
        [ObservableProperty] private bool _isLoadingMore;

        public StudentNotificationViewModel(IAuthService auth, INavigationService nav, IApiService api)
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
                var url = string.Format(Constants.NotificationsPaged, PageNumber, FromDate, ToDate);
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
            if (IsLoadingMore || IsBusy || !HasMoreData || PageNumber >= TotalPages)
                return;

            IsLoadingMore = true;
            try
            {
                var nextPage = PageNumber + 1;
                var url = string.Format(Constants.NotificationsPaged, nextPage, FromDate, ToDate);
                var r = await _api.GetAsync<PagedResult<NotificationItem>>(url);
                if (r.Success && r.Data != null && r.Data.Items != null && r.Data.Items.Any())
                {
                    PageNumber = nextPage;
                    TotalPages = Math.Max(1, r.Data.TotalPages);
                    TotalCount = r.Data.TotalCount;
                    foreach (var item in r.Data.Items)
                    {
                        Items.Add(item);
                    }
                    HasMoreData = PageNumber < TotalPages;
                    HasUnread = Items.Any(n => !n.IsRead);
                }
                else
                {
                    HasMoreData = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StudentNotificationViewModel] LoadMore error: {ex.Message}");
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            PageNumber = 1;
            await LoadAsync();
        }

        [RelayCommand]
        private async Task ResetTodayAsync()
        {
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
            PageNumber = 1;
            await LoadAsync();
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

        [RelayCommand]
        private async Task MarkAllReadAsync()
        {
            await RunAsync(async () =>
            {
                var r = await _api.PutAsync<object>(Constants.NotificationsReadAll, new { });
                if (r.Success) await LoadAsync();
                else SetError(r.Message);
            });
        }

        [RelayCommand]
        private async Task MarkReadAsync(NotificationItem item)
        {
            if (item.IsRead) return;
            await RunAsync(async () =>
            {
                var r = await _api.PutAsync<object>(string.Format(Constants.NotificationRead, item.NotificationId), new { });
                if (r.Success)
                {
                    item.IsRead = true;
                    HasUnread = Items.Any(n => !n.IsRead);
                }
            });
        }
    }
}
