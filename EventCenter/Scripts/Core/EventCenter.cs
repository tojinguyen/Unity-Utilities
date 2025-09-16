using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
        
        [Header("Performance Settings")]
        [SerializeField] private int maxEventsPerFrame = 10000;
        [SerializeField] private int maxBatchSize = 1000;
        [SerializeField] private bool processEventsInUpdate = true;
        [SerializeField] private bool processEventsInFixedUpdate = false;
        [SerializeField] private bool processEventsInLateUpdate = false;
        
        [Header("Pooling Settings")]
        [SerializeField] private int initialPoolSize = 100;
        [SerializeField] private int maxPoolSize = 1000;
        [SerializeField] private bool preWarmPools = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableLogging = false;
        [SerializeField] private bool enableProfiling = false;
        [SerializeField] private bool showStats = false;
        
        #endregion
        
        #region Fields
        
        // Core components
        private EventDispatcher _dispatcher;
        private EventQueue _eventQueue;
        private EventQueue _immediateQueue;
        
        // Subscription management
        private readonly List<IEventSubscription> _subscriptions = new List<IEventSubscription>();
        private readonly Dictionary<IEventListener, List<IEventSubscription>> _listenerSubscriptions 
            = new Dictionary<IEventListener, List<IEventSubscription>>();
        
        // Performance tracking
        private EventCenterStats _stats;
        private readonly List<float> _frameTimes = new List<float>();
        private int _frameEventCount;
        private float _frameStartTime;
        
        // State management
        private bool _isInitialized;
        private bool _isProcessing;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            Initialize();
        }
        
        private void Start()
        {
            if (preWarmPools)
            {
                PreWarmCommonEventTypes();
            }
        }
        
        private void Update()
        {
            if (processEventsInUpdate)
            {
                ProcessEventsInternal();
            }
        }
        
        private void FixedUpdate()
        {
            if (processEventsInFixedUpdate)
            {
                ProcessEventsInternal();
            }
        }
        
        private void LateUpdate()
        {
            if (processEventsInLateUpdate)
            {
                ProcessEventsInternal();
            }
            
            UpdateStats();
        }
        
        private void OnDestroy()
        {
            Clear();
        }
        
        #endregion
        
        #region Initialization
        
        private void Initialize()
        {
            if (_isInitialized) return;
            
            // Initialize components
            _dispatcher = new EventDispatcher(enableLogging);
            _eventQueue = new EventQueue(initialPoolSize, maxEventsPerFrame, maxBatchSize, enableLogging);
            _immediateQueue = new EventQueue(50, 1000, 100, enableLogging);
            
            // Initialize stats
            _stats = new EventCenterStats();
            
            _isInitialized = true;
            Log("EventCenter initialized");
        }
        
        private void PreWarmCommonEventTypes()
        {
            // Pre-warm pools for common event types
            // You can extend this based on your game's common events
            Log("Pre-warming event pools...");
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
            _dispatcher.Subscribe(eventType, listener);
            
            var subscription = new EventSubscription(eventType, listener, () => Unsubscribe(listener));
            _subscriptions.Add(subscription);
            
            if (!_listenerSubscriptions.ContainsKey(listener))
            {
                _listenerSubscriptions[listener] = new List<IEventSubscription>();
            }
            _listenerSubscriptions[listener].Add(subscription);
            
            Log($"Subscribed to {eventType.Name}");
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
            
            Log($"Unsubscribed listener");
        }
        
        /// <summary>
        /// Unsubscribe from a specific event type
        /// </summary>
        public void Unsubscribe<T>(IEventListener<T> listener) where T : struct
        {
            if (listener == null) return;
            
            var eventType = typeof(T);
            _dispatcher.Unsubscribe(eventType, listener);
            
            Log($"Unsubscribed from {eventType.Name}");
        }
        
        /// <summary>
        /// Publish an event to the event system
        /// </summary>
        public void PublishEvent(BaseEvent eventData)
        {
            if (eventData == null || eventData.IsDisposed) return;
            
            if (eventData.IsImmediate)
            {
                _immediateQueue.Enqueue(eventData);
            }
            else
            {
                _eventQueue.Enqueue(eventData);
            }
            
            Log($"Published {eventData.GetType().Name}");
        }
        
        /// <summary>
        /// Publish an event with immediate processing
        /// </summary>
        public void PublishEventImmediate(BaseEvent eventData)
        {
            if (eventData == null || eventData.IsDisposed) return;
            
            var listenersNotified = _dispatcher.Dispatch(eventData);
            _stats.EventsProcessedThisFrame++;
            
            // Return to pool if poolable
            if (eventData.IsPoolable)
            {
                eventData.Dispose();
            }
            
            Log($"Immediately processed {eventData.GetType().Name} to {listenersNotified} listeners");
        }
        
        /// <summary>
        /// Process all queued events
        /// </summary>
        public void ProcessEvents()
        {
            ProcessEventsInternal();
        }
        
        /// <summary>
        /// Clear all events and subscriptions
        /// </summary>
        public void Clear()
        {
            _eventQueue?.Clear();
            _immediateQueue?.Clear();
            _dispatcher?.Clear();
            
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
            _listenerSubscriptions.Clear();
            
            Log("Cleared EventCenter");
        }
        
        /// <summary>
        /// Get statistics about the event system
        /// </summary>
        public EventCenterStats GetStats()
        {
            return _stats;
        }
        
        /// <summary>
        /// Publish a struct event to the event system
        /// </summary>
        /// <typeparam name="T">Type of struct payload</typeparam>
        /// <param name="payload">The struct payload</param>
        /// <param name="priority">Event priority (higher value = higher priority)</param>
        public void PublishEvent<T>(T payload, int priority = 0) where T : struct
        {
            var eventWrapper = new EventWrapper<T>(payload, priority);
            
            if (priority > 0)
            {
                _immediateQueue.Enqueue(eventWrapper);
            }
            else
            {
                _eventQueue.Enqueue(eventWrapper);
            }
            
            Log($"Published struct event {typeof(T).Name}");
        }
        
        /// <summary>
        /// Publish a struct event with immediate processing
        /// </summary>
        /// <typeparam name="T">Type of struct payload</typeparam>
        /// <param name="payload">The struct payload to publish immediately</param>
        /// <param name="priority">Event priority (higher value = higher priority)</param>
        public void PublishEventImmediate<T>(T payload, int priority = 0) where T : struct
        {
            var eventWrapper = new EventWrapper<T>(payload, priority);
            var listenersNotified = _dispatcher.Dispatch(eventWrapper);
            _stats.EventsProcessedThisFrame++;
            
            Log($"Immediately processed struct event {typeof(T).Name} to {listenersNotified} listeners");
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
            
            Log($"Legacy subscribed to {eventType.Name}");
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
        
        private void ProcessEventsInternal()
        {
            if (_isProcessing || !_isInitialized) return;
            
            _isProcessing = true;
            _frameStartTime = Time.realtimeSinceStartup;
            _frameEventCount = 0;
            
            try
            {
                // Process immediate events first
                ProcessImmediateEvents();
                
                // Process regular events
                ProcessRegularEvents();
            }
            finally
            {
                _isProcessing = false;
            }
        }
        
        private void ProcessImmediateEvents()
        {
            var processed = _immediateQueue.ProcessBatch(ProcessSingleEvent, maxBatchSize);
            _frameEventCount += processed;
        }
        
        private void ProcessRegularEvents()
        {
            var remainingEvents = maxEventsPerFrame - _frameEventCount;
            if (remainingEvents <= 0) return;
            
            var processed = _eventQueue.ProcessBatch(ProcessSingleEvent, remainingEvents);
            _frameEventCount += processed;
        }
        
        private void ProcessSingleEvent(BaseEvent eventData)
        {
            if (eventData == null || eventData.IsDisposed || !eventData.IsValid())
                return;
            
            var listenersNotified = _dispatcher.Dispatch(eventData);
            
            // Return to pool if poolable
            if (eventData.IsPoolable)
            {
                eventData.Dispose();
            }
        }
        
        #endregion
        
        #region Statistics
        
        private void UpdateStats()
        {
            var frameTime = Time.realtimeSinceStartup - _frameStartTime;
            _frameTimes.Add(frameTime);
            
            // Keep only last 60 frames for average calculation
            if (_frameTimes.Count > 60)
            {
                _frameTimes.RemoveAt(0);
            }
            
            _stats.EventsProcessedThisFrame = _frameEventCount;
            _stats.QueuedEvents = _eventQueue.Count + _immediateQueue.Count;
            _stats.ActiveSubscriptions = _subscriptions.Count;
            _stats.PooledEvents = 0; // Will be updated by EventPool
            _stats.AverageProcessingTime = _frameTimes.Count > 0 
                ? _frameTimes.Sum() / _frameTimes.Count * 1000f 
                : 0f;
            _stats.PeakEventsPerFrame = Mathf.Max(_stats.PeakEventsPerFrame, _frameEventCount);
            
            if (showStats && enableLogging && Time.frameCount % 60 == 0)
            {
                LogStats();
            }
        }
        
        private void LogStats()
        {
            Log($"Stats - Events/Frame: {_stats.EventsProcessedThisFrame}, " +
                $"Queued: {_stats.QueuedEvents}, " +
                $"Subscriptions: {_stats.ActiveSubscriptions}, " +
                $"Avg Time: {_stats.AverageProcessingTime:F2}ms");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Create and publish an event of a specific type
        /// </summary>
        public T CreateAndPublishEvent<T>(object source = null) where T : BaseEvent, new()
        {
            var eventData = EventPoolService.Get<T>();
            eventData.Initialize(source);
            PublishEvent(eventData);
            return eventData;
        }
        
        /// <summary>
        /// Create an event without publishing it
        /// </summary>
        public T CreateEvent<T>(object source = null) where T : BaseEvent, new()
        {
            var eventData = EventPoolService.Get<T>();
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
        
        private void Log(string message)
        {
            if (enableLogging)
                Debug.Log($"[EventCenter] {message}");
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
            return EventCenterService.Current?.Subscribe(callback, priority);
        }
        
        #endregion
    }
}