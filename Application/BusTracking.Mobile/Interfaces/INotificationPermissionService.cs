namespace BusTracking.Mobile.Interfaces
{
    public interface INotificationPermissionService
    {
        Task<bool> IsNotificationPermissionGrantedAsync();
        Task<bool> RequestNotificationPermissionAsync();
        void OpenAppSettings();
    }
}
