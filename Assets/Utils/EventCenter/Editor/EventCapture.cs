// Editor-side capture buffer and bridge for EventCenter
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EventCenter.EditorTools
{
    internal static class EventCapture
    {
        public const int DefaultBufferSize = 10000;

        private static readonly object _lock = new object();
        private static EventRecord[] _ring = new EventRecord[DefaultBufferSize];
        private static int _head;
        private static int _count;
        private static bool _recording = true;
        private static bool _paused;
        private static DateTime _sessionStartUtc = DateTime.UtcNow;

        public static event Action OnDataChanged;
        public static event Action<EventRecord> OnAppended;

        public static int Capacity => _ring.Length;
        public static int Count
        {
            get { lock (_lock) return _count; }
        }
        public static bool IsRecording
        {
            get => _recording;
            set { _recording = value; }
        }
        public static bool IsPaused
        {
            get => _paused;
            set { _paused = value; }
        }
        public static DateTime SessionStartUtc => _sessionStartUtc;

        public static void ConfigureCapacity(int capacity)
        {
            if (capacity <= 0) return;
            lock (_lock)
            {
                var newRing = new EventRecord[capacity];
                int toCopy = Math.Min(_count, capacity);
                for (int i = 0; i < toCopy; i++)
                {
                    newRing[i] = GetUnsafe(i);
                }
                _ring = newRing;
                _head = Math.Min(toCopy, capacity - 1);
                _count = toCopy;
            }
            OnDataChanged?.Invoke();
        }

        public static void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_ring, 0, _ring.Length);
                _head = 0;
                _count = 0;
                _sessionStartUtc = DateTime.UtcNow;
            }
            OnDataChanged?.Invoke();
        }

        public static void Append(EventRecord rec)
        {
            Debug.Log($"[EventCapture] Append called - Recording: {_recording}, Paused: {_paused}, Event: {rec?.name}");
            
            if (!_recording || _paused || rec == null) 
            {
                Debug.Log($"[EventCapture] Append skipped - Recording: {_recording}, Paused: {_paused}, Rec null: {rec == null}");
                return;
            }
            
            lock (_lock)
            {
                _head = (_head + 1) % _ring.Length;
                _ring[_head] = rec;
                _count = Math.Min(_count + 1, _ring.Length);
                Debug.Log($"[EventCapture] Event appended to buffer. Count: {_count}, Head: {_head}");
            }
            OnDataChanged?.Invoke();
            OnAppended?.Invoke(rec);
            Debug.Log($"[EventCapture] Callbacks invoked for event: {rec.name}");
        }

        public static EventRecord Get(int index)
        {
            lock (_lock)
            {
                return GetUnsafe(index);
            }
        }

        private static EventRecord GetUnsafe(int index)
        {
            if (_count == 0) return null;
            if (index < 0 || index >= _count) return null;
            int start = (_head - _count + 1 + _ring.Length) % _ring.Length;
            int i = (start + index) % _ring.Length;
            return _ring[i];
        }

        public static IEnumerable<EventRecord> Enumerate()
        {
            int localCount;
            lock (_lock) localCount = _count;
            for (int i = 0; i < localCount; i++)
            {
                yield return Get(i);
            }
        }

        // Bridge API callable from runtime (if compiled in Editor)
        public static void OnPublish(string eventName, object payload, UnityEngine.Object source, string category,
            List<ListenerRecord> listeners)
        {
            Debug.Log($"[EventCapture] OnPublish called - Event: {eventName}, Category: {category}, Source: {source}");
            Debug.Log($"[EventCapture] Recording: {_recording}, Paused: {_paused}");
            
            var rec = new EventRecord
            {
                timeRealtime = Time.realtimeSinceStartupAsDouble,
                gameTime = Application.isPlaying ? Time.time : 0f,
                name = eventName,
                category = string.IsNullOrEmpty(category) ? "Uncategorized" : category,
                payloadPreview = SafePayloadToString(payload),
                sourceInfo = new SourceInfo
                {
                    objectName = source ? source.name : "<null>",
                    typeName = source ? source.GetType().Name : "Unknown",
                    instanceId = source ? source.GetInstanceID() : 0,
                    hierarchyPath = ResolveHierarchyPath(source)
                },
                listeners = listeners ?? new List<ListenerRecord>()
            };
            
            Debug.Log($"[EventCapture] Created EventRecord: {rec.name} at {rec.timeRealtime:F3}s");
            Append(rec);
            Debug.Log($"[EventCapture] EventRecord appended to buffer");
        }

        private static string ResolveHierarchyPath(UnityEngine.Object obj)
        {
            var go = obj as GameObject;
            if (!go)
            {
                if (obj is Component c) go = c.gameObject;
            }
            if (!go) return string.Empty;
            var path = go.name;
            var t = go.transform;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private static string SafePayloadToString(object payload)
        {
            if (payload == null) return "<null>";
            try
            {
                return JsonUtility.ToJson(payload);
            }
            catch
            {
                try { return payload.ToString(); }
                catch { return "<payload>"; }
            }
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Debug.Log("[EventCapture] Init called");
            EditorApplication.playModeStateChanged += state =>
            {
                Debug.Log($"[EventCapture] PlayMode state changed: {state}");
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    Debug.Log("[EventCapture] Entered PlayMode - clearing and starting recording");
                    Clear();
                    _recording = true;
                    _paused = false;
                    Debug.Log($"[EventCapture] Recording state: {_recording}, Paused: {_paused}");
                }
            };
            
            // Ensure recording is enabled by default
            _recording = true;
            _paused = false;
            Debug.Log($"[EventCapture] Init completed - Recording: {_recording}, Paused: {_paused}");
        }
    }
}
#endif


