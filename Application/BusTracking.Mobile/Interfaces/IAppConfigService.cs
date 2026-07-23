namespace BusTracking.Mobile.Interfaces
{
    public interface IAppConfigService
    {
        Task<Dictionary<string, string>> GetMobileConfigAsync(bool forceRefresh = false);
        Task<string?> GetValueAsync(string key);
        Task<bool> IsMaintenanceModeAsync();
        Task<bool> IsMandatoryUpdateAsync();
        /// <summary>
        /// Returns true when IsMobileUpdateImage = "1" in AppConfig.
        /// </summary>
        /// <returns></returns>
        Task<bool> IsMobileImageUpdateEnabledAsync();
        /// <summary>
        /// Returns true when IsAllowPushNotification = "1" in AppConfig.
        /// </summary>
        Task<bool> IsAllowPushNotificationAsync();
        /// <summary>
        /// Returns the WebsiteImageUrl config value (base URL for images when mobile upload is disabled).
        /// </summary>
        /// <returns></returns>
        Task<string?> GetWebsiteImageUrlAsync();
    }
}
