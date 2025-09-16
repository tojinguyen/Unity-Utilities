using System;
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Event subscription implementation
    /// </summary>
    internal class EventSubscription : IEventSubscription
    {
        public Type EventType { get; private set; }
        public IEventListener Listener { get; private set; }
        public bool IsActive { get; set; }
        
        private readonly Action _unsubscribeAction;
        private bool _isDisposed;
        
        public EventSubscription(Type eventType, IEventListener listener, Action unsubscribeAction)
        {
            EventType = eventType;
            Listener = listener;
            IsActive = true;
            _unsubscribeAction = unsubscribeAction;
        }
        
        public void Unsubscribe()
        {
            if (_isDisposed) return;
            
            IsActive = false;
            _unsubscribeAction?.Invoke();
        }
        
        public void Dispose()
        {
            if (_isDisposed) return;
            
            Unsubscribe();
            _isDisposed = true;
        }
    }
    
    /// <summary>
    /// Callback listener wrapper for Action-based subscriptions
    /// </summary>
    internal class CallbackEventListener<T> : IEventListener<T> where T : struct
    {
        public int Priority { get; private set; }
        public bool IsActive => _callback != null;
        
        private readonly Action<T> _callback;
        
        public CallbackEventListener(Action<T> callback, int priority = 0)
        {
            _callback = callback;
            Priority = priority;
        }
        
        public bool HandleEvent(BaseEvent eventData)
        {
            // For struct events, we need to unwrap from EventWrapper
            if (eventData is EventWrapper<T> wrapper)
            {
                return HandleEvent(wrapper.Payload);
            }
            return false;
        }
        
        public bool HandleEvent(T eventData)
        {
            try
            {
                _callback?.Invoke(eventData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in event callback: {ex.Message}");
                return false;
            }
        }
    }
    
    /// <summary>
    /// Callback listener wrapper for Action-based legacy subscriptions
    /// </summary>
    internal class CallbackEventLegacyListener<T> : IEventListenerLegacy<T> where T : BaseEvent
    {
        public int Priority { get; private set; }
        public bool IsActive => _callback != null;
        
        private readonly Action<T> _callback;
        
        public CallbackEventLegacyListener(Action<T> callback, int priority = 0)
        {
            _callback = callback;
            Priority = priority;
        }
        
        public bool HandleEvent(BaseEvent eventData)
        {
            if (eventData is T typedEvent)
            {
                return HandleEvent(typedEvent);
            }
            return false;
        }
        
        public bool HandleEvent(T eventData)
        {
            try
            {
                _callback?.Invoke(eventData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in legacy event callback: {ex.Message}");
                return false;
            }
        }
    }
}