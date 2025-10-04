using System;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Base class for all events in the Event Center system
    /// Provides common functionality and performance optimizations
    /// </summary>
    public abstract class BaseEvent : IDisposable
    {
        #region Properties
        
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
        public virtual int Priority => 0;
        
        /// <summary>
        /// Whether this event can be pooled for reuse
        /// </summary>
        public virtual bool IsPoolable => true;
        
        /// <summary>
        /// Whether this event should be processed immediately or can be batched
        /// </summary>
        public virtual bool IsImmediate => false;
        
        /// <summary>
        /// Source object that raised this event
        /// </summary>
        public object Source { get; protected set; }
        
        /// <summary>
        /// Whether this event has been handled
        /// </summary>
        public bool IsHandled { get; protected set; }
        
        /// <summary>
        /// Whether this event is disposed
        /// </summary>
        public bool IsDisposed { get; protected set; }
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// Constructor for BaseEvent
        /// </summary>
        protected BaseEvent()
        {
            Initialize();
        }
        
        /// <summary>
        /// Initialize event data
        /// Called when event is created or retrieved from pool
        /// </summary>
        public virtual void Initialize(object source = null)
        {
            EventId = Guid.NewGuid().ToString();
            TimeStamp = Time.unscaledTime;
            Source = source;
            IsHandled = false;
            IsDisposed = false;
            
            OnInitialize();
        }
        
        /// <summary>
        /// Override this to perform custom initialization
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// Reset event to default state for pooling
        /// </summary>
        public virtual void Reset()
        {
            EventId = null;
            TimeStamp = 0f;
            Source = null;
            IsHandled = false;
            IsDisposed = false;
            
            OnReset();
        }
        
        /// <summary>
        /// Override this to perform custom reset logic
        /// </summary>
        protected virtual void OnReset() { }
        
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
        public virtual bool IsValid()
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
            
            OnDispose();
            
            if (IsPoolable)
            {
                EventPoolService.Return(this);
            }
            
            IsDisposed = true;
        }
        
        /// <summary>
        /// Override this to perform custom disposal logic
        /// </summary>
        protected virtual void OnDispose() { }
        
        #endregion
        
        #region ConsoleLoggerging
        
        public override string ToString()
        {
            return $"[{GetType().Name}] ID: {EventId}, Time: {TimeStamp:F2}, Priority: {Priority}, Source: {Source?.GetType().Name ?? "null"}";
        }
        
        #endregion
    }
}