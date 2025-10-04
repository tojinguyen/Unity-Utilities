using System;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Static event system API for convenient event handling
    /// Provides easy-to-use static methods for subscribing, publishing, and managing events
    /// </summary>
    public static class EventSystem
    {
        #region Private Fields
        
        private static IEventCenter _eventCenter;
        private static bool _isInitialized = false;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the static event system
        /// This is called automatically on first use
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;
            
            _eventCenter = EventCenterService.Current;
            
            if (_eventCenter == null)
            {
                return;
            }
            
            _isInitialized = true;
        }
        
        /// <summary>
        /// Ensure the event system is initialized
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized || _eventCenter == null)
            {
                Initialize();
                
                // If initialization still failed, throw an exception
                if (_eventCenter == null)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        
        /// <summary>
        /// Check if the event system is initialized
        /// </summary>
        public static bool IsInitialized => _isInitialized;
        
        #endregion
        
        #region Struct Event Subscription (Primary API)
        
        /// <summary>
        /// Subscribe to struct events of a specific type
        /// </summary>
        /// <typeparam name="T">Type of struct to subscribe to</typeparam>
        /// <param name="listener">The listener to register</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        public static IEventSubscription Subscribe<T>(IEventListener<T> listener) where T : struct
        {
            EnsureInitialized();
            return _eventCenter.Subscribe(listener);
        }
        
        /// <summary>
        /// Subscribe to struct events with a callback function
        /// </summary>
        /// <typeparam name="T">Type of struct to subscribe to</typeparam>
        /// <param name="callback">Callback function to invoke</param>
        /// <param name="priority">Priority of this callback (higher = executed first)</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        public static IEventSubscription Subscribe<T>(Action<T> callback, int priority = 0) where T : struct
        {
            EnsureInitialized();
            var subscription = _eventCenter.Subscribe(callback, priority);
            return subscription;
        }
        
        /// <summary>
        /// Unsubscribe from a specific struct type
        /// </summary>
        /// <typeparam name="T">Type of struct to unsubscribe from</typeparam>
        /// <param name="listener">The listener to unsubscribe</param>
        public static void Unsubscribe<T>(IEventListener<T> listener) where T : struct
        {
            EnsureInitialized();
            _eventCenter.Unsubscribe(listener);
        }
        
        /// <summary>
        /// Unsubscribe a listener from all events
        /// </summary>
        /// <param name="listener">The listener to unsubscribe</param>
        public static void Unsubscribe(IEventListener listener)
        {
            EnsureInitialized();
            _eventCenter.Unsubscribe(listener);
        }
        
        #endregion
        
        #region Struct Event Publishing (Primary API)
        
        /// <summary>
        /// Publish a struct event to all registered listeners
        /// </summary>
        /// <typeparam name="T">Type of struct payload</typeparam>
        /// <param name="payload">The struct payload</param>
        /// <param name="priority">Event priority (higher value = higher priority)</param>
        public static void Publish<T>(T payload, int priority = 0) where T : struct
        {
            EnsureInitialized();
            _eventCenter.PublishEvent(payload, priority);
        }
        
        /// <summary>
        /// Publish a struct event with immediate processing (bypasses queue)
        /// </summary>
        /// <typeparam name="T">Type of struct payload</typeparam>
        /// <param name="payload">The struct payload to publish immediately</param>
        /// <param name="priority">Event priority (higher value = higher priority)</param>
        public static void PublishImmediate<T>(T payload, int priority = 0) where T : struct
        {
            EnsureInitialized();
            _eventCenter.PublishEventImmediate(payload, priority);
        }
        
        #endregion
        
        #region Legacy BaseEvent Support
        
        /// <summary>
        /// Subscribe to legacy BaseEvent types (for backward compatibility)
        /// </summary>
        /// <typeparam name="T">Type of BaseEvent to subscribe to</typeparam>
        /// <param name="listener">The listener to register</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        public static IEventSubscription SubscribeLegacy<T>(IEventListenerLegacy<T> listener) where T : BaseEvent
        {
            EnsureInitialized();
            return _eventCenter.SubscribeLegacy(listener);
        }
        
        /// <summary>
        /// Subscribe to legacy BaseEvent types with callback (for backward compatibility)
        /// </summary>
        /// <typeparam name="T">Type of BaseEvent to subscribe to</typeparam>
        /// <param name="callback">Callback function to invoke</param>
        /// <param name="priority">Priority of this callback</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        public static IEventSubscription SubscribeLegacy<T>(Action<T> callback, int priority = 0) where T : BaseEvent
        {
            EnsureInitialized();
            return _eventCenter.SubscribeLegacy(callback, priority);
        }
        
        /// <summary>
        /// Publish a legacy BaseEvent (for backward compatibility)
        /// </summary>
        /// <param name="eventData">The event to publish</param>
        public static void PublishLegacy(BaseEvent eventData)
        {
            EnsureInitialized();
            _eventCenter.PublishEvent(eventData);
        }
        
        /// <summary>
        /// Publish a legacy BaseEvent with immediate processing (for backward compatibility)
        /// </summary>
        /// <param name="eventData">The event to publish immediately</param>
        public static void PublishLegacyImmediate(BaseEvent eventData)
        {
            EnsureInitialized();
            _eventCenter.PublishEventImmediate(eventData);
        }
        
        #endregion
        
        #region System Management
        
        /// <summary>
        /// Process all queued events
        /// Usually called automatically by Unity's event loop
        /// </summary>
        public static void ProcessEvents()
        {
            if (!_isInitialized) return;
            _eventCenter.ProcessEvents();
        }
        
        /// <summary>
        /// Clear all events and subscriptions
        /// </summary>
        public static void Clear()
        {
            if (!_isInitialized) return;
            _eventCenter?.Clear();
        }
        
        /// <summary>
        /// Get statistics about the event system
        /// </summary>
        /// <returns>Event system statistics</returns>
        public static EventCenterStats GetStats()
        {
            if (!_isInitialized) return default;
            return _eventCenter.GetStats();
        }
        
        /// <summary>
        /// Log the current status and statistics of the event system
        /// </summary>
        public static void LogStatus()
        {
            if (!_isInitialized)
            {
                // Always log critical issues regardless of logging config
                ConsoleLogger.Log("[EventSystem] ❌ EventSystem is not initialized");
                return;
            }
            
            if (_eventCenter == null)
            {
                // Always log critical issues regardless of logging config
                ConsoleLogger.Log("[EventSystem] ❌ EventCenter is null");
                return;
            }
            
            // Check if logging is enabled in the EventCenter configuration
            if (!_eventCenter.IsLoggingEnabled)
            {
                // Silently return if logging is disabled
                return;
            }
            
            var stats = _eventCenter.GetStats();
            ConsoleLogger.Log($"[EventSystem] ✅ Status Report:");
            ConsoleLogger.Log($"[EventSystem] • Initialized: {_isInitialized}");
            ConsoleLogger.Log($"[EventSystem] • Active Subscriptions: {stats.ActiveSubscriptions}");
            ConsoleLogger.Log($"[EventSystem] • Queued Events: {stats.QueuedEvents}");
            ConsoleLogger.Log($"[EventSystem] • Events Processed This Frame: {stats.EventsProcessedThisFrame}");
            ConsoleLogger.Log($"[EventSystem] • Peak Events Per Frame: {stats.PeakEventsPerFrame}");
            ConsoleLogger.Log($"[EventSystem] • Pooled Events: {stats.PooledEvents}");
            ConsoleLogger.Log($"[EventSystem] • Average Processing Time: {stats.AverageProcessingTime:F2}ms");
            ConsoleLogger.Log($"[EventSystem] • Memory Usage: {stats.MemoryUsage} bytes");
        }
        
        /// <summary>
        /// Check if logging is enabled in the current EventCenter
        /// </summary>
        /// <returns>True if logging is enabled, false otherwise</returns>
        public static bool IsLoggingEnabled()
        {
            if (!_isInitialized || _eventCenter == null)
            {
                return false;
            }
            
            return _eventCenter.IsLoggingEnabled;
        }
        
        /// <summary>
        /// Shutdown the static event system
        /// </summary>
        public static void Shutdown()
        {
            if (_isInitialized)
            {
                Clear();
                _eventCenter = null;
                _isInitialized = false;
            }
        }
        
        #endregion
        
        #region Convenience Methods
        
        /// <summary>
        /// Subscribe to an event for a single execution (auto-unsubscribes after first trigger)
        /// </summary>
        /// <typeparam name="T">Type of struct to subscribe to</typeparam>
        /// <param name="callback">Callback function to invoke once</param>
        /// <param name="priority">Priority of this callback</param>
        /// <returns>Subscription handle</returns>
        public static IEventSubscription SubscribeOnce<T>(Action<T> callback, int priority = 0) where T : struct
        {
            IEventSubscription subscription = null;
            subscription = Subscribe<T>((payload) =>
            {
                callback(payload);
                subscription?.Dispose(); // Auto-unsubscribe after first execution
            }, priority);
            return subscription;
        }
        
        /// <summary>
        /// Subscribe to an event with a condition check
        /// </summary>
        /// <typeparam name="T">Type of struct to subscribe to</typeparam>
        /// <param name="callback">Callback function to invoke</param>
        /// <param name="condition">Condition that must be true for callback to execute</param>
        /// <param name="priority">Priority of this callback</param>
        /// <returns>Subscription handle</returns>
        public static IEventSubscription SubscribeWhen<T>(Action<T> callback, Func<T, bool> condition, int priority = 0) where T : struct
        {
            return Subscribe<T>((payload) =>
            {
                if (condition(payload))
                {
                    callback(payload);
                }
            }, priority);
        }
        
        /// <summary>
        /// Batch publish multiple events of the same type
        /// </summary>
        /// <typeparam name="T">Type of struct to publish</typeparam>
        /// <param name="events">Array of events to publish</param>
        /// <param name="priority">Priority for all events</param>
        public static void PublishBatch<T>(T[] events, int priority = 0) where T : struct
        {
            EnsureInitialized();
            foreach (var evt in events)
            {
                _eventCenter.PublishEvent(evt, priority);
            }
        }
        
        #endregion
        

    }
}