using System;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Interface for objects that can listen to events
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        /// Priority of this listener (higher value = higher priority)
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Whether this listener is active and should receive events
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Handle an incoming event
        /// </summary>
        /// <param name="eventData">The event to handle</param>
        /// <returns>True if event was handled, false otherwise</returns>
        bool HandleEvent(BaseEvent eventData);
    }
    
    /// <summary>
    /// Generic interface for type-safe event listening with struct payloads
    /// </summary>
    /// <typeparam name="T">Type of struct payload to listen for</typeparam>
    public interface IEventListener<in T> : IEventListener where T : struct
    {
        /// <summary>
        /// Handle a typed struct event
        /// </summary>
        /// <param name="eventData">The struct payload</param>
        /// <returns>True if event was handled, false otherwise</returns>
        bool HandleEvent(T eventData);
    }
    
    /// <summary>
    /// Legacy interface for backward compatibility with BaseEvent
    /// </summary>
    /// <typeparam name="T">Type of BaseEvent to listen for</typeparam>
    public interface IEventListenerLegacy<in T> : IEventListener where T : BaseEvent
    {
        /// <summary>
        /// Handle a typed BaseEvent
        /// </summary>
        /// <param name="eventData">The typed event to handle</param>
        /// <returns>True if event was handled, false otherwise</returns>
        bool HandleEvent(T eventData);
    }
    
    /// <summary>
    /// Interface for objects that can publish events
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publish a struct event to the event system
        /// </summary>
        /// <typeparam name="T">Type of struct payload</typeparam>
        /// <param name="payload">The struct payload</param>
        /// <param name="priority">Event priority (higher value = higher priority)</param>
        void PublishEvent<T>(T payload, int priority = 0) where T : struct;
        
        /// <summary>
        /// Publish a struct event with immediate processing
        /// </summary>
        /// <typeparam name="T">Type of struct payload</typeparam>
        /// <param name="payload">The struct payload to publish immediately</param>
        /// <param name="priority">Event priority (higher value = higher priority)</param>
        void PublishEventImmediate<T>(T payload, int priority = 0) where T : struct;
        
        /// <summary>
        /// Legacy method for publishing BaseEvent objects
        /// </summary>
        /// <param name="eventData">The event to publish</param>
        void PublishEvent(BaseEvent eventData);
        
        /// <summary>
        /// Legacy method for publishing BaseEvent objects with immediate processing
        /// </summary>
        /// <param name="eventData">The event to publish immediately</param>
        void PublishEventImmediate(BaseEvent eventData);
    }
    
    /// <summary>
    /// Interface for event subscription management
    /// </summary>
    public interface IEventSubscription : IDisposable
    {
        /// <summary>
        /// Type of event this subscription is for
        /// </summary>
        Type EventType { get; }
        
        /// <summary>
        /// The listener for this subscription
        /// </summary>
        IEventListener Listener { get; }
        
        /// <summary>
        /// Whether this subscription is active
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        void Unsubscribe();
    }
    
    /// <summary>
    /// Interface for the main event center
    /// </summary>
    public interface IEventCenter : IEventPublisher
    {
        /// <summary>
        /// Subscribe to struct events of a specific type
        /// </summary>
        /// <typeparam name="T">Type of struct to subscribe to</typeparam>
        /// <param name="listener">The listener to register</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        IEventSubscription Subscribe<T>(IEventListener<T> listener) where T : struct;
        
        /// <summary>
        /// Subscribe to struct events with a callback function
        /// </summary>
        /// <typeparam name="T">Type of struct to subscribe to</typeparam>
        /// <param name="callback">Callback function to invoke</param>
        /// <param name="priority">Priority of this callback</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        IEventSubscription Subscribe<T>(Action<T> callback, int priority = 0) where T : struct;
        
        /// <summary>
        /// Legacy method for subscribing to BaseEvent types
        /// </summary>
        /// <typeparam name="T">Type of BaseEvent to subscribe to</typeparam>
        /// <param name="listener">The listener to register</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        IEventSubscription SubscribeLegacy<T>(IEventListenerLegacy<T> listener) where T : BaseEvent;
        
        /// <summary>
        /// Legacy method for subscribing to BaseEvent types with callback
        /// </summary>
        /// <typeparam name="T">Type of BaseEvent to subscribe to</typeparam>
        /// <param name="callback">Callback function to invoke</param>
        /// <param name="priority">Priority of this callback</param>
        /// <returns>Subscription handle for managing the subscription</returns>
        IEventSubscription SubscribeLegacy<T>(Action<T> callback, int priority = 0) where T : BaseEvent;
        
        /// <summary>
        /// Unsubscribe a listener from events
        /// </summary>
        /// <param name="listener">The listener to unsubscribe</param>
        void Unsubscribe(IEventListener listener);
        
        /// <summary>
        /// Unsubscribe from a specific struct type
        /// </summary>
        /// <typeparam name="T">Type of struct to unsubscribe from</typeparam>
        /// <param name="listener">The listener to unsubscribe</param>
        void Unsubscribe<T>(IEventListener<T> listener) where T : struct;
        
        /// <summary>
        /// Process all queued events
        /// </summary>
        void ProcessEvents();
        
        /// <summary>
        /// Clear all events and subscriptions
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Get statistics about the event system
        /// </summary>
        EventCenterStats GetStats();
        
        /// <summary>
        /// Check if logging is enabled for this EventCenter
        /// </summary>
        bool IsLoggingEnabled { get; }
    }
    
    /// <summary>
    /// Statistics about the event center performance
    /// </summary>
    public struct EventCenterStats
    {
        /// <summary>
        /// Number of events processed this frame
        /// </summary>
        public int EventsProcessedThisFrame;
        
        /// <summary>
        /// Number of events currently queued
        /// </summary>
        public int QueuedEvents;
        
        /// <summary>
        /// Total number of active subscriptions
        /// </summary>
        public int ActiveSubscriptions;
        
        /// <summary>
        /// Number of events pooled for reuse
        /// </summary>
        public int PooledEvents;
        
        /// <summary>
        /// Average processing time per event (in milliseconds)
        /// </summary>
        public float AverageProcessingTime;
        
        /// <summary>
        /// Peak number of events processed in a single frame
        /// </summary>
        public int PeakEventsPerFrame;
        
        /// <summary>
        /// Total memory allocated for events (in bytes)
        /// </summary>
        public long MemoryUsage;
    }
}