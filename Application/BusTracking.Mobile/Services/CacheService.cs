namespace BusTracking.Mobile.Services
{
    public class CacheService : ICacheService
    {
        private readonly Dictionary<string, CacheEntry> _store = new();
        private readonly object _lock = new();

        public T? Get<T>(string key)
        {
            lock (_lock)
            {
                if (!_store.TryGetValue(key, out var entry)) return default;
                if (DateTime.UtcNow > entry.ExpiresAt)
                {
                    _store.Remove(key);
                    return default;
                }
                return (T)entry.Value;
            }
        }

        public void Set<T>(string key, T value, TimeSpan ttl)
        {
            lock (_lock)
            {
                _store[key] = new CacheEntry(value!, DateTime.UtcNow.Add(ttl));
            }
        }

        public void Remove(string key)
        {
            lock (_lock) { _store.Remove(key); }
        }

        public void Clear()
        {
            lock (_lock) { _store.Clear(); }
        }

        public bool Has(string key)
        {
            lock (_lock)
            {
                if (!_store.TryGetValue(key, out var entry)) return false;
                if (DateTime.UtcNow > entry.ExpiresAt) { _store.Remove(key); return false; }
                return true;
            }
        }

        private record CacheEntry(object Value, DateTime ExpiresAt);
    }
}
