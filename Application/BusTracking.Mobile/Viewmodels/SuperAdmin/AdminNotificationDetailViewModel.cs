namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminNotificationDetailViewModel : BaseViewModel, IQueryAttributable
    {
        [ObservableProperty] private NotificationItem? _item;

        public AdminNotificationDetailViewModel(IAuthService auth, INavigationService nav)
            : base(auth, nav) { Title = "Notification"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Item", out var item))
                Item = item as NotificationItem;
        }

        [RelayCommand] private Task BackAsync() => Nav.GoBackAsync();
    }
}
