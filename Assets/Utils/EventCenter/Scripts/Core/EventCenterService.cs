using System;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Service locator for EventCenter - provides access to event system without singleton
    /// Supports dependency injection and better testability
    /// </summary>
    public static class EventCenterService
    {
        #region Service Locator
        
        private static IEventCenter _current;
        
        /// <summary>
        /// Current active EventCenter instance
        /// </summary>
        public static IEventCenter Current
        {
            get
            {
                if (_current == null)
                {
                    // Try to find EventCenter in scene
                    var eventCenter = UnityEngine.Object.FindFirstObjectByType<EventCenter>();
                    if (eventCenter != null)
                    {
                        SetCurrent(eventCenter);
                    }
                    else
                    {
                        // Try to find EventCenterSetup which might create one
                        var eventCenterSetup = UnityEngine.Object.FindFirstObjectByType<EventCenterSetup>();
                        if (eventCenterSetup != null)
                        {
                            // EventCenterSetup should handle creation
                        }
                        else
                        {
                            // Auto-create EventCenter using config settings
                            var config = EventCenterConfig.Instance;
                            if (config.autoCreateEventCenter)
                            {
                                CreateAndSetCurrent(config.autoCreatedName, config.dontDestroyOnLoad);
                            }
                            else
                            {
                                // Auto-creation is disabled
                            }
                        }
                    }
                }
                return _current;
            }
        }
        
        /// <summary>
        /// Check if EventCenter service is available
        /// </summary>
        public static bool IsAvailable => _current != null;
        
        #endregion
        
        #region Service Management
        
        /// <summary>
        /// Set the current EventCenter instance
        /// </summary>
        /// <param name="eventCenter">EventCenter instance to use</param>
        public static void SetCurrent(IEventCenter eventCenter)
        {
            _current = eventCenter;
        }
        
        /// <summary>
        /// Clear the current EventCenter instance
        /// </summary>
        public static void ClearCurrent()
        {
            _current = null;
        }
        
        /// <summary>
        /// Create and set a new EventCenter instance
        /// </summary>
        /// <param name="name">Name for the EventCenter GameObject</param>
        /// <param name="dontDestroyOnLoad">Whether to persist across scenes</param>
        /// <returns>Created EventCenter instance</returns>
        public static EventCenter CreateAndSetCurrent(string name = "EventCenter", bool dontDestroyOnLoad = true)
        {
            var go = new GameObject(name);
            var eventCenter = go.AddComponent<EventCenter>();
            
            if (dontDestroyOnLoad)
            {
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
            
            SetCurrent(eventCenter);
            return eventCenter;
        }
        
        #endregion
        
        #region Convenience API
        
        /// <summary>
        /// Publish an event using the current EventCenter
        /// </summary>
        /// <param name="eventData">Event to publish</param>
        public static void Publish(BaseEvent eventData)
        {
            Current?.PublishEvent(eventData);
        }
        
        /// <summary>
        /// Publish an event immediately using the current EventCenter
        /// </summary>
        /// <param name="eventData">Event to publish immediately</param>
        public static void PublishImmediate(BaseEvent eventData)
        {
            Current?.PublishEventImmediate(eventData);
        }
        
        /// <summary>
        /// Subscribe to events using the current EventCenter
        /// </summary>
        /// <typeparam name="T">Type of event to subscribe to</typeparam>
        /// <param name="callback">Callback function</param>
        /// <param name="priority">Priority of the subscription</param>
        /// <returns>Subscription handle</returns>
        public static IEventSubscription Subscribe<T>(Action<T> callback, int priority = 0) where T : BaseEvent
        {
            return Current?.SubscribeLegacy(callback, priority);
        }
        
        /// <summary>
        /// Subscribe to events using the current EventCenter
        /// </summary>
        /// <typeparam name="T">Type of event to subscribe to</typeparam>
        /// <param name="listener">Listener instance</param>
        /// <returns>Subscription handle</returns>
        public static IEventSubscription Subscribe<T>(IEventListenerLegacy<T> listener) where T : BaseEvent
        {
            return Current?.SubscribeLegacy(listener);
        }
        
        /// <summary>
        /// Subscribe to struct events using the current EventCenter
        /// </summary>
        /// <typeparam name="T">Type of struct to subscribe to</typeparam>
        /// <param name="callback">Callback function</param>
        /// <param name="priority">Priority of the subscription</param>
        /// <returns>Subscription handle</returns>
        public static IEventSubscription SubscribeToStruct<T>(Action<T> callback, int priority = 0) where T : struct
        {
            return Current?.Subscribe(callback, priority);
        }
        
        /// <summary>
        /// Subscribe to struct events using the current EventCenter
        /// </summary>
        /// <typeparam name="T">Type of struct to subscribe to</typeparam>
        /// <param name="listener">Listener instance</param>
        /// <returns>Subscription handle</returns>
        public static IEventSubscription SubscribeToStruct<T>(IEventListener<T> listener) where T : struct
        {
            return Current?.Subscribe(listener);
        }
        
        /// <summary>
        /// Publish a struct event using the current EventCenter
        /// </summary>
        /// <typeparam name="T">Type of struct to publish</typeparam>
        /// <param name="payload">The struct payload</param>
        /// <param name="priority">Event priority</param>
        public static void PublishStruct<T>(T payload, int priority = 0) where T : struct
        {
            Current?.PublishEvent(payload, priority);
        }
        
        /// <summary>
        /// Publish a struct event immediately using the current EventCenter
        /// </summary>
        /// <typeparam name="T">Type of struct to publish</typeparam>
        /// <param name="payload">The struct payload</param>
        /// <param name="priority">Event priority</param>
        public static void PublishStructImmediate<T>(T payload, int priority = 0) where T : struct
        {
            Current?.PublishEventImmediate(payload, priority);
        }
        
        /// <summary>
        /// Create and publish an event using the current EventCenter
        /// </summary>
        /// <typeparam name="T">Type of event to create</typeparam>
        /// <param name="source">Source object for the event</param>
        /// <returns>Created event instance</returns>
        public static T CreateAndPublish<T>(object source = null) where T : BaseEvent, new()
        {
            if (Current is EventCenter eventCenter)
            {
                return eventCenter.CreateAndPublishEvent<T>(source);
            }
            
            // Fallback for other IEventCenter implementations
            var eventData = new T();
            eventData.Initialize(source);
            Current?.PublishEvent(eventData);
            return eventData;
        }
        
        /// <summary>
        /// Check if there are listeners for a specific event type
        /// </summary>
        /// <typeparam name="T">Type of event to check</typeparam>
        /// <returns>True if listeners exist</returns>
        public static bool HasListeners<T>() where T : BaseEvent
        {
            if (Current is EventCenter eventCenter)
            {
                return eventCenter.HasListeners<T>();
            }
            return false;
        }
        
        /// <summary>
        /// Get number of listeners for a specific event type
        /// </summary>
        /// <typeparam name="T">Type of event to check</typeparam>
        /// <returns>Number of listeners</returns>
        public static int GetListenerCount<T>() where T : BaseEvent
        {
            if (Current is EventCenter eventCenter)
            {
                return eventCenter.GetListenerCount<T>();
            }
            return 0;
        }
        
        /// <summary>
        /// Get statistics about the current EventCenter
        /// </summary>
        /// <returns>EventCenter statistics</returns>
        public static EventCenterStats? GetStats()
        {
            return Current?.GetStats();
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Ensure EventCenter service is available, create one if needed
        /// </summary>
        /// <param name="autoCreate">Whether to automatically create EventCenter if none exists</param>
        /// <returns>True if service is available</returns>
        public static bool EnsureAvailable(bool autoCreate = true)
        {
            if (IsAvailable)
                return true;
            
            if (autoCreate)
            {
                CreateAndSetCurrent();
                return IsAvailable;
            }
            
            return false;
        }
        
        /// <summary>
        /// Validate that EventCenter service is available, log warning if not
        /// </summary>
        /// <param name="operation">Name of operation being attempted</param>
        /// <returns>True if service is available</returns>
        public static bool ValidateAvailable(string operation = "operation")
        {
            if (!IsAvailable)
            {
                return false;
            }
            return true;
        }
        
        #endregion
    }
}