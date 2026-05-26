namespace BusTracking.Mobile.Interfaces
{
    public interface IAppConfigService
    {
        Task<Dictionary<string, string>> GetMobileConfigAsync(bool forceRefresh = false);
        Task<string?> GetValueAsync(string key);
        Task<bool> IsMaintenanceModeAsync();
        Task<bool> IsMandatoryUpdateAsync();
    }
}
