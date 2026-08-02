namespace BusTracking.Mobile.Interfaces
{
    /// <summary>
    /// Pre-login global configuration service.
    /// Reads from GlobalConfigurations table via /api/global-config/mobile (anonymous, system-wide).
    /// Used for maintenance mode, forced app update, and version checks BEFORE user login.
    /// </summary>
    public interface IGlobalConfigService
    {
        Task<Dictionary<string, string>> GetGlobalConfigAsync(bool forceRefresh = false);
        Task<string?> GetValueAsync(string key);
        Task<bool> IsMaintenanceModeAsync();
        Task<bool> IsMandatoryUpdateAsync();
    }
}
