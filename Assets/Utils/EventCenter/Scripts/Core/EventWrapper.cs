using System;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Wrapper for struct payloads to provide event metadata
    /// Allows any struct to be used as event payload without inheritance
    /// </summary>
    public partial class EventWrapper<T> : BaseEvent where T : struct
    {
        #region Properties
        
        /// <summary>
        /// The actual event payload (struct)
        /// </summary>
        public T Payload { get; set; }
        
        private int _priority = 0;
        /// <summary>
        /// Priority of this event (higher value = higher priority)
        /// </summary>
        public override int Priority => _priority;
        
        /// <summary>
        /// Set the priority for this event wrapper
        /// </summary>
        public void SetPriority(int priority)
        {
            _priority = priority;
        }
        
        private bool _isImmediate = false;
        /// <summary>
        /// Whether this event should be processed immediately or can be batched
        /// </summary>
        public override bool IsImmediate => _isImmediate;
        
        /// <summary>
        /// Set whether this event is immediate for this event wrapper
        /// </summary>
        public void SetIsImmediate(bool isImmediate)
        {
            _isImmediate = isImmediate;
        }
        
        private bool _isPoolable = true;
        /// <summary>
        /// Whether this event can be pooled for reuse
        /// </summary>
        public override bool IsPoolable => _isPoolable;
        
        /// <summary>
        /// Set whether this event is poolable for this event wrapper
        /// </summary>
        public void SetIsPoolable(bool isPoolable)
        {
            _isPoolable = isPoolable;
        }
        
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
        /// <param name="priority">Event priority</param>
        /// <param name="isImmediate">Whether to process immediately</param>
        public EventWrapper(T payload, int priority = 0, bool isImmediate = false)
        {
            Initialize();
            Payload = payload;
            _priority = priority;
            _isImmediate = isImmediate;
        }
        
        /// <summary>
        /// Initialize event data
        /// Called when event is created or retrieved from pool
        /// </summary>
        public override void Initialize(object source = null)
        {
            base.Initialize(source);
            Payload = default(T);
            _priority = 0;
            _isImmediate = false;
            _isPoolable = true;
        }
        
        /// <summary>
        /// Reset event to default state for pooling
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            Payload = default(T);
            _priority = 0;
            _isImmediate = false;
            _isPoolable = true;
        }
        
        #endregion
        
        #region Handling
        
        /// <summary>
        /// Mark this event as handled
        /// </summary>
        public new void MarkAsHandled()
        {
            IsHandled = true;
        }
        
        /// <summary>
        /// Validate if this event is in a valid state for processing
        /// </summary>
        public override bool IsValid()
        {
            return !IsDisposed && !string.IsNullOrEmpty(EventId);
        }
        
        #endregion
        
        #region IDisposable
        
        /// <summary>
        /// Dispose this event
        /// </summary>
        public new void Dispose()
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
        // Explicit interface implementations for setters
        int IEventWrapper.Priority
        {
            get => Priority;
            set => _priority = value;
        }
        
        bool IEventWrapper.IsImmediate
        {
            get => IsImmediate;
            set => _isImmediate = value;
        }
        
        bool IEventWrapper.IsPoolable
        {
            get => IsPoolable;
            set => _isPoolable = value;
        }
    }
}