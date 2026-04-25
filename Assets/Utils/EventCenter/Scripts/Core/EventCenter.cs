using System;
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Main Event Center - Central hub for all event-driven communication
    /// Designed for high-performance processing of thousands of events per frame
    /// Can be used as a service instance rather than singleton for better testability
    /// </summary>
    public class EventCenter : MonoBehaviour, IEventCenter
    {
        
        #region Configuration
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableLogging = false;
        
        #endregion
        
        #region Fields
        
        private EventDispatcher _dispatcher;
        private readonly List<IEventSubscription> _subscriptions = new List<IEventSubscription>();
        private readonly Dictionary<IEventListener, List<IEventSubscription>> _listenerSubscriptions
            = new Dictionary<IEventListener, List<IEventSubscription>>();
        private EventCenterStats _stats;
        private bool _isInitialized;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            Initialize();
            
            // Register with service locator (legacy BaseEvent support)
            EventCenterService.SetCurrent(this);
            
            // Wire struct-event system to this instance
            EventSystem.Initialize(this);
        }
        
        private void Update() { }
        private void FixedUpdate() { }
        private void LateUpdate() { UpdateStats(); }
        
        private void OnDestroy()
        {
            // Clear EventCenterService if this was the current instance
            if (ReferenceEquals(EventCenterService.Current, this))
            {
                EventCenterService.SetCurrent(null);
            }
            
            EventSystem.Shutdown();
            Clear();
        }
        
        #endregion
        
        #region Initialization
        
        private void Initialize()
        {
            if (_isInitialized) return;
            _dispatcher = new EventDispatcher(enableLogging);
            _stats = new EventCenterStats();
            _isInitialized = true;
        }
        
        #endregion
        
        #region IEventCenter Implementation
        
        /// <summary>
        /// Subscribe to events of a specific type
        /// </summary>
        public IEventSubscription Subscribe<T>(IEventListener<T> listener) where T : struct
        {
            if (listener == null) return null;
            
            var eventType = typeof(T);
            // Call the generic Subscribe method for struct types
            _dispatcher.Subscribe<T>(listener);
            
            var subscription = new EventSubscription(eventType, listener, () => Unsubscribe(listener));
            _subscriptions.Add(subscription);
            
            if (!_listenerSubscriptions.ContainsKey(listener))
            {
                _listenerSubscriptions[listener] = new List<IEventSubscription>();
            }
            _listenerSubscriptions[listener].Add(subscription);
            
            return subscription;
        }
        
        /// <summary>
        /// Subscribe to events with a callback function
        /// </summary>
        public IEventSubscription Subscribe<T>(Action<T> callback, int priority = 0) where T : struct
        {
            if (callback == null) return null;
            
            var listener = new CallbackEventListener<T>(callback, priority);
            return Subscribe<T>(listener);
        }
        
        /// <summary>
        /// Unsubscribe a listener from events
        /// </summary>
        public void Unsubscribe(IEventListener listener)
        {
            if (listener == null) return;
            
            _dispatcher.UnsubscribeAll(listener);
            
            if (_listenerSubscriptions.TryGetValue(listener, out var subscriptions))
            {
                foreach (var subscription in subscriptions)
                {
                    subscription.IsActive = false;
                    _subscriptions.Remove(subscription);
                }
                _listenerSubscriptions.Remove(listener);
            }
        }
        
        /// <summary>
        /// Unsubscribe from a specific event type
        /// </summary>
        public void Unsubscribe<T>(IEventListener<T> listener) where T : struct
        {
            if (listener == null) return;
            
            var eventType = typeof(T);
            // Call the generic Unsubscribe method for struct types
            _dispatcher.Unsubscribe<T>(listener);
        }
        
        /// <summary>
        /// Publish an event to the event system
        /// </summary>
        public void PublishEvent(BaseEvent eventData)
        {
            if (eventData == null || eventData.IsDisposed) return;
#if UNITY_EDITOR
            NotifyEditorOnPublish(eventData);
#endif
            _dispatcher.Dispatch(eventData);
            _stats.EventsProcessedThisFrame++;
        }
        
        /// <summary>
        /// Publish an event with immediate processing
        /// </summary>
        public void PublishEventImmediate(BaseEvent eventData)
        {
            if (eventData == null || eventData.IsDisposed) return;
            
#if UNITY_EDITOR
            // Hook for Editor tracking
            NotifyEditorOnPublish(eventData);
#endif
            
            var listenersNotified = _dispatcher.Dispatch(eventData);
            _stats.EventsProcessedThisFrame++;
            
            // Return to pool if poolable
            if (eventData.IsPoolable)
            {
                eventData.Dispose();
            }
        }
        
        /// <summary>
        /// No-op — events are dispatched synchronously on Publish. Kept for interface compatibility.
        /// </summary>
        public void ProcessEvents() { }
        
        /// <summary>
        /// Clear all events and subscriptions
        /// </summary>
        public void Clear()
        {
            _dispatcher?.Clear();
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
            _subscriptions.Clear();
            _listenerSubscriptions.Clear();
        }
        
        /// <summary>
        /// Get statistics about the event system
        /// </summary>
        public EventCenterStats GetStats()
        {
            return _stats;
        }
        
        /// <summary>
        /// Check if logging is enabled for this EventCenter
        /// </summary>
        public bool IsLoggingEnabled => enableLogging;
        
        /// <summary>
        /// Publish a struct event to the event system
        /// </summary>
        /// <typeparam name="T">Type of struct payload</typeparam>
        /// <param name="payload">The struct payload</param>
        /// <param name="priority">Event priority (higher value = higher priority)</param>
        public void PublishEvent<T>(T payload, int priority = 0) where T : struct
        {
            // Struct events are dispatched via EventHub<T> directly from EventSystem.Publish().
            // This method is kept for IEventCenter interface compatibility (legacy usage).
            EventHub<T>.Publish(payload);
        }
        
        /// <summary>
        /// Publish a struct event with immediate processing
        /// </summary>
        /// <typeparam name="T">Type of struct payload</typeparam>
        /// <param name="payload">The struct payload to publish immediately</param>
        /// <param name="priority">Event priority (higher value = higher priority)</param>
        public void PublishEventImmediate<T>(T payload, int priority = 0) where T : struct
        {
            // Immediate struct publish — same as regular since EventHub<T> is always synchronous
            EventHub<T>.Publish(payload);
        }
        
        /// <summary>
        /// Legacy method for subscribing to BaseEvent types
        /// </summary>
        /// <typeparam name="T">Type of BaseEvent to subscribe to</typeparam>
        /// <param name="listener">The listener to register</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        public IEventSubscription SubscribeLegacy<T>(IEventListenerLegacy<T> listener) where T : BaseEvent
        {
            if (listener == null) return null;
            
            var eventType = typeof(T);
            _dispatcher.Subscribe(eventType, listener);
            
            var subscription = new EventSubscription(eventType, listener, () => Unsubscribe(listener));
            _subscriptions.Add(subscription);
            
            if (!_listenerSubscriptions.ContainsKey(listener))
            {
                _listenerSubscriptions[listener] = new List<IEventSubscription>();
            }
            _listenerSubscriptions[listener].Add(subscription);
            
            return subscription;
        }
        
        /// <summary>
        /// Legacy method for subscribing to BaseEvent types with callback
        /// </summary>
        /// <typeparam name="T">Type of BaseEvent to subscribe to</typeparam>
        /// <param name="callback">Callback function to invoke</param>
        /// <param name="priority">Priority of this callback</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        public IEventSubscription SubscribeLegacy<T>(Action<T> callback, int priority = 0) where T : BaseEvent
        {
            if (callback == null) return null;
            
            var listener = new CallbackEventLegacyListener<T>(callback, priority);
            return SubscribeLegacy<T>(listener);
        }
        
        #endregion
        
        #region Event Processing
        #endregion
        
        #region Statistics
        
        private void UpdateStats()
        {
            _stats.QueuedEvents = 0;
            _stats.ActiveSubscriptions = _subscriptions.Count;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Create and publish an event of a specific type
        /// </summary>
        public T CreateAndPublishEvent<T>(object source = null) where T : BaseEvent, new()
        {
            var eventData = new T();
            eventData.Initialize(source);
            PublishEvent(eventData);
            return eventData;
        }
        
        public T CreateEvent<T>(object source = null) where T : BaseEvent, new()
        {
            var eventData = new T();
            eventData.Initialize(source);
            return eventData;
        }
        
        /// <summary>
        /// Get number of listeners for a specific event type
        /// </summary>
        public int GetListenerCount<T>() where T : BaseEvent
        {
            return _dispatcher.GetListenerCount(typeof(T));
        }
        
        /// <summary>
        /// Check if there are any listeners for a specific event type
        /// </summary>
        public bool HasListeners<T>() where T : BaseEvent
        {
            return GetListenerCount<T>() > 0;
        }
        

        
        #endregion
        
        #region Public Static Methods - Deprecated (Use Service Locator instead)
        
        /// <summary>
        /// Static convenience method to publish an event
        /// DEPRECATED: Use EventCenterService.Current instead
        /// </summary>
        [System.Obsolete("Use EventCenterService.Current.PublishEvent() instead")]
        public static void Publish(BaseEvent eventData)
        {
            EventCenterService.Current?.PublishEvent(eventData);
        }
        
        /// <summary>
        /// Static convenience method to publish an event immediately
        /// DEPRECATED: Use EventCenterService.Current instead
        /// </summary>
        [System.Obsolete("Use EventCenterService.Current.PublishEventImmediate() instead")]
        public static void PublishImmediate(BaseEvent eventData)
        {
            EventCenterService.Current?.PublishEventImmediate(eventData);
        }
        
        /// <summary>
        /// Static convenience method to subscribe to events
        /// DEPRECATED: Use EventCenterService.Current instead
        /// </summary>
        [System.Obsolete("Use EventCenterService.Current.Subscribe() instead")]
        public static IEventSubscription Listen<T>(Action<T> callback, int priority = 0) where T : BaseEvent
        {
            return EventCenterService.Subscribe(callback, priority);
        }
        
        #endregion
        
        #region Editor Integration
        
#if UNITY_EDITOR
        // ── Editor notification for legacy BaseEvent publish ──────────────────
        // Uses a cached method reference (looked up once, not per-event).
        
        private static System.Reflection.MethodInfo _editorPublishMethod;
        private static bool _editorPublishLookupDone;
        
        private void NotifyEditorOnPublish(BaseEvent eventData)
        {
            var method = GetEditorPublishMethod();
            if (method == null) return;
            
            try
            {
                var category = ResolveEventCategory(eventData.GetType().Name);
                method.Invoke(null, new object[] { eventData.GetType().Name, eventData, this, category, null });
            }
            catch { }
        }
        
        private static System.Reflection.MethodInfo GetEditorPublishMethod()
        {
            if (_editorPublishLookupDone) return _editorPublishMethod;
            _editorPublishLookupDone = true;
            
            var bridgeType = System.Type.GetType(
                "EventCenter.EditorTools.EventCaptureBridge, Assembly-CSharp-Editor");
            _editorPublishMethod = bridgeType?.GetMethod(
                "Publish",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return _editorPublishMethod;
        }
        
        private static string ResolveEventCategory(string name)
        {
            if (name.Contains("Player"))  return "Player";
            if (name.Contains("UI"))      return "UI";
            if (name.Contains("Game"))    return "Gameplay";
            if (name.Contains("Audio") || name.Contains("Sound")) return "Audio";
            if (name.Contains("Network")) return "Network";
            return "Uncategorized";
        }
#endif
        
        #endregion
    }
}