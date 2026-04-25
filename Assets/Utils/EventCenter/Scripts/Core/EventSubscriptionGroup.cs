using System;
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// A bag that collects multiple IEventSubscription tokens and disposes them all at once.
    ///
    /// USAGE:
    /// <code>
    ///   private readonly EventSubscriptionGroup _subs = new EventSubscriptionGroup();
    ///
    ///   void Init()
    ///   {
    ///       EventSystem.Subscribe&lt;EventA&gt;(OnA).AddTo(_subs);
    ///       EventSystem.Subscribe&lt;EventB&gt;(OnB).AddTo(_subs);
    ///       this.Subscribe&lt;EventC&gt;(OnC).AddTo(_subs); // MonoBehaviour auto-cleanup stacks
    ///   }
    ///
    ///   void Cleanup()
    ///   {
    ///       _subs.Dispose(); // unsubscribes everything in one line
    ///   }
    /// </code>
    ///
    /// After Dispose(), the group is reset and can be reused.
    /// </summary>
    public sealed class EventSubscriptionGroup : IDisposable
    {
        private readonly List<IEventSubscription> _subscriptions = new List<IEventSubscription>(8);
        private bool _disposed;

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Number of active subscriptions currently in this group.
        /// </summary>
        public int Count => _subscriptions.Count;

        /// <summary>
        /// Add a subscription to this group.
        /// The subscription will be disposed when the group is disposed.
        /// </summary>
        public void Add(IEventSubscription subscription)
        {
            if (subscription == null || subscription is NullSubscription) return;
            if (_disposed)
            {
                // Group already disposed — immediately dispose the incoming subscription
                subscription.Dispose();
                return;
            }
            _subscriptions.Add(subscription);
        }

        /// <summary>
        /// Dispose all subscriptions in the group and clear the list.
        /// The group can be reused after this call.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            DisposeAll();
            _disposed = false; // allow reuse
        }

        /// <summary>
        /// Dispose all subscriptions and prevent future adds.
        /// Use this when the owning object is being destroyed permanently.
        /// </summary>
        public void DisposeAndLock()
        {
            _disposed = true;
            DisposeAll();
        }

        /// <summary>
        /// Remove and dispose a specific subscription from the group.
        /// </summary>
        public void Remove(IEventSubscription subscription)
        {
            if (subscription == null) return;
            if (_subscriptions.Remove(subscription))
            {
                subscription.Dispose();
            }
        }

        /// <summary>
        /// Clear the group without disposing the subscriptions.
        /// Use only when subscriptions are managed elsewhere.
        /// </summary>
        public void Clear()
        {
            _subscriptions.Clear();
        }

        // ── Private ────────────────────────────────────────────────────────────

        private void DisposeAll()
        {
            for (int i = _subscriptions.Count - 1; i >= 0; i--)
            {
                try
                {
                    _subscriptions[i]?.Dispose();
                }
                catch (Exception ex)
                {
                    ConsoleLogger.LogError($"[EventSubscriptionGroup] Error disposing subscription: {ex.Message}");
                }
            }
            _subscriptions.Clear();
        }
    }
}
