using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// High-performance object pool for EventWrapper instances
    /// Supports multi-threading and type-safe pooling for struct payloads
    /// </summary>
    public static class EventWrapperPool<T> where T : struct
    {
        #region Fields
        
        // Thread-safe pool for EventWrapper instances
        private static readonly ConcurrentQueue<EventWrapper<T>> _pool = new ConcurrentQueue<EventWrapper<T>>();
        
        // Pool statistics
        private static int _totalCreated;
        private static int _totalRetrieved;
        private static int _currentPoolSize;
        
        // Configuration
        private static readonly int _maxPoolSize = 1000;
        private static readonly bool _enableLogging = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Current number of instances in pool
        /// </summary>
        public static int CurrentPoolSize => _currentPoolSize;
        
        /// <summary>
        /// Total number of instances created
        /// </summary>
        public static int TotalCreated => _totalCreated;
        
        /// <summary>
        /// Total number of instances retrieved from pool
        /// </summary>
        public static int TotalRetrieved => _totalRetrieved;
        
        /// <summary>
        /// Pool hit rate (retrieved / (created + retrieved))
        /// </summary>
        public static float HitRate => _totalCreated + _totalRetrieved > 0 
            ? (float)_totalRetrieved / (_totalCreated + _totalRetrieved) 
            : 0f;
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get an EventWrapper instance from the pool
        /// </summary>
        /// <returns>EventWrapper instance from pool or newly created</returns>
        public static EventWrapper<T> Get()
        {
            if (_pool.TryDequeue(out var pooledWrapper))
            {
                pooledWrapper.Initialize();
                System.Threading.Interlocked.Decrement(ref _currentPoolSize);
                System.Threading.Interlocked.Increment(ref _totalRetrieved);
                Log($"Retrieved EventWrapper<{typeof(T).Name}> from pool");
                return pooledWrapper;
            }
            
            // Create new instance if pool is empty
            var newWrapper = new EventWrapper<T>();
            System.Threading.Interlocked.Increment(ref _totalCreated);
            Log($"Created new EventWrapper<{typeof(T).Name}> instance");
            return newWrapper;
        }
        
        /// <summary>
        /// Get an EventWrapper instance with payload
        /// </summary>
        /// <param name="payload">The struct payload</param>
        /// <param name="source">Source object</param>
        /// <param name="priority">Event priority</param>
        /// <param name="isImmediate">Whether to process immediately</param>
        /// <returns>EventWrapper instance with payload</returns>
        public static EventWrapper<T> Get(T payload, object source = null, int priority = 0, bool isImmediate = false)
        {
            var wrapper = Get();
            wrapper.Payload = payload;
            wrapper.Initialize(source);
            wrapper.SetPriority(priority);
            wrapper.SetIsImmediate(isImmediate);
            return wrapper;
        }
        
        /// <summary>
        /// Return an EventWrapper instance to the pool
        /// </summary>
        /// <param name="wrapper">EventWrapper to return to pool</param>
        public static void Return(EventWrapper<T> wrapper)
        {
            if (wrapper == null || !wrapper.IsPoolable)
                return;
            
            // Check pool size limit
            if (_currentPoolSize >= _maxPoolSize)
            {
                Log($"Pool for EventWrapper<{typeof(T).Name}> has reached max size ({_maxPoolSize}). Discarding wrapper.");
                return;
            }
            
            // Reset wrapper and return to pool
            wrapper.Reset();
            _pool.Enqueue(wrapper);
            System.Threading.Interlocked.Increment(ref _currentPoolSize);
            Log($"Returned EventWrapper<{typeof(T).Name}> to pool");
        }
        
        /// <summary>
        /// Pre-warm the pool with initial instances
        /// </summary>
        /// <param name="count">Number of instances to create</param>
        public static void PreWarm(int count = 50)
        {
            for (int i = 0; i < count; i++)
            {
                var wrapper = new EventWrapper<T>();
                wrapper.Reset();
                _pool.Enqueue(wrapper);
                System.Threading.Interlocked.Increment(ref _currentPoolSize);
            }
            
            Log($"Pre-warmed {count} instances of EventWrapper<{typeof(T).Name}>");
        }
        
        /// <summary>
        /// Clear the pool
        /// </summary>
        public static void Clear()
        {
            while (_pool.TryDequeue(out _))
            {
                System.Threading.Interlocked.Decrement(ref _currentPoolSize);
            }
            
            Log($"Cleared pool for EventWrapper<{typeof(T).Name}>");
        }
        
        /// <summary>
        /// Get pool statistics
        /// </summary>
        /// <returns>Pool statistics</returns>
        public static PoolStats GetStats()
        {
            return new PoolStats
            {
                TotalCreated = _totalCreated,
                TotalRetrieved = _totalRetrieved,
                CurrentPoolSize = _currentPoolSize
            };
        }
        
        #endregion
        
        #region Private Methods
        
        private static void Log(string message)
        {
            if (_enableLogging)
                Debug.Log($"[EventWrapperPool<{typeof(T).Name}>] {message}");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Non-generic pool manager for EventWrapper pools
    /// </summary>
    public static class EventWrapperPoolManager
    {
        private static readonly Dictionary<Type, object> _pools = new Dictionary<Type, object>();
        
        /// <summary>
        /// Get pool statistics for a specific type
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <returns>Pool statistics</returns>
        public static PoolStats GetPoolStats<T>() where T : struct
        {
            return EventWrapperPool<T>.GetStats();
        }
        
        /// <summary>
        /// Pre-warm pool for a specific type
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="count">Number of instances to pre-warm</param>
        public static void PreWarmPool<T>(int count = 50) where T : struct
        {
            EventWrapperPool<T>.PreWarm(count);
        }
        
        /// <summary>
        /// Clear pool for a specific type
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        public static void ClearPool<T>() where T : struct
        {
            EventWrapperPool<T>.Clear();
        }
        
        /// <summary>
        /// Clear all pools
        /// </summary>
        public static void ClearAllPools()
        {
            // Since we're using static generic classes, we can't easily enumerate all types
            // This would need to be implemented differently if needed
            Debug.Log("[EventWrapperPoolManager] Individual pool clearing required for static generic pools");
        }
    }
}