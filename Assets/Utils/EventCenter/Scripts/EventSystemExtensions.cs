using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Extension methods for automatic event unsubscription when GameObject is destroyed
    /// Phương thức mở rộng để tự động hủy đăng ký event khi GameObject bị hủy
    /// </summary>
    public static class EventSystemExtensions
    {
        /// <summary>
        /// Subscribe to an event and automatically unsubscribe when the MonoBehaviour is destroyed
        /// Đăng ký event và tự động hủy đăng ký khi MonoBehaviour bị hủy
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="component">MonoBehaviour that will be monitored for destruction</param>
        /// <param name="callback">Event callback</param>
        public static void SubscribeWithCleanup<T>(this MonoBehaviour component, Action<T> callback) 
            where T : struct
        {
            Debug.Log($"[EventSystemExtensions] SubscribeWithCleanup called for type {typeof(T).Name}");
            
            if (component == null)
            {
                Debug.LogError("[EventSystem] Component is null, cannot subscribe with cleanup");
                return;
            }

            if (callback == null)
            {
                Debug.LogError("[EventSystem] Callback is null, cannot subscribe");
                return;
            }

            try
            {
                Debug.Log($"[EventSystemExtensions] Attempting to subscribe to {typeof(T).Name}...");
                
                // Subscribe to the event and store the subscription
                var subscription = EventSystem.Subscribe<T>(callback);
                
                if (subscription != null)
                {
                    Debug.Log($"[EventSystemExtensions] ✅ Successfully subscribed to {typeof(T).Name}");
                    
                    // Register for auto-cleanup using CancellationTokenOnDestroy
                    component.GetCancellationTokenOnDestroy().Register(() => 
                    {
                        Debug.Log($"[EventSystemExtensions] Cleaning up subscription for {typeof(T).Name}");
                        subscription?.Dispose();
                    });
                    
                    Debug.Log($"[EventSystemExtensions] ✅ Auto-cleanup registered for {typeof(T).Name}");
                }
                else
                {
                    Debug.LogError($"[EventSystemExtensions] ❌ Subscription returned null for {typeof(T).Name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventSystemExtensions] ❌ Exception in SubscribeWithCleanup for {typeof(T).Name}: {ex.Message}");
                Debug.LogError($"[EventSystemExtensions] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Subscribe to an event with condition and automatically unsubscribe when destroyed
        /// Đăng ký event có điều kiện và tự động hủy đăng ký khi bị hủy
        /// </summary>
        public static void SubscribeWhenWithCleanup<T>(this MonoBehaviour component, Action<T> callback, Func<T, bool> condition) 
            where T : struct
        {
            if (component == null || callback == null || condition == null)
            {
                Debug.LogError("[EventSystem] Invalid parameters for SubscribeWhenWithCleanup");
                return;
            }

            var subscription = EventSystem.SubscribeWhen<T>(callback, condition);

            component.GetCancellationTokenOnDestroy().Register(() => 
            {
                subscription?.Dispose();
            });
        }

        /// <summary>
        /// Subscribe once and automatically handle cleanup (though SubscribeOnce already cleans itself)
        /// Đăng ký một lần và tự động xử lý cleanup (mặc dù SubscribeOnce đã tự cleanup)
        /// </summary>
        public static void SubscribeOnceWithCleanup<T>(this MonoBehaviour component, Action<T> callback) 
            where T : struct
        {
            if (component == null || callback == null)
            {
                Debug.LogError("[EventSystem] Invalid parameters for SubscribeOnceWithCleanup");
                return;
            }

            var subscription = EventSystem.SubscribeOnce<T>(callback);
            
            // SubscribeOnce tự động cleanup sau lần đầu, nhưng chúng ta vẫn register
            // để đảm bảo cleanup nếu component bị destroy trước khi event được trigger
            component.GetCancellationTokenOnDestroy().Register(() => 
            {
                subscription?.Dispose();
            });
        }

        /// <summary>
        /// Subscribe once with condition and automatic cleanup
        /// Đăng ký một lần có điều kiện và tự động cleanup
        /// </summary>
        public static void SubscribeOnceWithCleanup<T>(this MonoBehaviour component, Action<T> callback, Func<T, bool> condition) 
            where T : struct
        {
            if (component == null || callback == null || condition == null)
            {
                Debug.LogError("[EventSystem] Invalid parameters for SubscribeOnceWithCleanup");
                return;
            }

            IEventSubscription subscription = null;
            subscription = EventSystem.Subscribe<T>((payload) =>
            {
                if (condition(payload))
                {
                    callback(payload);
                    subscription?.Dispose();
                }
            });

            component.GetCancellationTokenOnDestroy().Register(() => 
            {
                subscription?.Dispose();
            });
        }
    }
}