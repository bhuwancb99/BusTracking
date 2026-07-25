namespace BusTracking.Mobile.Services
{
    public class PushTokenService : IPushTokenService
    {
        private readonly IApiService _api;
        private static bool _eventsSubscribed = false;

        public PushTokenService(IApiService api)
        {
            _api = api;
            SubscribeFirebaseMessagingEvents();
        }

        private void SubscribeFirebaseMessagingEvents()
        {
            if (_eventsSubscribed) return;
            _eventsSubscribed = true;

            try
            {
                CrossFirebaseCloudMessaging.Current.NotificationReceived += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[FCM Push] NotificationReceived: Title='{e.Notification?.Title}', Body='{e.Notification?.Body}'");
                };

                CrossFirebaseCloudMessaging.Current.NotificationTapped += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[FCM Push] NotificationTapped: Title='{e.Notification?.Title}'");
                };

                CrossFirebaseCloudMessaging.Current.TokenChanged += async (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[FCM Push] TokenChanged: {e.Token}");
                    if (!string.IsNullOrWhiteSpace(e.Token) && DeviceInfo.DeviceType != DeviceType.Virtual)
                    {
                        try
                        {
                            await _api.PostAsync<object>(Constants.DeviceToken, new
                            {
                                token = e.Token,
                                platform = DeviceInfo.Platform == DevicePlatform.iOS ? "iOS" : "Android",
                                isVirtual = false
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[FCM Push] TokenChanged sync error: {ex.Message}");
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PushTokenService] SubscribeEvents error: {ex.Message}");
            }
        }

        public async Task RegisterDeviceTokenAsync()
        {
            try
            {
                // Requirement 4: Only physical mobile devices should register tokens.
                // Ignore virtual devices / emulators.
                if (DeviceInfo.DeviceType == DeviceType.Virtual)
                {
                    System.Diagnostics.Debug.WriteLine("[PushTokenService] Skipping device token registration on emulator / virtual device.");
                    return;
                }

                string? token = null;
                string platform = DeviceInfo.Platform == DevicePlatform.iOS ? "iOS" : "Android";

                try
                {
#if ANDROID
                    if (OperatingSystem.IsAndroidVersionAtLeast(33))
                    {
                        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                        if (status != PermissionStatus.Granted)
                        {
                            status = await Permissions.RequestAsync<Permissions.PostNotifications>();
                        }
                    }
#endif
                    await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
                    token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PushTokenService] Plugin.Firebase GetTokenAsync error: {ex.Message}");
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    await _api.PostAsync<object>(Constants.DeviceToken, new
                    {
                        token = token,
                        platform = platform,
                        isVirtual = false
                    });
                    System.Diagnostics.Debug.WriteLine($"[PushTokenService] Physical device token registered successfully: {token}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PushTokenService] RegisterDeviceTokenAsync error: {ex.Message}");
            }
        }

        public async Task RemoveDeviceTokenAsync()
        {
            try
            {
                if (DeviceInfo.DeviceType == DeviceType.Virtual) return;

                string? token = null;
                try
                {
                    token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                }
                catch { }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    await _api.PostAsync<object>("api/notifications/device-token/remove", new
                    {
                        token = token
                    });
                    System.Diagnostics.Debug.WriteLine($"[PushTokenService] Physical device token removed on logout: {token}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PushTokenService] RemoveDeviceTokenAsync error: {ex.Message}");
            }
        }
    }
}
