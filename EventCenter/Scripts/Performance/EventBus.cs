using System;
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Ultra-fast event bus for critical performance scenarios
    /// Uses direct delegate calls for minimal overhead
    /// </summary>
    public class EventBus
    {
        #region Singleton
        
        private static EventBus _instance;
        public static EventBus Instance => _instance ??= new EventBus();
        
        #endregion
        
        #region Fields
        
        // Direct delegate storage for maximum performance
        private readonly Dictionary<Type, Delegate> _eventHandlers = new Dictionary<Type, Delegate>();
        
        // Fast lookup cache
        private readonly Dictionary<Type, bool> _hasHandlersCache = new Dictionary<Type, bool>();
        
        // Statistics
        private int _totalSubscriptions;
        private int _totalPublishes;
        private long _totalExecutionTime; // in ticks
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Total number of active subscriptions
        /// </summary>
        public int TotalSubscriptions => _totalSubscriptions;
        
        /// <summary>
        /// Total number of events published
        /// </summary>
        public int TotalPublishes => _totalPublishes;
        
        /// <summary>
        /// Average execution time per event in microseconds
        /// </summary>
        public double AverageExecutionTime => _totalPublishes > 0 
            ? (_totalExecutionTime / (double)_totalPublishes) / 10.0 // Convert ticks to microseconds
            : 0.0;
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Subscribe to events with ultra-fast delegate binding
        /// </summary>
        /// <typeparam name="T">Type of event to subscribe to</typeparam>
        /// <param name="handler">Handler delegate</param>
        public void Subscribe<T>(Action<T> handler) where T : BaseEvent
        {
            if (handler == null) return;
            
            var eventType = typeof(T);
            
            if (_eventHandlers.TryGetValue(eventType, out var existingHandler))
            {
                _eventHandlers[eventType] = Delegate.Combine(existingHandler, handler);
            }
            else
            {
                _eventHandlers[eventType] = handler;
            }
            
            _hasHandlersCache[eventType] = true;
            _totalSubscriptions++;
        }
        
        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        /// <typeparam name="T">Type of event to unsubscribe from</typeparam>
        /// <param name="handler">Handler delegate to remove</param>
        public void Unsubscribe<T>(Action<T> handler) where T : BaseEvent
        {
            if (handler == null) return;
            
            var eventType = typeof(T);
            
            if (_eventHandlers.TryGetValue(eventType, out var existingHandler))
            {
                var newHandler = Delegate.Remove(existingHandler, handler);
                
                if (newHandler == null)
                {
                    _eventHandlers.Remove(eventType);
                    _hasHandlersCache[eventType] = false;
                }
                else
                {
                    _eventHandlers[eventType] = newHandler;
                }
                
                _totalSubscriptions--;
            }
        }
        
        /// <summary>
        /// Publish an event with minimal overhead
        /// </summary>
        /// <typeparam name="T">Type of event to publish</typeparam>
        /// <param name="eventData">Event data to publish</param>
        public void Publish<T>(T eventData) where T : BaseEvent
        {
            if (eventData == null) return;
            
            var eventType = typeof(T);
            
            // Fast path: check cache first
            if (!_hasHandlersCache.TryGetValue(eventType, out var hasHandlers) || !hasHandlers)
            {
                return;
            }
            
            if (_eventHandlers.TryGetValue(eventType, out var handler))
            {
                var startTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                
                try
                {
                    (handler as Action<T>)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in EventBus handler for {eventType.Name}: {ex.Message}");
                }
                finally
                {
                    var endTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                    _totalExecutionTime += endTicks - startTicks;
                    _totalPublishes++;
                }
            }
        }
        
        /// <summary>
        /// Check if there are any handlers for an event type
        /// </summary>
        /// <typeparam name="T">Type of event to check</typeparam>
        /// <returns>True if handlers exist</returns>
        public bool HasHandlers<T>() where T : BaseEvent
        {
            var eventType = typeof(T);
            return _hasHandlersCache.TryGetValue(eventType, out var hasHandlers) && hasHandlers;
        }
        
        /// <summary>
        /// Get number of handlers for a specific event type
        /// </summary>
        /// <typeparam name="T">Type of event</typeparam>
        /// <returns>Number of handlers</returns>
        public int GetHandlerCount<T>() where T : BaseEvent
        {
            var eventType = typeof(T);
            
            if (_eventHandlers.TryGetValue(eventType, out var handler))
            {
                return handler.GetInvocationList().Length;
            }
            
            return 0;
        }
        
        /// <summary>
        /// Clear all event handlers
        /// </summary>
        public void Clear()
        {
            _eventHandlers.Clear();
            _hasHandlersCache.Clear();
            _totalSubscriptions = 0;
        }
        
        /// <summary>
        /// Get performance statistics
        /// </summary>
        /// <returns>EventBus performance stats</returns>
        public EventBusStats GetStats()
        {
            return new EventBusStats
            {
                TotalSubscriptions = _totalSubscriptions,
                TotalPublishes = _totalPublishes,
                AverageExecutionTime = AverageExecutionTime,
                EventTypesWithHandlers = _eventHandlers.Count,
                CacheSize = _hasHandlersCache.Count
            };
        }
        
        #endregion
        
        #region Static Convenience Methods
        
        /// <summary>
        /// Static convenience method to subscribe
        /// </summary>
        public static void Listen<T>(Action<T> handler) where T : BaseEvent
        {
            Instance.Subscribe(handler);
        }
        
        /// <summary>
        /// Static convenience method to unsubscribe
        /// </summary>
        public static void Unlisten<T>(Action<T> handler) where T : BaseEvent
        {
            Instance.Unsubscribe(handler);
        }
        
        /// <summary>
        /// Static convenience method to publish
        /// </summary>
        public static void Send<T>(T eventData) where T : BaseEvent
        {
            Instance.Publish(eventData);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Performance statistics for EventBus
    /// </summary>
    [Serializable]
    public struct EventBusStats
    {
        /// <summary>
        /// Total number of active subscriptions
        /// </summary>
        public int TotalSubscriptions;
        
        /// <summary>
        /// Total number of events published
        /// </summary>
        public int TotalPublishes;
        
        /// <summary>
        /// Average execution time per event in microseconds
        /// </summary>
        public double AverageExecutionTime;
        
        /// <summary>
        /// Number of event types with active handlers
        /// </summary>
        public int EventTypesWithHandlers;
        
        /// <summary>
        /// Size of the handler cache
        /// </summary>
        public int CacheSize;
    }
    
    /// <summary>
    /// Fast event for EventBus - minimal overhead event class
    /// </summary>
    public abstract class FastEvent : BaseEvent
    {
        /// <summary>
        /// Fast events are not pooled by default for maximum performance
        /// </summary>
        public override bool IsPoolable => false;
        
        /// <summary>
        /// Fast events have immediate priority
        /// </summary>
        public override bool IsImmediate => true;
        
        /// <summary>
        /// Fast events skip most validation for performance
        /// </summary>
        public override bool IsValid() => true;
        
        /// <summary>
        /// Minimal reset implementation
        /// </summary>
        public override void Reset()
        {
            // Minimal reset for performance
            IsHandled = false;
            IsDisposed = false;
        }
    }
}