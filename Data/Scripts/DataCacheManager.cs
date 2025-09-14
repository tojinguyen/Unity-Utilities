using System;
using System.Collections.Generic;
using System.Linq;

namespace TirexGame.Utils.Data
{
    /// <summary>
    /// Statistics for data cache performance monitoring
    /// </summary>
    public class DataCacheStats
    {
        public int TotalItems { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
        public double HitRatio => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0;
        public int TotalRequests => HitCount + MissCount;
        public long MemoryUsageBytes { get; set; }
        public int ExpiredItems { get; set; }
        public DateTime LastCleanup { get; set; }
    }

    /// <summary>
    /// Cached data entry with expiration support
    /// </summary>
    internal class CacheEntry
    {
        public object Data { get; set; }
        public DateTime CachedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int AccessCount { get; set; }
        public DateTime LastAccessed { get; set; }
        public long SizeBytes { get; set; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public CacheEntry(object data, TimeSpan expiration)
        {
            Data = data;
            CachedAt = DateTime.UtcNow;
            ExpiresAt = CachedAt.Add(expiration);
            LastAccessed = CachedAt;
            AccessCount = 0;
            SizeBytes = EstimateSize(data);
        }

        public void UpdateAccess()
        {
            AccessCount++;
            LastAccessed = DateTime.UtcNow;
        }

        private long EstimateSize(object obj)
        {
            if (obj == null) return 0;
            
            // Simple size estimation - in real scenarios you might want more sophisticated calculation
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return System.Text.Encoding.UTF8.GetByteCount(json);
        }
    }

    /// <summary>
    /// High-performance data cache manager with expiration and statistics
    /// </summary>
    public class DataCacheManager
    {
        private readonly Dictionary<string, CacheEntry> _cache = new();
        private readonly object _lock = new object();
        private readonly DataCacheStats _stats = new DataCacheStats();
        
        private readonly long _maxMemoryBytes;
        private readonly TimeSpan _defaultExpiration;
        private readonly TimeSpan _cleanupInterval;
        private DateTime _lastCleanup = DateTime.UtcNow;

        public DataCacheManager(
            long maxMemoryBytes = 50 * 1024 * 1024, // 50MB default
            TimeSpan? defaultExpiration = null,
            TimeSpan? cleanupInterval = null)
        {
            _maxMemoryBytes = maxMemoryBytes;
            _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(30);
            _cleanupInterval = cleanupInterval ?? TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Cache data with specified expiration
        /// </summary>
        public void Cache<T>(string key, T data, TimeSpan? expiration = null) where T : class
        {
            if (string.IsNullOrEmpty(key) || data == null) return;

            lock (_lock)
            {
                var exp = expiration ?? _defaultExpiration;
                var entry = new CacheEntry(data, exp);

                // Check memory constraints
                if (_stats.MemoryUsageBytes + entry.SizeBytes > _maxMemoryBytes)
                {
                    EvictLeastRecentlyUsed();
                }

                _cache[key] = entry;
                UpdateMemoryUsage();
                PerformCleanupIfNeeded();
            }
        }

        /// <summary>
        /// Try to get cached data
        /// </summary>
        public bool TryGetCached<T>(string key, out T data) where T : class
        {
            data = null;
            if (string.IsNullOrEmpty(key))
            {
                _stats.MissCount++;
                return false;
            }

            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (entry.IsExpired)
                    {
                        _cache.Remove(key);
                        _stats.MissCount++;
                        _stats.ExpiredItems++;
                        UpdateMemoryUsage();
                        return false;
                    }

                    entry.UpdateAccess();
                    data = entry.Data as T;
                    _stats.HitCount++;
                    return data != null;
                }

                _stats.MissCount++;
                return false;
            }
        }

        /// <summary>
        /// Check if key exists in cache (without updating access stats)
        /// </summary>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (entry.IsExpired)
                    {
                        _cache.Remove(key);
                        _stats.ExpiredItems++;
                        UpdateMemoryUsage();
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Remove specific item from cache
        /// </summary>
        public bool RemoveFromCache(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            lock (_lock)
            {
                var removed = _cache.Remove(key);
                if (removed)
                {
                    UpdateMemoryUsage();
                }
                return removed;
            }
        }

        /// <summary>
        /// Clear all cached data
        /// </summary>
        public void ClearAll()
        {
            lock (_lock)
            {
                _cache.Clear();
                _stats.MemoryUsageBytes = 0;
                _stats.TotalItems = 0;
            }
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        public DataCacheStats GetStats()
        {
            lock (_lock)
            {
                _stats.TotalItems = _cache.Count;
                _stats.LastCleanup = _lastCleanup;
                return new DataCacheStats
                {
                    TotalItems = _stats.TotalItems,
                    HitCount = _stats.HitCount,
                    MissCount = _stats.MissCount,
                    MemoryUsageBytes = _stats.MemoryUsageBytes,
                    ExpiredItems = _stats.ExpiredItems,
                    LastCleanup = _stats.LastCleanup
                };
            }
        }

        /// <summary>
        /// Manually trigger cache cleanup
        /// </summary>
        public void Cleanup()
        {
            lock (_lock)
            {
                var expiredKeys = _cache
                    .Where(kvp => kvp.Value.IsExpired)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _cache.Remove(key);
                    _stats.ExpiredItems++;
                }

                _lastCleanup = DateTime.UtcNow;
                UpdateMemoryUsage();
            }
        }

        /// <summary>
        /// Get all cached keys
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            lock (_lock)
            {
                return _cache.Keys.ToList();
            }
        }

        /// <summary>
        /// Get cache entry information
        /// </summary>
        public bool TryGetCacheInfo(string key, out DateTime cachedAt, out DateTime expiresAt, out int accessCount)
        {
            cachedAt = default;
            expiresAt = default;
            accessCount = 0;

            if (string.IsNullOrEmpty(key)) return false;

            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
                {
                    cachedAt = entry.CachedAt;
                    expiresAt = entry.ExpiresAt;
                    accessCount = entry.AccessCount;
                    return true;
                }
                return false;
            }
        }

        private void UpdateMemoryUsage()
        {
            _stats.MemoryUsageBytes = _cache.Values.Sum(e => e.SizeBytes);
        }

        private void PerformCleanupIfNeeded()
        {
            if (DateTime.UtcNow - _lastCleanup > _cleanupInterval)
            {
                Cleanup();
            }
        }

        private void EvictLeastRecentlyUsed()
        {
            if (_cache.Count == 0) return;

            // Find least recently used entry
            var lruEntry = _cache
                .OrderBy(kvp => kvp.Value.LastAccessed)
                .First();

            _cache.Remove(lruEntry.Key);
            UpdateMemoryUsage();

#if DATA_LOG
            UnityEngine.Debug.Log($"[DataCacheManager] Evicted LRU entry: {lruEntry.Key}");
#endif
        }
    }
}
