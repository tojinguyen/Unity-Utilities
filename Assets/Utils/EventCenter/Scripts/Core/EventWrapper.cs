using System;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Wrapper for struct payloads to provide event metadata
    /// Allows any struct to be used as event payload without inheritance
    /// </summary>
    public class EventWrapper<T> : IDisposable where T : struct
    {
        #region Properties
        
        /// <summary>
        /// The actual event payload (struct)
        /// </summary>
        public T Payload { get; set; }
        
        /// <summary>
        /// Unique identifier for this event instance
        /// </summary>
        public string EventId { get; private set; }
        
        /// <summary>
        /// Timestamp when this event was created
        /// </summary>
        public float TimeStamp { get; private set; }
        
        /// <summary>
        /// Priority of this event (higher value = higher priority)
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Whether this event should be processed immediately or can be batched
        /// </summary>
        public bool IsImmediate { get; set; }
        
        /// <summary>
        /// Source object that raised this event
        /// </summary>
        public object Source { get; private set; }
        
        /// <summary>
        /// Whether this event has been handled
        /// </summary>
        public bool IsHandled { get; private set; }
        
        /// <summary>
        /// Whether this event is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        
        /// <summary>
        /// Whether this event can be pooled for reuse
        /// </summary>
        public bool IsPoolable { get; set; } = true;
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// Constructor for EventWrapper
        /// </summary>
        public EventWrapper()
        {
            Initialize();
        }
        
        /// <summary>
        /// Constructor with payload
        /// </summary>
        /// <param name="payload">The struct payload</param>
        /// <param name="source">Source object</param>
        /// <param name="priority">Event priority</param>
        /// <param name="isImmediate">Whether to process immediately</param>
        public EventWrapper(T payload, object source = null, int priority = 0, bool isImmediate = false)
        {
            Initialize(source);
            Payload = payload;
            Priority = priority;
            IsImmediate = isImmediate;
        }
        
        /// <summary>
        /// Initialize event data
        /// Called when event is created or retrieved from pool
        /// </summary>
        public void Initialize(object source = null)
        {
            EventId = Guid.NewGuid().ToString();
            TimeStamp = Time.unscaledTime;
            Source = source;
            IsHandled = false;
            IsDisposed = false;
            Priority = 0;
            IsImmediate = false;
            IsPoolable = true;
            Payload = default(T);
        }
        
        /// <summary>
        /// Reset event to default state for pooling
        /// </summary>
        public void Reset()
        {
            EventId = null;
            TimeStamp = 0f;
            Source = null;
            IsHandled = false;
            IsDisposed = false;
            Priority = 0;
            IsImmediate = false;
            IsPoolable = true;
            Payload = default(T);
        }
        
        #endregion
        
        #region Handling
        
        /// <summary>
        /// Mark this event as handled
        /// </summary>
        public void MarkAsHandled()
        {
            IsHandled = true;
        }
        
        /// <summary>
        /// Validate if this event is in a valid state for processing
        /// </summary>
        public bool IsValid()
        {
            return !IsDisposed && !string.IsNullOrEmpty(EventId);
        }
        
        #endregion
        
        #region IDisposable
        
        /// <summary>
        /// Dispose this event
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            
            if (IsPoolable)
            {
                EventWrapperPool<T>.Return(this);
            }
            
            IsDisposed = true;
        }
        
        #endregion
        
        #region Debugging
        
        public override string ToString()
        {
            return $"[EventWrapper<{typeof(T).Name}>] ID: {EventId}, Time: {TimeStamp:F2}, Priority: {Priority}, Source: {Source?.GetType().Name ?? "null"}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Non-generic base interface for EventWrapper
    /// </summary>
    public interface IEventWrapper
    {
        string EventId { get; }
        float TimeStamp { get; }
        int Priority { get; set; }
        bool IsImmediate { get; set; }
        object Source { get; }
        bool IsHandled { get; }
        bool IsDisposed { get; }
        bool IsPoolable { get; set; }
        
        void MarkAsHandled();
        bool IsValid();
        void Dispose();
    }
    
    /// <summary>
    /// Make EventWrapper implement the interface
    /// </summary>
    public partial class EventWrapper<T> : IEventWrapper where T : struct
    {
        // Interface implementation is already covered by the properties above
    }
}