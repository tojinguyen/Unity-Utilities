using System;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Service locator for EventPool - provides access to event pooling without singleton
    /// </summary>
    public static class EventPoolService
    {
        #region Service Locator
        
        private static EventPool _current;
        
        /// <summary>
        /// Current active EventPool instance
        /// </summary>
        public static EventPool Current
        {
            get
            {
                if (_current == null)
                {
                    // Try to find EventPool in scene
                    var eventPool = UnityEngine.Object.FindFirstObjectByType<EventPool>();
                    if (eventPool != null)
                    {
                        SetCurrent(eventPool);
                    }
                    else
                    {
                        // Create a default EventPool if none exists
                        CreateAndSetCurrent();
                    }
                }
                return _current;
            }
        }
        
        /// <summary>
        /// Check if EventPool service is available
        /// </summary>
        public static bool IsAvailable => _current != null;
        
        #endregion
        
        #region Service Management
        
        /// <summary>
        /// Set the current EventPool instance
        /// </summary>
        /// <param name="eventPool">EventPool instance to use</param>
        public static void SetCurrent(EventPool eventPool)
        {
            _current = eventPool;
        }
        
        /// <summary>
        /// Clear the current EventPool instance
        /// </summary>
        public static void ClearCurrent()
        {
            _current = null;
        }
        
        /// <summary>
        /// Create and set a new EventPool instance
        /// </summary>
        /// <param name="name">Name for the EventPool GameObject</param>
        /// <param name="dontDestroyOnLoad">Whether to persist across scenes</param>
        /// <returns>Created EventPool instance</returns>
        public static EventPool CreateAndSetCurrent(string name = "EventPool", bool dontDestroyOnLoad = true)
        {
            var go = new GameObject(name);
            var eventPool = go.AddComponent<EventPool>();
            
            if (dontDestroyOnLoad)
            {
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
            
            SetCurrent(eventPool);
            return eventPool;
        }
        
        #endregion
        
        #region Convenience API
        
        /// <summary>
        /// Get an event instance from the pool
        /// </summary>
        /// <typeparam name="T">Type of event to get</typeparam>
        /// <returns>Event instance from pool or newly created</returns>
        public static T Get<T>() where T : BaseEvent, new()
        {
            return Current?.Get<T>() ?? new T();
        }
        
        /// <summary>
        /// Return an event instance to the pool
        /// </summary>
        /// <param name="eventInstance">Event to return to pool</param>
        public static void Return(BaseEvent eventInstance)
        {
            Current?.Return(eventInstance);
        }
        
        /// <summary>
        /// Pre-warm a pool with initial instances
        /// </summary>
        /// <typeparam name="T">Type of event to pre-warm</typeparam>
        /// <param name="count">Number of instances to create</param>
        public static void PreWarm<T>(int count = -1) where T : BaseEvent, new()
        {
            Current?.PreWarm<T>(count);
        }
        
        /// <summary>
        /// Clear all pools
        /// </summary>
        public static void ClearAllPools()
        {
            Current?.ClearAllPools();
        }
        
        /// <summary>
        /// Clear pool for specific event type
        /// </summary>
        /// <typeparam name="T">Type of event pool to clear</typeparam>
        public static void ClearPool<T>() where T : BaseEvent
        {
            Current?.ClearPool<T>();
        }
        
        /// <summary>
        /// Get statistics for specific pool
        /// </summary>
        /// <typeparam name="T">Type of event pool</typeparam>
        /// <returns>Pool statistics</returns>
        public static PoolStats GetPoolStats<T>() where T : BaseEvent
        {
            return Current?.GetPoolStats<T>() ?? new PoolStats();
        }
        
        #endregion
    }
}