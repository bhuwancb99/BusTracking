namespace BusTracking.Mobile.Interfaces
{
    /// <summary>
    /// Simple in-memory cache with TTL (time-to-live).
    /// Used for frequent API data: app config, dashboard, lists.
    /// Cache is cleared on logout.
    /// </summary>
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan ttl);
        void Remove(string key);
        void Clear();
        bool Has(string key);
    }
}
