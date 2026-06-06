namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordNotificationDetailViewModel : BaseViewModel, IQueryAttributable
    {
        [ObservableProperty] private NotificationItem? _item;

        public CoordNotificationDetailViewModel(IAuthService auth, INavigationService nav)
            : base(auth, nav) { Title = "Notification"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Item", out var item))
                Item = item as NotificationItem;
        }

        [RelayCommand] private Task BackAsync() => Nav.GoBackAsync();
    }
}
