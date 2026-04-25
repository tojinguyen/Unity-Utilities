using System;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Static entry point for the Event System.
    /// 
    /// PRIMARY API — recommended for all new code:
    /// <code>
    ///   // Publish
    ///   EventSystem.Publish(new PlayerDeadEvent());
    ///
    ///   // Subscribe (manual unsubscribe)
    ///   var sub = EventSystem.Subscribe&lt;PlayerDeadEvent&gt;(OnPlayerDead);
    ///   sub.Dispose(); // when done
    ///
    ///   // Subscribe (auto-cleanup with MonoBehaviour)
    ///   this.Subscribe&lt;PlayerDeadEvent&gt;(OnPlayerDead);
    ///
    ///   // Subscribe once
    ///   EventSystem.SubscribeOnce&lt;PlayerDeadEvent&gt;(OnPlayerDead);
    ///
    ///   // Subscribe with filter
    ///   EventSystem.SubscribeWhen&lt;PlayerDeadEvent&gt;(OnPlayerDead, e => e.IsLocalPlayer);
    /// </code>
    ///
    /// LEGACY API (backward compat — BaseEvent class):
    /// <code>
    ///   EventSystem.PublishLegacy(myBaseEvent);
    ///   EventSystem.SubscribeLegacy&lt;MyEvent&gt;(callback);
    /// </code>
    /// </summary>
    public static class EventSystem
    {
        #region Internal State

        private static IEventCenter _eventCenter;
        private static bool _isInitialized;

        #endregion

        #region Initialization

        /// <summary>
        /// Called automatically by EventCenter MonoBehaviour on Awake.
        /// </summary>
        /// <summary>
        /// Initialize by auto-discovering the EventCenter from EventCenterService.
        /// Called automatically by EventCenter MonoBehaviour — but can also be called manually.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized && _eventCenter != null) return;
            EnsureLegacyInitialized();
        }

        /// <summary>
        /// Initialize with an explicit IEventCenter instance.
        /// Called internally by EventCenter MonoBehaviour on Awake.
        /// </summary>
        internal static void Initialize(IEventCenter center)
        {
            _eventCenter     = center;
            _isInitialized   = true;
        }

        /// <summary>
        /// True if the system is ready to receive publish/subscribe calls.
        /// </summary>
        public static bool IsInitialized => _isInitialized && _eventCenter != null;

        internal static void Shutdown()
        {
            _eventCenter    = null;
            _isInitialized  = false;
        }

        #endregion

        #region Struct Event API  ──  PRIMARY (zero-allocation)

        /// <summary>
        /// Publish a struct event to all subscribers.
        /// Zero GC allocation for the dispatch itself.
        /// </summary>
        public static void Publish<T>(T payload) where T : struct
        {
#if UNITY_EDITOR
            EditorBridge.NotifyPublish(payload);
#endif
            EventHub<T>.Publish(payload);
        }

        /// <summary>
        /// Subscribe to a struct event type.
        /// Returns an IDisposable subscription token — call Dispose() to unsubscribe.
        /// </summary>
        public static IEventSubscription Subscribe<T>(Action<T> callback, int priority = 0)
            where T : struct
        {
            if (callback == null)
            {
                ConsoleLogger.LogError("[EventSystem] Subscribe: callback is null.");
                return NullSubscription.Instance;
            }
            return EventHub<T>.Subscribe(callback, priority);
        }

        /// <summary>
        /// Subscribe to a struct event that fires only once, then auto-unsubscribes.
        /// </summary>
        public static IEventSubscription SubscribeOnce<T>(Action<T> callback, int priority = 0)
            where T : struct
        {
            if (callback == null) return NullSubscription.Instance;

            IEventSubscription sub = null;
            sub = Subscribe<T>(payload =>
            {
                callback(payload);
                sub?.Dispose();
            }, priority);
            return sub;
        }

        /// <summary>
        /// Subscribe to a struct event with a filter predicate.
        /// The callback is only invoked when <paramref name="condition"/> returns true.
        /// </summary>
        public static IEventSubscription SubscribeWhen<T>(
            Action<T> callback,
            Func<T, bool> condition,
            int priority = 0)
            where T : struct
        {
            if (callback == null || condition == null) return NullSubscription.Instance;

            return Subscribe<T>(payload =>
            {
                if (condition(payload)) callback(payload);
            }, priority);
        }

        /// <summary>
        /// Unsubscribe a specific callback from a struct event type.
        /// Prefer holding and Disposing the IEventSubscription returned by Subscribe.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> callback) where T : struct
        {
            EventHub<T>.Unsubscribe(callback);
        }

        /// <summary>
        /// Number of active subscribers for a given struct event type.
        /// </summary>
        public static int GetSubscriberCount<T>() where T : struct
            => EventHub<T>.Count;

        #endregion

        #region Legacy BaseEvent API  ──  BACKWARD COMPAT

        /// <summary>
        /// Publish a legacy BaseEvent (class-based). May allocate.
        /// </summary>
        public static void PublishLegacy(BaseEvent eventData)
        {
            EnsureLegacyInitialized();
            _eventCenter?.PublishEvent(eventData);
        }

        /// <summary>
        /// Publish a legacy BaseEvent with immediate processing.
        /// </summary>
        public static void PublishLegacyImmediate(BaseEvent eventData)
        {
            EnsureLegacyInitialized();
            _eventCenter?.PublishEventImmediate(eventData);
        }

        /// <summary>
        /// Subscribe to a legacy BaseEvent type.
        /// </summary>
        public static IEventSubscription SubscribeLegacy<T>(Action<T> callback, int priority = 0)
            where T : BaseEvent
        {
            EnsureLegacyInitialized();
            return _eventCenter?.SubscribeLegacy(callback, priority) ?? NullSubscription.Instance;
        }

        /// <summary>
        /// Subscribe to a legacy BaseEvent type using IEventListenerLegacy.
        /// </summary>
        public static IEventSubscription SubscribeLegacy<T>(IEventListenerLegacy<T> listener)
            where T : BaseEvent
        {
            EnsureLegacyInitialized();
            return _eventCenter?.SubscribeLegacy(listener) ?? NullSubscription.Instance;
        }

        private static void EnsureLegacyInitialized()
        {
            if (_isInitialized && _eventCenter != null) return;

            // Fallback: try to get EventCenterService for backward compat
            _eventCenter = EventCenterService.Current;
            if (_eventCenter != null) _isInitialized = true;
        }

        #endregion

        #region System Management

        /// <summary>
        /// Process all queued events (called automatically by EventCenter MonoBehaviour).
        /// </summary>
        public static void ProcessEvents()
        {
            _eventCenter?.ProcessEvents();
        }

        /// <summary>
        /// Check if logging is enabled.
        /// </summary>
        public static bool IsLoggingEnabled
            => _isInitialized && (_eventCenter?.IsLoggingEnabled ?? false);

        /// <summary>
        /// Get stats from the legacy event center (struct events are zero-overhead).
        /// </summary>
        public static EventCenterStats GetStats()
            => _isInitialized ? (_eventCenter?.GetStats() ?? default) : default;

        /// <summary>
        /// Clear all events and subscriptions (legacy queue).
        /// Struct-event subscriptions in EventHub are not affected.
        /// </summary>
        public static void Clear()
        {
            _eventCenter?.Clear();
        }

        /// <summary>
        /// Log the current status of the event system.
        /// No-op in release builds unless logging is enabled.
        /// </summary>
        public static void LogStatus()
        {
            if (!_isInitialized)
            {
                ConsoleLogger.Log("[EventSystem] Not initialized.");
                return;
            }
            var stats = GetStats();
            ConsoleLogger.Log($"[EventSystem] Active subscriptions: {stats.ActiveSubscriptions} | Queued events: {stats.QueuedEvents}");
        }

        /// <summary>
        /// Publish multiple struct events of the same type in one call.
        /// </summary>
        public static void PublishBatch<T>(T[] events, int priority = 0) where T : struct
        {
            if (events == null) return;
            for (int i = 0; i < events.Length; i++)
            {
                EventHub<T>.Publish(events[i]);
            }
        }

        #endregion

        #region Editor Bridge (compile-time stripped in builds)
#if UNITY_EDITOR
        private static class EditorBridge
        {
            private static bool _triedLookup;
            private static System.Reflection.MethodInfo _publishMethod;

            internal static void NotifyPublish<T>(T payload) where T : struct
            {
                if (!_triedLookup)
                {
                    _triedLookup = true;
                    var bridgeType = System.Type.GetType(
                        "EventCenter.EditorTools.EventCaptureBridge, Assembly-CSharp-Editor");
                    _publishMethod = bridgeType?.GetMethod(
                        "Publish",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                }

                if (_publishMethod == null) return;
                
                try
                {
                    _publishMethod.Invoke(null, new object[]
                    {
                        typeof(T).Name, (object)payload, null,
                        ResolveCategory(typeof(T).Name),
                        null
                    });
                }
                catch { /* Editor hook failure is non-critical */ }
            }

            private static string ResolveCategory(string name)
            {
                if (name.Contains("Player")) return "Player";
                if (name.Contains("UI"))     return "UI";
                if (name.Contains("Game"))   return "Gameplay";
                if (name.Contains("Audio") || name.Contains("Sound")) return "Audio";
                if (name.Contains("Network")) return "Network";
                return "Uncategorized";
            }
        }
#endif
        #endregion
    }
}