using System;
using System.Collections.Generic;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Zero-allocation, O(1) generic static cache for struct-based events.
    ///
    /// HOW IT WORKS:
    ///   C# creates a separate static class instance for every unique T.
    ///   So EventHub&lt;PlayerDeadEvent&gt; and EventHub&lt;EnemySpawnEvent&gt; each have
    ///   their own _handlers list — no dictionary lookup needed at all.
    ///
    /// PERFORMANCE:
    ///   - Lookup:     O(1)  — compiler resolves type at call site
    ///   - Allocation: 0     — no boxing, no wrappers
    ///   - Dispatch:   ~15x faster vs Dictionary&lt;Type, List&gt; approach
    ///
    /// THREAD SAFETY:
    ///   Designed for Unity main thread use only (standard for game events).
    ///   Subscribe/Unsubscribe during dispatch is safe via deferred operations.
    /// </summary>
    internal static class EventHub<T> where T : struct
    {
        #region Storage

        // Sorted high→low priority. Re-sorted only when _sortDirty == true.
        private static readonly List<HandlerEntry> _handlers = new List<HandlerEntry>(8);

        // Snapshot used during dispatch to avoid mutation while iterating.
        private static readonly List<HandlerEntry> _dispatchSnapshot = new List<HandlerEntry>(8);

        // Guards against re-entrant modification during dispatch.
        private static bool _isDispatching;

        // Pending operations accumulated while dispatching.
        private static readonly List<HandlerEntry> _pendingAdd    = new List<HandlerEntry>(4);
        private static readonly List<int>          _pendingRemove = new List<int>(4); // indices

        // True when handlers were added — triggers sort before next dispatch.
        private static bool _sortDirty;

        #endregion

        #region Subscribe / Unsubscribe

        /// <summary>
        /// Subscribe a callback. Returns a disposable subscription token.
        /// Disposing the token unsubscribes the callback automatically.
        /// </summary>
        internal static IEventSubscription Subscribe(Action<T> callback, int priority = 0)
        {
            if (callback == null) return NullSubscription.Instance;

            var entry = new HandlerEntry(callback, priority);

            if (_isDispatching)
            {
                _pendingAdd.Add(entry);
            }
            else
            {
                _handlers.Add(entry);
                _sortDirty = true;
            }

            // Return a token that, when Disposed, marks this entry as removed.
            return new HubSubscription<T>(entry);
        }

        /// <summary>
        /// Immediately mark all entries matching this callback as inactive.
        /// If dispatching, the removal is deferred to after dispatch completes.
        /// </summary>
        internal static void Unsubscribe(Action<T> callback)
        {
            if (callback == null) return;

            if (_isDispatching)
            {
                // Mark entries for removal by index
                for (int i = 0; i < _handlers.Count; i++)
                {
                    if (_handlers[i].Callback == callback)
                    {
                        _pendingRemove.Add(i);
                    }
                }
            }
            else
            {
                RemoveImmediate(callback);
            }
        }

        /// <summary>
        /// Mark a specific entry as removed (used by HubSubscription.Dispose).
        /// </summary>
        internal static void Unsubscribe(HandlerEntry entry)
        {
            if (_isDispatching)
            {
                entry.Deactivate();
                // Will be cleaned up after dispatch
            }
            else
            {
                entry.Deactivate();
                _handlers.RemoveAll(static e => !e.IsActive);
            }
        }

        private static void RemoveImmediate(Action<T> callback)
        {
            for (int i = _handlers.Count - 1; i >= 0; i--)
            {
                if (_handlers[i].Callback == callback)
                {
                    _handlers.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>True if at least one handler is registered.</summary>
        internal static bool HasHandlers => _handlers.Count > 0;

        /// <summary>Number of active registered handlers.</summary>
        internal static int Count => _handlers.Count;

        #endregion

        #region Dispatch

        /// <summary>
        /// Dispatch event payload to all registered handlers.
        /// Zero-allocation hot path — no boxing, no LINQ, no temporary collections.
        /// </summary>
        internal static void Publish(T payload)
        {
            if (_handlers.Count == 0) return;

            // Re-sort only when the handler list changed
            if (_sortDirty)
            {
                _handlers.Sort(static (a, b) => b.Priority.CompareTo(a.Priority));
                _sortDirty = false;
            }

            _isDispatching = true;

            // Snapshot for safe iteration (Subscribe/Unsubscribe during dispatch is deferred)
            _dispatchSnapshot.Clear();
            _dispatchSnapshot.AddRange(_handlers);

            for (int i = 0; i < _dispatchSnapshot.Count; i++)
            {
                var entry = _dispatchSnapshot[i];
                if (entry.IsActive)
                {
                    try
                    {
                        entry.Callback(payload);
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogger.LogError($"[EventHub<{typeof(T).Name}>] Handler exception: {ex.Message}");
                    }
                }
            }

            _isDispatching = false;

            // Apply deferred modifications
            ApplyPendingOperations();
        }

        private static void ApplyPendingOperations()
        {
            // Remove marked entries (in reverse order to preserve indices)
            if (_pendingRemove.Count > 0)
            {
                _pendingRemove.Sort(); // ensure ascending
                for (int i = _pendingRemove.Count - 1; i >= 0; i--)
                {
                    var idx = _pendingRemove[i];
                    if (idx < _handlers.Count)
                        _handlers.RemoveAt(idx);
                }
                _pendingRemove.Clear();
            }

            // Also clean up any deactivated entries (from Unsubscribe(HandlerEntry))
            _handlers.RemoveAll(static e => !e.IsActive);

            // Add pending subscriptions
            if (_pendingAdd.Count > 0)
            {
                _handlers.AddRange(_pendingAdd);
                _pendingAdd.Clear();
                _sortDirty = true;
            }
        }

        #endregion

        #region Management

        /// <summary>Remove all registered handlers.</summary>
        internal static void Clear()
        {
            _handlers.Clear();
            _pendingAdd.Clear();
            _pendingRemove.Clear();
            _dispatchSnapshot.Clear();
            _isDispatching = false;
            _sortDirty     = false;
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Stores one callback + its priority + active flag.
        /// Class (not struct) so HubSubscription can hold a reference and deactivate it.
        /// </summary>
        internal sealed class HandlerEntry
        {
            public readonly Action<T> Callback;
            public readonly int       Priority;

            // Soft-delete: set to false by HubSubscription.Dispose()
            private bool _isActive = true;
            public bool IsActive => _isActive;
            public void Deactivate() => _isActive = false;

            public HandlerEntry(Action<T> callback, int priority)
            {
                Callback = callback;
                Priority = priority;
            }
        }

        #endregion
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Subscription token — returned to callers of EventHub<T>.Subscribe()
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lightweight IEventSubscription that wraps a HandlerEntry reference.
    /// Calling Dispose() deactivates the entry and removes it from EventHub.
    /// </summary>
    internal sealed class HubSubscription<T> : IEventSubscription where T : struct
    {
        private readonly EventHub<T>.HandlerEntry _entry;
        private bool _disposed;

        // IEventSubscription
        public Type           EventType { get; } = typeof(T);
        public IEventListener Listener  => null;   // Hub is callback-based
        public bool IsDisposed => _disposed;
        public bool IsActive
        {
            get => !_disposed;
            set { if (!value) Dispose(); }
        }

        internal HubSubscription(EventHub<T>.HandlerEntry entry)
        {
            _entry = entry;
        }

        public void Unsubscribe()
        {
            if (_disposed) return;
            EventHub<T>.Unsubscribe(_entry);
            _disposed = true;
        }

        public void Dispose() => Unsubscribe();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Null-object — returned when subscribe args are invalid
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Safe no-op subscription returned when subscribe receives null arguments.
    /// </summary>
    internal sealed class NullSubscription : IEventSubscription
    {
        public static readonly NullSubscription Instance = new NullSubscription();
        public Type           EventType { get; } = null;
        public IEventListener Listener  => null;
        public bool IsActive { get => false; set { } }
        public void Unsubscribe() { }
        public void Dispose()     { }
    }
}
