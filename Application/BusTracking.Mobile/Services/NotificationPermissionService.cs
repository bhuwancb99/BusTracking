namespace BusTracking.Mobile.Services
{
    public class NotificationPermissionService : INotificationPermissionService
    {
        public async Task<bool> IsNotificationPermissionGrantedAsync()
        {
            try
            {
#if ANDROID
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                    return status == PermissionStatus.Granted;
                }
                return true;
#elif IOS
                var settings = await UserNotifications.UNUserNotificationCenter.Current.GetNotificationSettingsAsync();
                return settings.AuthorizationStatus == UserNotifications.UNAuthorizationStatus.Authorized
                    || settings.AuthorizationStatus == UserNotifications.UNAuthorizationStatus.Provisional;
#else
                return true;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationPermissionService] CheckStatus Exception: {ex.Message}");
                return true;
            }
        }

        public async Task<bool> RequestNotificationPermissionAsync()
        {
            try
            {
#if ANDROID
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.PostNotifications>();
                    }
                    return status == PermissionStatus.Granted;
                }
                return true;
#elif IOS
                var (granted, error) = await UserNotifications.UNUserNotificationCenter.Current.RequestAuthorizationAsync(
                    UserNotifications.UNAuthorizationOptions.Alert | 
                    UserNotifications.UNAuthorizationOptions.Sound | 
                    UserNotifications.UNAuthorizationOptions.Badge);
                return granted;
#else
                return true;
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationPermissionService] RequestPermission Exception: {ex.Message}");
                return false;
            }
        }

        public void OpenAppSettings()
        {
            try
            {
                AppInfo.Current.ShowSettingsUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationPermissionService] OpenAppSettings Exception: {ex.Message}");
            }
        }
    }
}
