using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// High-performance object pool for event instances
    /// Supports multi-threading and type-safe pooling
    /// Can be used as a service instance rather than singleton
    /// </summary>
    public class EventPool : MonoBehaviour
    {
        
        #region Configuration
        
        [Header("Pool Configuration")]
        [SerializeField] private int initialPoolSize = 100;
        [SerializeField] private int maxPoolSize = 1000;
        [SerializeField] private bool enableLogging = false;
        [SerializeField] private bool enableWarnings = true;
        
        #endregion
        
        #region Fields
        
        // Thread-safe pools for different event types
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<BaseEvent>> _eventPools 
            = new ConcurrentDictionary<Type, ConcurrentQueue<BaseEvent>>();
        
        // Pool statistics
        private readonly Dictionary<Type, PoolStats> _poolStats = new Dictionary<Type, PoolStats>();
        
        // Lock for statistics updates
        private readonly object _statsLock = new object();
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            Log("EventPool initialized");
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get an event instance from the pool
        /// </summary>
        /// <typeparam name="T">Type of event to get</typeparam>
        /// <returns>Event instance from pool or newly created</returns>
        public T Get<T>() where T : BaseEvent, new()
        {
            var type = typeof(T);
            var pool = GetOrCreatePool(type);
            
            if (pool.TryDequeue(out var pooledEvent))
            {
                var typedEvent = pooledEvent as T;
                if (typedEvent != null)
                {
                    typedEvent.Initialize();
                    UpdateStats(type, false, true);
                    Log($"Retrieved {type.Name} from pool");
                    return typedEvent;
                }
            }
            
            // Create new instance if pool is empty
            var newEvent = new T();
            newEvent.Initialize();
            UpdateStats(type, true, false);
            Log($"Created new {type.Name} instance");
            return newEvent;
        }
        
        /// <summary>
        /// Return an event instance to the pool
        /// </summary>
        /// <param name="eventInstance">Event to return to pool</param>
        public void Return(BaseEvent eventInstance)
        {
            if (eventInstance == null || !eventInstance.IsPoolable)
                return;
            
            var type = eventInstance.GetType();
            var pool = GetOrCreatePool(type);
            
            // Check pool size limit
            if (GetPoolSize(type) >= maxPoolSize)
            {
                if (enableWarnings)
                    Debug.LogWarning($"Pool for {type.Name} has reached max size ({maxPoolSize}). Discarding event.");
                return;
            }
            
            // Reset event and return to pool
            eventInstance.Reset();
            pool.Enqueue(eventInstance);
            UpdateStats(type, false, false);
            Log($"Returned {type.Name} to pool");
        }
        
        /// <summary>
        /// Pre-warm a pool with initial instances
        /// </summary>
        /// <typeparam name="T">Type of event to pre-warm</typeparam>
        /// <param name="count">Number of instances to create</param>
        public void PreWarm<T>(int count = -1) where T : BaseEvent, new()
        {
            if (count == -1) count = initialPoolSize;
            
            var type = typeof(T);
            var pool = GetOrCreatePool(type);
            
            for (int i = 0; i < count; i++)
            {
                var instance = new T();
                instance.Reset();
                pool.Enqueue(instance);
            }
            
            Log($"Pre-warmed {count} instances of {type.Name}");
        }
        
        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            lock (_statsLock)
            {
                _eventPools.Clear();
                _poolStats.Clear();
            }
            Log("Cleared all event pools");
        }
        
        /// <summary>
        /// Clear pool for specific event type
        /// </summary>
        /// <typeparam name="T">Type of event pool to clear</typeparam>
        public void ClearPool<T>() where T : BaseEvent
        {
            var type = typeof(T);
            if (_eventPools.TryRemove(type, out _))
            {
                lock (_statsLock)
                {
                    _poolStats.Remove(type);
                }
                Log($"Cleared pool for {type.Name}");
            }
        }
        
        /// <summary>
        /// Get statistics for all pools
        /// </summary>
        /// <returns>Dictionary of pool statistics by type</returns>
        public Dictionary<Type, PoolStats> GetAllPoolStats()
        {
            lock (_statsLock)
            {
                return new Dictionary<Type, PoolStats>(_poolStats);
            }
        }
        
        /// <summary>
        /// Get statistics for specific pool
        /// </summary>
        /// <typeparam name="T">Type of event pool</typeparam>
        /// <returns>Pool statistics</returns>
        public PoolStats GetPoolStats<T>() where T : BaseEvent
        {
            var type = typeof(T);
            lock (_statsLock)
            {
                return _poolStats.TryGetValue(type, out var stats) ? stats : new PoolStats();
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private ConcurrentQueue<BaseEvent> GetOrCreatePool(Type eventType)
        {
            return _eventPools.GetOrAdd(eventType, _ => new ConcurrentQueue<BaseEvent>());
        }
        
        private int GetPoolSize(Type eventType)
        {
            if (_eventPools.TryGetValue(eventType, out var pool))
            {
                return pool.Count;
            }
            return 0;
        }
        
        private void UpdateStats(Type eventType, bool created, bool retrieved)
        {
            lock (_statsLock)
            {
                if (!_poolStats.ContainsKey(eventType))
                {
                    _poolStats[eventType] = new PoolStats();
                }
                
                var stats = _poolStats[eventType];
                if (created) stats.TotalCreated++;
                if (retrieved) stats.TotalRetrieved++;
                stats.CurrentPoolSize = GetPoolSize(eventType);
                _poolStats[eventType] = stats;
            }
        }
        
        private void Log(string message)
        {
            if (enableLogging)
                Debug.Log($"[EventPool] {message}");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Statistics for an event pool
    /// </summary>
    [Serializable]
    public struct PoolStats
    {
        /// <summary>
        /// Total number of instances created
        /// </summary>
        public int TotalCreated;
        
        /// <summary>
        /// Total number of instances retrieved from pool
        /// </summary>
        public int TotalRetrieved;
        
        /// <summary>
        /// Current number of instances in pool
        /// </summary>
        public int CurrentPoolSize;
        
        /// <summary>
        /// Pool hit rate (retrieved / (created + retrieved))
        /// </summary>
        public float HitRate => TotalCreated + TotalRetrieved > 0 
            ? (float)TotalRetrieved / (TotalCreated + TotalRetrieved) 
            : 0f;
    }
}