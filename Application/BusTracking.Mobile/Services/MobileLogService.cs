namespace BusTracking.Mobile.Services
{
    public class MobileLogService : IMobileLogService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public MobileLogService(IApiService api, IAuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        public async Task LogExceptionAsync(Exception ex, string? moduleName = null, string? actionName = null, string? additionalDetails = null)
        {
            await LogToApiAsync(ex.Message, ex.StackTrace, null, moduleName, actionName, additionalDetails);
        }

        public async Task LogEventAsync(string message, string? moduleName = null, string? actionName = null, string? additionalDetails = null)
        {
            await LogToApiAsync(message, null, null, moduleName, actionName, additionalDetails);
        }

        private async Task LogToApiAsync(string message, string? stackTrace, string? requestUrl, string? moduleName, string? actionName, string? additionalDetails)
        {
            try
            {
                string platform = "Unknown";
#if ANDROID
                platform = "Android";
#elif IOS
                platform = "iOS";
#elif WINDOWS
                platform = "Windows";
#elif MACCATALYST
                platform = "macOS";
#endif

                int? userId = null;
                string? username = null;
                string? role = null;

                try
                {
                    var user = await _auth.GetCurrentUserAsync();
                    if (user != null)
                    {
                        userId = user.UserId;
                        username = user.UserName ?? user.Email;
                        role = user.Role;
                    }
                }
                catch { }

                var details = $"Model: {DeviceInfo.Model}, Version: {DeviceInfo.VersionString}";
                if (!string.IsNullOrEmpty(additionalDetails))
                    details += $"; {additionalDetails}";

                var logDto = new
                {
                    Platform = platform,
                    ExceptionMessage = message,
                    StackTrace = stackTrace,
                    RequestUrl = requestUrl,
                    UserId = userId,
                    Username = username,
                    Role = role,
                    ModuleName = moduleName,
                    ActionName = actionName,
                    AdditionalDetails = details
                };

                // Run in background task to avoid blocking user flow or UI
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _api.PostAsync<object>("api/logger", logDto);
                    }
                    catch { }
                });
            }
            catch { }
        }
    }
}
