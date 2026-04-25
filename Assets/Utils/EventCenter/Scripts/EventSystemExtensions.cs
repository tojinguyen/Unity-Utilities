using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Extension methods that make event subscription feel native on MonoBehaviour.
    ///
    /// USAGE:
    /// <code>
    ///   // Auto-cleanup when GameObject is destroyed
    ///   this.Subscribe&lt;PlayerDeadEvent&gt;(OnPlayerDead);
    ///
    ///   // With priority
    ///   this.Subscribe&lt;PlayerDeadEvent&gt;(OnPlayerDead, priority: 10);
    ///
    ///   // Subscribe once, auto-cleanup
    ///   this.SubscribeOnce&lt;PlayerDeadEvent&gt;(OnPlayerDead);
    ///
    ///   // Subscribe with filter
    ///   this.SubscribeWhen&lt;PlayerDeadEvent&gt;(OnPlayerDead, e => e.IsLocalPlayer);
    /// </code>
    /// </summary>
    public static class EventSystemExtensions
    {
        // ── Core Subscribe ────────────────────────────────────────────────────

        /// <summary>
        /// Subscribe to a struct event.
        /// Automatically unsubscribes when the MonoBehaviour's GameObject is destroyed.
        /// </summary>
        public static IEventSubscription Subscribe<T>(
            this MonoBehaviour component,
            Action<T> callback,
            int priority = 0)
            where T : struct
        {
            if (!ValidateArgs(component, callback, nameof(Subscribe))) 
                return NullSubscription.Instance;

            var sub = EventSystem.Subscribe(callback, priority);
            component.GetCancellationTokenOnDestroy().Register(sub.Dispose);
            return sub;
        }

        // ── Subscribe Once ────────────────────────────────────────────────────

        /// <summary>
        /// Subscribe to a struct event that fires only once, then auto-unsubscribes.
        /// Also unsubscribes if the component is destroyed before the event fires.
        /// </summary>
        public static IEventSubscription SubscribeOnce<T>(
            this MonoBehaviour component,
            Action<T> callback,
            int priority = 0)
            where T : struct
        {
            if (!ValidateArgs(component, callback, nameof(SubscribeOnce)))
                return NullSubscription.Instance;

            var sub = EventSystem.SubscribeOnce(callback, priority);
            component.GetCancellationTokenOnDestroy().Register(sub.Dispose);
            return sub;
        }

        // ── Subscribe When ────────────────────────────────────────────────────

        /// <summary>
        /// Subscribe to a struct event with a filter.
        /// Callback only fires when <paramref name="condition"/> returns true.
        /// Auto-cleanup on destroy.
        /// </summary>
        public static IEventSubscription SubscribeWhen<T>(
            this MonoBehaviour component,
            Action<T> callback,
            Func<T, bool> condition,
            int priority = 0)
            where T : struct
        {
            if (component == null || callback == null || condition == null)
            {
                ConsoleLogger.LogError("[EventSystem] SubscribeWhen: null argument.");
                return NullSubscription.Instance;
            }

            var sub = EventSystem.SubscribeWhen(callback, condition, priority);
            component.GetCancellationTokenOnDestroy().Register(sub.Dispose);
            return sub;
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static bool ValidateArgs<T>(MonoBehaviour component, Action<T> callback, string methodName)
        {
            if (component == null)
            {
                ConsoleLogger.LogError($"[EventSystem] {methodName}: component is null.");
                return false;
            }
            if (callback == null)
            {
                ConsoleLogger.LogError($"[EventSystem] {methodName}: callback is null.");
                return false;
            }
            return true;
        }

        // ── Backward-compat aliases (old names → new names) ───────────────────
        // These allow existing code to compile without changes.
        // Use the shorter names (Subscribe / SubscribeWhen / SubscribeOnce) in new code.

        /// <inheritdoc cref="Subscribe{T}(MonoBehaviour, Action{T}, int)"/>
        [System.Obsolete("Use Subscribe<T>() instead.")]
        public static IEventSubscription SubscribeWithCleanup<T>(
            this MonoBehaviour component, Action<T> callback, int priority = 0)
            where T : struct
            => Subscribe(component, callback, priority);

        /// <inheritdoc cref="SubscribeWhen{T}(MonoBehaviour, Action{T}, Func{T,bool}, int)"/>
        [System.Obsolete("Use SubscribeWhen<T>() instead.")]
        public static IEventSubscription SubscribeWhenWithCleanup<T>(
            this MonoBehaviour component, Action<T> callback, Func<T, bool> condition, int priority = 0)
            where T : struct
            => SubscribeWhen(component, callback, condition, priority);

        /// <inheritdoc cref="SubscribeOnce{T}(MonoBehaviour, Action{T}, int)"/>
        [System.Obsolete("Use SubscribeOnce<T>() instead.")]
        public static IEventSubscription SubscribeOnceWithCleanup<T>(
            this MonoBehaviour component, Action<T> callback, int priority = 0)
            where T : struct
            => SubscribeOnce(component, callback, priority);

        /// <inheritdoc cref="SubscribeOnce{T}(MonoBehaviour, Action{T}, int)"/>
        [System.Obsolete("Use SubscribeOnce<T>(callback, condition) instead.")]
        public static IEventSubscription SubscribeOnceWithCleanup<T>(
            this MonoBehaviour component, Action<T> callback, Func<T, bool> condition, int priority = 0)
            where T : struct
        {
            if (!ValidateArgs(component, callback, nameof(SubscribeOnceWithCleanup)))
                return NullSubscription.Instance;

            IEventSubscription sub = null;
            sub = Subscribe<T>(component, payload =>
            {
                if (condition(payload))
                {
                    callback(payload);
                    sub?.Dispose();
                }
            }, priority);
            return sub;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AddTo — fluent chaining into a group or MonoBehaviour lifetime
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Extension methods on IEventSubscription for fluent group management.
    /// </summary>
    public static class EventSubscriptionExtensions
    {
        // ── AddTo EventSubscriptionGroup ──────────────────────────────────────

        /// <summary>
        /// Add this subscription to a group.
        /// The group disposes it when <see cref="EventSubscriptionGroup.Dispose"/> is called.
        ///
        /// <code>
        ///   // Non-MonoBehaviour class:
        ///   private readonly EventSubscriptionGroup _subs = new EventSubscriptionGroup();
        ///
        ///   void Init()
        ///   {
        ///       EventSystem.Subscribe&lt;EventA&gt;(OnA).AddTo(_subs);
        ///       EventSystem.Subscribe&lt;EventB&gt;(OnB).AddTo(_subs);
        ///   }
        ///
        ///   void Cleanup() => _subs.Dispose();
        /// </code>
        /// </summary>
        /// <returns>The original subscription (for further chaining if needed).</returns>
        public static IEventSubscription AddTo(this IEventSubscription subscription, EventSubscriptionGroup group)
        {
            group?.Add(subscription);
            return subscription;
        }

        // ── AddTo MonoBehaviour ───────────────────────────────────────────────

        /// <summary>
        /// Wire this subscription to a MonoBehaviour's lifetime.
        /// It will be disposed automatically when the MonoBehaviour's GameObject is destroyed.
        ///
        /// Equivalent to using <c>this.Subscribe&lt;T&gt;(callback)</c> but works on
        /// any subscription, including those created outside the MonoBehaviour.
        ///
        /// <code>
        ///   EventSystem.Subscribe&lt;PlayerDeadEvent&gt;(OnPlayerDead).AddTo(this);
        /// </code>
        /// </summary>
        /// <returns>The original subscription.</returns>
        public static IEventSubscription AddTo(this IEventSubscription subscription, MonoBehaviour component)
        {
            if (subscription == null || component == null) return subscription;
            component.GetCancellationTokenOnDestroy().Register(subscription.Dispose);
            return subscription;
        }
    }
}