using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// High-performance event dispatcher that routes events to appropriate listeners
    /// Supports priority-based ordering, type-safe dispatch, and struct payloads
    /// </summary>
    public class EventDispatcher
    {
        #region Fields
        
        // Event type to listeners mapping (for struct payloads)
        private readonly Dictionary<Type, List<ListenerEntry>> _structListeners 
            = new Dictionary<Type, List<ListenerEntry>>();
            
        // Legacy event type to listeners mapping (for BaseEvent compatibility)
        private readonly Dictionary<Type, List<ListenerEntry>> _legacyListeners 
            = new Dictionary<Type, List<ListenerEntry>>();
        
        // Cache for fast type lookups
        private readonly Dictionary<Type, Type[]> _typeHierarchyCache 
            = new Dictionary<Type, Type[]>();
        
        // Temporary list for batch dispatch
        private readonly List<ListenerEntry> _tempListeners = new List<ListenerEntry>();
        
        // Statistics
        private int _totalDispatched;
        private int _totalListeners;
        private float _lastDispatchTime;
        
        private readonly bool _enableLogging;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Total number of events dispatched
        /// </summary>
        public int TotalDispatched => _totalDispatched;
        
        /// <summary>
        /// Total number of active listeners
        /// </summary>
        public int TotalListeners => _totalListeners;
        
        /// <summary>
        /// Last dispatch time in milliseconds
        /// </summary>
        public float LastDispatchTime => _lastDispatchTime;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Create a new EventDispatcher
        /// </summary>
        /// <param name="enableLogging">Enable debug logging</param>
        public EventDispatcher(bool enableLogging = false)
        {
            _enableLogging = enableLogging;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Subscribe a listener to struct events of a specific type
        /// </summary>
        /// <typeparam name="T">Type of struct to listen for</typeparam>
        /// <param name="listener">The listener to register</param>
        public void Subscribe<T>(IEventListener<T> listener) where T : struct
        {
            if (listener == null) return;
            
            var eventType = typeof(T);
            if (!_structListeners.TryGetValue(eventType, out var listenerList))
            {
                listenerList = new List<ListenerEntry>();
                _structListeners[eventType] = listenerList;
            }
            
            // Check if already subscribed
            if (listenerList.Any(entry => entry.Listener == listener))
            {
                return;
            }
            
            // Add and sort by priority
            var entry = new ListenerEntry(listener);
            listenerList.Add(entry);
            listenerList.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            
            _totalListeners++;
        }
        
        /// <summary>
        /// Subscribe a listener to events of a specific type (legacy BaseEvent support)
        /// </summary>
        /// <param name="eventType">Type of event to listen for</param>
        /// <param name="listener">The listener to register</param>
        public void Subscribe(Type eventType, IEventListener listener)
        {
            if (eventType == null || listener == null) return;
            
            // Check if it's a struct type
            if (eventType.IsValueType && !eventType.IsEnum && !eventType.IsPrimitive)
            {
                return;
            }
            
            if (!_legacyListeners.TryGetValue(eventType, out var listenerList))
            {
                listenerList = new List<ListenerEntry>();
                _legacyListeners[eventType] = listenerList;
            }
            
            // Check if already subscribed
            if (listenerList.Any(entry => entry.Listener == listener))
            {
                return;
            }
            
            // Add and sort by priority
            var entry = new ListenerEntry(listener);
            listenerList.Add(entry);
            listenerList.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            
            _totalListeners++;
        }
        
        /// <summary>
        /// Unsubscribe a listener from struct events of a specific type
        /// </summary>
        /// <typeparam name="T">Type of struct to unsubscribe from</typeparam>
        /// <param name="listener">The listener to unsubscribe</param>
        public void Unsubscribe<T>(IEventListener<T> listener) where T : struct
        {
            if (listener == null) return;
            
            var eventType = typeof(T);
            if (_structListeners.TryGetValue(eventType, out var listenerList))
            {
                var initialCount = listenerList.Count;
                listenerList.RemoveAll(entry => entry.Listener == listener);
                
                var removedCount = initialCount - listenerList.Count;
                _totalListeners -= removedCount;
                
                if (listenerList.Count == 0)
                {
                    _structListeners.Remove(eventType);
                }
            }
        }
        
        /// <summary>
        /// Unsubscribe a listener from events of a specific type (legacy)
        /// </summary>
        /// <param name="eventType">Type of event to unsubscribe from</param>
        /// <param name="listener">The listener to unsubscribe</param>
        public void Unsubscribe(Type eventType, IEventListener listener)
        {
            if (eventType == null || listener == null) return;
            
            if (_legacyListeners.TryGetValue(eventType, out var listenerList))
            {
                var initialCount = listenerList.Count;
                listenerList.RemoveAll(entry => entry.Listener == listener);
                
                var removedCount = initialCount - listenerList.Count;
                _totalListeners -= removedCount;
                
                if (listenerList.Count == 0)
                {
                    _legacyListeners.Remove(eventType);
                }
            }
        }
        
        /// <summary>
        /// Unsubscribe a listener from all event types
        /// </summary>
        /// <param name="listener">The listener to unsubscribe</param>
        public void UnsubscribeAll(IEventListener listener)
        {
            if (listener == null) return;
            
            var typesToRemove = new List<Type>();
            var totalRemoved = 0;
            
            // Remove from struct listeners
            foreach (var kvp in _structListeners)
            {
                var initialCount = kvp.Value.Count;
                kvp.Value.RemoveAll(entry => entry.Listener == listener);
                
                var removedCount = initialCount - kvp.Value.Count;
                totalRemoved += removedCount;
                
                if (kvp.Value.Count == 0)
                {
                    typesToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var type in typesToRemove)
            {
                _structListeners.Remove(type);
            }
            
            typesToRemove.Clear();
            
            // Remove from legacy listeners
            foreach (var kvp in _legacyListeners)
            {
                var initialCount = kvp.Value.Count;
                kvp.Value.RemoveAll(entry => entry.Listener == listener);
                
                var removedCount = initialCount - kvp.Value.Count;
                totalRemoved += removedCount;
                
                if (kvp.Value.Count == 0)
                {
                    typesToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var type in typesToRemove)
            {
                _legacyListeners.Remove(type);
            }
            
            _totalListeners -= totalRemoved;
        }
        
        /// <summary>
        /// Dispatch a struct event to all registered listeners
        /// </summary>
        /// <typeparam name="T">Type of struct payload</typeparam>
        /// <param name="payload">The struct payload to dispatch</param>
        /// <returns>Number of listeners that processed the event</returns>
        public int Dispatch<T>(T payload) where T : struct
        {
            var startTime = Time.realtimeSinceStartup;
            var eventType = typeof(T);
            var listenersNotified = 0;
            
            _tempListeners.Clear();
            
            // Collect all relevant listeners for this struct type
            if (_structListeners.TryGetValue(eventType, out var listenerList))
            {
                foreach (var entry in listenerList)
                {
                    if (entry.IsActive)
                    {
                        _tempListeners.Add(entry);
                    }
                }
            }
            
            // Sort by priority
            _tempListeners.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            
            // Dispatch to listeners
            foreach (var entry in _tempListeners)
            {
                if (entry.Listener is IEventListener<T> typedListener)
                {
                    if (typedListener.HandleEvent(payload))
                    {
                        listenersNotified++;
                    }
                    listenersNotified++;
                }
            }
            
            _totalDispatched++;
            _lastDispatchTime = (Time.realtimeSinceStartup - startTime) * 1000f;
            
            return listenersNotified;
        }
        
        /// <summary>
        /// Dispatch an event to all registered listeners (legacy BaseEvent support)
        /// </summary>
        /// <param name="eventData">The event to dispatch</param>
        /// <returns>Number of listeners that processed the event</returns>
        public int Dispatch(BaseEvent eventData)
        {
            if (eventData == null || eventData.IsDisposed)
            {
                return 0;
            }
            
            var startTime = Time.realtimeSinceStartup;
            var eventType = eventData.GetType();
            var listenersNotified = 0;
            
            // Get all types in the inheritance hierarchy
            var typeHierarchy = GetTypeHierarchy(eventType);
            
            _tempListeners.Clear();
            
            // Collect all relevant listeners
            foreach (var type in typeHierarchy)
            {
                if (_legacyListeners.TryGetValue(type, out var listenerList))
                {
                    foreach (var entry in listenerList)
                    {
                        if (entry.IsActive && !_tempListeners.Contains(entry))
                        {
                            _tempListeners.Add(entry);
                        }
                    }
                }
            }
            
            // Sort by priority
            _tempListeners.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            
            // Dispatch to listeners
            foreach (var entry in _tempListeners)
            {
                if (entry.Listener.HandleEvent(eventData))
                {
                    listenersNotified++;
                }
                
                // Stop if event is marked as handled and should stop propagation
                if (eventData.IsHandled)
                {
                    break;
                }
            }
            
            _totalDispatched++;
            _lastDispatchTime = (Time.realtimeSinceStartup - startTime) * 1000f;
            
            return listenersNotified;
        }
        
        /// <summary>
        /// Get number of listeners for a specific struct type
        /// </summary>
        /// <typeparam name="T">Type of struct</typeparam>
        /// <returns>Number of listeners</returns>
        public int GetListenerCount<T>() where T : struct
        {
            return _structListeners.TryGetValue(typeof(T), out var list) ? list.Count : 0;
        }
        
        /// <summary>
        /// Get number of listeners for a specific event type (legacy)
        /// </summary>
        /// <param name="eventType">Type of event</param>
        /// <returns>Number of listeners</returns>
        public int GetListenerCount(Type eventType)
        {
            return _legacyListeners.TryGetValue(eventType, out var list) ? list.Count : 0;
        }
        
        /// <summary>
        /// Get all struct types that have listeners
        /// </summary>
        /// <returns>Array of struct types</returns>
        public Type[] GetListenedStructTypes()
        {
            return _structListeners.Keys.ToArray();
        }
        
        /// <summary>
        /// Get all event types that have listeners (legacy)
        /// </summary>
        /// <returns>Array of event types</returns>
        public Type[] GetListenedEventTypes()
        {
            return _legacyListeners.Keys.ToArray();
        }
        
        /// <summary>
        /// Clear all listeners
        /// </summary>
        public void Clear()
        {
            _structListeners.Clear();
            _legacyListeners.Clear();
            _typeHierarchyCache.Clear();
            _totalListeners = 0;
        }
        
        /// <summary>
        /// Get dispatcher statistics
        /// </summary>
        /// <returns>Dispatcher statistics</returns>
        public DispatcherStats GetStats()
        {
            return new DispatcherStats
            {
                TotalDispatched = _totalDispatched,
                TotalListeners = _totalListeners,
                LastDispatchTime = _lastDispatchTime,
                StructEventTypesCount = _structListeners.Count,
                LegacyEventTypesCount = _legacyListeners.Count,
                TypeCacheSize = _typeHierarchyCache.Count
            };
        }
        
        #endregion
        
        #region Private Methods
        
        private Type[] GetTypeHierarchy(Type eventType)
        {
            if (_typeHierarchyCache.TryGetValue(eventType, out var cached))
            {
                return cached;
            }
            
            var types = new List<Type>();
            var currentType = eventType;
            
            // Add the type and all its base types
            while (currentType != null && currentType != typeof(object))
            {
                types.Add(currentType);
                currentType = currentType.BaseType;
            }
            
            // Add interfaces
            types.AddRange(eventType.GetInterfaces());
            
            var result = types.ToArray();
            _typeHierarchyCache[eventType] = result;
            
            return result;
        }
        

        
        /// <summary>
        /// Get listeners for a legacy event type (BaseEvent)
        /// </summary>
        /// <param name="eventType">The event type to get listeners for</param>
        /// <returns>List of listener entries or null if none found</returns>
        public List<ListenerEntry> GetListenersForType(Type eventType)
        {
            if (eventType == null) return null;
            
            if (_legacyListeners.TryGetValue(eventType, out var listeners))
            {
                return new List<ListenerEntry>(listeners);
            }
            
            return null;
        }
        
        /// <summary>
        /// Get listeners for a struct event type
        /// </summary>
        /// <param name="eventType">The struct event type to get listeners for</param>
        /// <returns>List of listener entries or null if none found</returns>
        public List<ListenerEntry> GetStructListenersForType(Type eventType)
        {
            if (eventType == null) return null;
            
            if (_structListeners.TryGetValue(eventType, out var listeners))
            {
                return new List<ListenerEntry>(listeners);
            }
            
            return null;
        }

        #endregion
        
        #region Nested Types
        
        /// <summary>
        /// Internal wrapper for listener entries
        /// </summary>
        public class ListenerEntry
        {
            public IEventListener Listener { get; }
            public int Priority { get; }
            public bool IsActive => Listener?.IsActive ?? false;
            
            public ListenerEntry(IEventListener listener)
            {
                Listener = listener;
                Priority = listener?.Priority ?? 0;
            }
            
            public override bool Equals(object obj)
            {
                return obj is ListenerEntry other && other.Listener == Listener;
            }
            
            public override int GetHashCode()
            {
                return Listener?.GetHashCode() ?? 0;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Statistics for event dispatcher performance
    /// </summary>
    [Serializable]
    public struct DispatcherStats
    {
        /// <summary>
        /// Total events dispatched
        /// </summary>
        public int TotalDispatched;
        
        /// <summary>
        /// Total active listeners
        /// </summary>
        public int TotalListeners;
        
        /// <summary>
        /// Last dispatch time in milliseconds
        /// </summary>
        public float LastDispatchTime;
        
        /// <summary>
        /// Number of different struct types with listeners
        /// </summary>
        public int StructEventTypesCount;
        
        /// <summary>
        /// Number of different legacy event types with listeners
        /// </summary>
        public int LegacyEventTypesCount;
        
        /// <summary>
        /// Size of type hierarchy cache
        /// </summary>
        public int TypeCacheSize;
        
        /// <summary>
        /// Total event types count
        /// </summary>
        public int TotalEventTypesCount => StructEventTypesCount + LegacyEventTypesCount;
        
        /// <summary>
        /// Average listeners per event type
        /// </summary>
        public float AverageListenersPerType => TotalEventTypesCount > 0 ? (float)TotalListeners / TotalEventTypesCount : 0f;
    }
}