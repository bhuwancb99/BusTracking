namespace BusTracking.Mobile.Services
{
    public class PushTokenService : IPushTokenService
    {
        private readonly IApiService _api;

        public PushTokenService(IApiService api)
        {
            _api = api;
        }

        public async Task RegisterDeviceTokenAsync()
        {
            try
            {
                string? token = null;
                string platform = DeviceInfo.Platform == DevicePlatform.iOS ? "iOS" : "Android";

                try
                {
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
                        platform = platform
                    });
                    System.Diagnostics.Debug.WriteLine($"[PushTokenService] Device token registered successfully: {token}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PushTokenService] RegisterDeviceTokenAsync error: {ex.Message}");
            }
        }
    }
}
