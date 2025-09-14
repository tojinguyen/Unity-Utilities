using System;
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.Data
{
    public class DataEventArgs<T> : EventArgs where T : class
    {
        public T Data { get; }
        public string Key { get; }
        public DateTime Timestamp { get; }
        public string Source { get; }

        public DataEventArgs(T data, string key, string source = null)
        {
            Data = data;
            Key = key;
            Timestamp = DateTime.UtcNow;
            Source = source ?? "Unknown";
        }
    }

    public class DataDeletedEventArgs : EventArgs
    {
        public string Key { get; }
        public Type DataType { get; }
        public DateTime Timestamp { get; }
        public string Source { get; }

        public DataDeletedEventArgs(string key, Type dataType, string source = null)
        {
            Key = key;
            DataType = dataType;
            Timestamp = DateTime.UtcNow;
            Source = source ?? "Unknown";
        }
    }

    public class DataErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public string Key { get; }
        public Type DataType { get; }
        public string Operation { get; }
        public DateTime Timestamp { get; }

        public DataErrorEventArgs(Exception exception, string key, Type dataType, string operation)
        {
            Exception = exception;
            Key = key;
            DataType = dataType;
            Operation = operation;
            Timestamp = DateTime.UtcNow;
        }
    }

    internal class DataEventSubscription<T> where T : class
    {
        public Action<T> OnSaved { get; set; }
        public Action<T> OnLoaded { get; set; }
        public Action<string> OnDeleted { get; set; }
        public Action<Exception> OnError { get; set; }

        public bool HasSubscriptions => OnSaved != null || OnLoaded != null || OnDeleted != null || OnError != null;
    }

    public class DataEventManager
    {
        private readonly Dictionary<Type, object> _subscriptions = new();
        private readonly object _lock = new object();

        public event EventHandler<DataEventArgs<object>> OnAnyDataSaved;
        public event EventHandler<DataEventArgs<object>> OnAnyDataLoaded;
        public event EventHandler<DataDeletedEventArgs> OnAnyDataDeleted;
        public event EventHandler<DataErrorEventArgs> OnAnyDataError;

        public void Subscribe<T>(
            Action<T> onSaved = null,
            Action<T> onLoaded = null,
            Action<string> onDeleted = null,
            Action<Exception> onError = null) where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                
                if (!_subscriptions.TryGetValue(type, out var subscription))
                {
                    subscription = new DataEventSubscription<T>();
                    _subscriptions[type] = subscription;
                }

                var typedSubscription = (DataEventSubscription<T>)subscription;
                
                if (onSaved != null)
                    typedSubscription.OnSaved += onSaved;
                if (onLoaded != null)
                    typedSubscription.OnLoaded += onLoaded;
                if (onDeleted != null)
                    typedSubscription.OnDeleted += onDeleted;
                if (onError != null)
                    typedSubscription.OnError += onError;

#if DATA_LOG
                Debug.Log($"[DataEventManager] Subscribed to events for type: {type.Name}");
#endif
            }
        }

        public void Unsubscribe<T>(
            Action<T> onSaved = null,
            Action<T> onLoaded = null,
            Action<string> onDeleted = null,
            Action<Exception> onError = null) where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                
                if (_subscriptions.TryGetValue(type, out var subscription))
                {
                    var typedSubscription = (DataEventSubscription<T>)subscription;
                    
                    if (onSaved != null)
                        typedSubscription.OnSaved -= onSaved;
                    if (onLoaded != null)
                        typedSubscription.OnLoaded -= onLoaded;
                    if (onDeleted != null)
                        typedSubscription.OnDeleted -= onDeleted;
                    if (onError != null)
                        typedSubscription.OnError -= onError;

                    // Remove subscription if no handlers remain
                    if (!typedSubscription.HasSubscriptions)
                    {
                        _subscriptions.Remove(type);
                    }

#if DATA_LOG
                    Debug.Log($"[DataEventManager] Unsubscribed from events for type: {type.Name}");
#endif
                }
            }
        }

        public void Unsubscribe<T>() where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_subscriptions.Remove(type))
                {
#if DATA_LOG
                    Debug.Log($"[DataEventManager] Cleared all subscriptions for type: {type.Name}");
#endif
                }
            }
        }

        public void RaiseDataSaved<T>(Type dataType, T data, string key = null) where T : class
        {
            if (data == null) return;

            try
            {
                key ??= dataType.Name;

                // Raise type-specific event
                lock (_lock)
                {
                    if (_subscriptions.TryGetValue(dataType, out var subscription))
                    {
                        var typedSubscription = (DataEventSubscription<T>)subscription;
                        typedSubscription.OnSaved?.Invoke(data);
                    }
                }

                // Raise global event
                OnAnyDataSaved?.Invoke(this, new DataEventArgs<object>(data, key, "DataEventManager"));

#if DATA_LOG
                Debug.Log($"[DataEventManager] Data saved event raised for: {key}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataEventManager] Error raising data saved event: {ex.Message}");
            }
        }

        public void RaiseDataLoaded<T>(Type dataType, T data, string key = null) where T : class
        {
            if (data == null) return;

            try
            {
                key ??= dataType.Name;

                // Raise type-specific event
                lock (_lock)
                {
                    if (_subscriptions.TryGetValue(dataType, out var subscription))
                    {
                        var typedSubscription = (DataEventSubscription<T>)subscription;
                        typedSubscription.OnLoaded?.Invoke(data);
                    }
                }

                // Raise global event
                OnAnyDataLoaded?.Invoke(this, new DataEventArgs<object>(data, key, "DataEventManager"));

#if DATA_LOG
                Debug.Log($"[DataEventManager] Data loaded event raised for: {key}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataEventManager] Error raising data loaded event: {ex.Message}");
            }
        }

        public void RaiseDataDeleted(Type dataType, string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            try
            {
                // Raise type-specific event
                lock (_lock)
                {
                    if (_subscriptions.TryGetValue(dataType, out var subscription))
                    {
                        // Use reflection to call the appropriate OnDeleted action
                        var subscriptionType = subscription.GetType();
                        var onDeletedProperty = subscriptionType.GetProperty("OnDeleted");
                        var onDeletedAction = onDeletedProperty?.GetValue(subscription) as Action<string>;
                        onDeletedAction?.Invoke(key);
                    }
                }

                // Raise global event
                OnAnyDataDeleted?.Invoke(this, new DataDeletedEventArgs(key, dataType, "DataEventManager"));

#if DATA_LOG
                Debug.Log($"[DataEventManager] Data deleted event raised for: {key}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataEventManager] Error raising data deleted event: {ex.Message}");
            }
        }

        public void RaiseDataError(Type dataType, Exception exception, string key = null, string operation = "Unknown")
        {
            if (exception == null) return;

            try
            {
                key ??= dataType?.Name ?? "Unknown";

                // Raise type-specific event
                if (dataType != null)
                {
                    lock (_lock)
                    {
                        if (_subscriptions.TryGetValue(dataType, out var subscription))
                        {
                            // Use reflection to call the appropriate OnError action
                            var subscriptionType = subscription.GetType();
                            var onErrorProperty = subscriptionType.GetProperty("OnError");
                            var onErrorAction = onErrorProperty?.GetValue(subscription) as Action<Exception>;
                            onErrorAction?.Invoke(exception);
                        }
                    }
                }

                // Raise global event
                OnAnyDataError?.Invoke(this, new DataErrorEventArgs(exception, key, dataType, operation));

#if DATA_LOG
                Debug.LogError($"[DataEventManager] Data error event raised for: {key} - {exception.Message}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataEventManager] Error raising data error event: {ex.Message}");
            }
        }

        public int GetSubscriptionCount<T>() where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_subscriptions.TryGetValue(type, out var subscription))
                {
                    var typedSubscription = (DataEventSubscription<T>)subscription;
                    int count = 0;
                    if (typedSubscription.OnSaved != null) count++;
                    if (typedSubscription.OnLoaded != null) count++;
                    if (typedSubscription.OnDeleted != null) count++;
                    if (typedSubscription.OnError != null) count++;
                    return count;
                }
                return 0;
            }
        }

        public int GetTotalSubscriptionCount()
        {
            lock (_lock)
            {
                return _subscriptions.Count;
            }
        }

        public IEnumerable<Type> GetSubscribedTypes()
        {
            lock (_lock)
            {
                return new List<Type>(_subscriptions.Keys);
            }
        }

        public void ClearAll()
        {
            lock (_lock)
            {
                _subscriptions.Clear();
                
                // Clear global events
                OnAnyDataSaved = null;
                OnAnyDataLoaded = null;
                OnAnyDataDeleted = null;
                OnAnyDataError = null;

#if DATA_LOG
                Debug.Log("[DataEventManager] All subscriptions cleared");
#endif
            }
        }

        public bool HasSubscriptions<T>() where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                return _subscriptions.ContainsKey(type);
            }
        }

        public bool HasGlobalSubscriptions()
        {
            return OnAnyDataSaved != null || OnAnyDataLoaded != null || 
                   OnAnyDataDeleted != null || OnAnyDataError != null;
        }
    }
}
