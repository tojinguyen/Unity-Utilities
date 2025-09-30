// Dockable EditorWindow: Timeline, Filters, Details, Recording, Replay, Highlight Flow
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EventCenter.EditorTools
{
    public class EventVisualizerWindow : EditorWindow
    {
        private EventVisualizerConfig _config;
        private Vector2 _leftScroll;
        private Vector2 _timelineScroll;
        private Vector2 _rightScroll;

        // Filters
        private string _searchText = string.Empty;
        private string _filterCategory = string.Empty;
        private string _filterSource = string.Empty;
        private string _filterListener = string.Empty;
        private double _timeMin = 0;
        private double _timeMax = 0;
        private bool _caseInsensitive = true;

        // Timeline zoom/pan
        private float _pixelsPerSecond = 100f;
        private float _minPixelsPerSecond = 20f;
        private float _maxPixelsPerSecond = 400f;
        private double _viewStartTime;
        private double _viewEndTime;

        // Selection / highlight
        private EventRecord _selected;

        // Replay
        private bool _replayMode;
        private double _replayPlayhead;
        private bool _replayPlaying;

        private readonly Dictionary<string, bool> _visibleChannels = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private List<string> _watchList = new List<string>();
        private string _breakpointPattern = string.Empty;
        private bool _breakOnMatch;

        [MenuItem("Window/Event Visualizer")] 
        public static void Open()
        {
            var win = GetWindow<EventVisualizerWindow>("EventCenter Timeline");
            win.Show();
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            _config = EventVisualizerConfig.LoadOrCreate();
            if (_config != null)
            {
                EventCapture.ConfigureCapacity(Mathf.Max(1000, _config.bufferSize));
                _pixelsPerSecond = Mathf.Max(_config.defaultZoom, 20f);
            }
            EventCapture.OnDataChanged += Repaint;
            EventCapture.OnAppended += OnEventAppended;
            _viewStartTime = 0;
            _viewEndTime = 10;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EventCapture.OnDataChanged -= Repaint;
            EventCapture.OnAppended -= OnEventAppended;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (_replayMode && _replayPlaying)
            {
                _replayPlayhead += EditorApplication.timeSinceStartup - _lastUpdateTime;
                Repaint();
            }
            _lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        private double _lastUpdateTime;

        private void OnGUI()
        {
            DrawToolbar();

            var rect = position;
            float leftWidth = 200f;
            float rightWidth = 320f;
            float toolbarHeight = 22f;

            var body = new Rect(0, toolbarHeight, rect.width, rect.height - toolbarHeight);
            var left = new Rect(body.x, body.y, leftWidth, body.height);
            var center = new Rect(left.xMax, body.y, body.width - leftWidth - rightWidth, body.height);
            var right = new Rect(center.xMax, body.y, rightWidth, body.height);

            GUILayout.BeginArea(left, EditorStyles.helpBox);
            _leftScroll = GUILayout.BeginScrollView(_leftScroll);
            DrawChannelList();
            GUILayout.Space(8);
            DrawWatchAndBreakpoints();
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUILayout.BeginArea(center);
            DrawTimelineArea(center);
            GUILayout.EndArea();

            GUILayout.BeginArea(right, EditorStyles.helpBox);
            _rightScroll = GUILayout.BeginScrollView(_rightScroll);
            DrawDetailsPanel();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Toggle(EventCapture.IsRecording, new GUIContent("Record"), EditorStyles.toolbarButton) != EventCapture.IsRecording)
                {
                    EventCapture.IsRecording = !EventCapture.IsRecording;
                }
                if (GUILayout.Toggle(EventCapture.IsPaused, new GUIContent("Pause"), EditorStyles.toolbarButton) != EventCapture.IsPaused)
                {
                    EventCapture.IsPaused = !EventCapture.IsPaused;
                }
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                {
                    EventCapture.Clear();
                    _selected = null;
                }
                if (Event.current.type == EventType.KeyDown)
                {
                    // Hotkeys: Ctrl/Cmd+R, Ctrl/Cmd+P, Ctrl/Cmd+K, F(Fit)
                    bool ctrl = Event.current.control || Event.current.command;
                    if (ctrl && Event.current.keyCode == KeyCode.R) { EventCapture.IsRecording = !EventCapture.IsRecording; Repaint(); }
                    if (ctrl && Event.current.keyCode == KeyCode.P) { EventCapture.IsPaused = !EventCapture.IsPaused; Repaint(); }
                    if (ctrl && Event.current.keyCode == KeyCode.K) { EventCapture.Clear(); _selected = null; Repaint(); }
                    if (Event.current.keyCode == KeyCode.F) { ZoomToFit(); Repaint(); }
                }
                GUILayout.Space(8);

                _searchText = GUILayout.TextField(_searchText, EditorStyles.toolbarTextField, GUILayout.MinWidth(160));
                _caseInsensitive = GUILayout.Toggle(_caseInsensitive, new GUIContent("Aa"), EditorStyles.toolbarButton, GUILayout.Width(28));

                GUILayout.Space(8);
                GUILayout.Label("Cat:", GUILayout.Width(28));
                _filterCategory = GUILayout.TextField(_filterCategory, EditorStyles.toolbarTextField, GUILayout.Width(120));
                GUILayout.Label("Src:", GUILayout.Width(26));
                _filterSource = GUILayout.TextField(_filterSource, EditorStyles.toolbarTextField, GUILayout.Width(120));
                GUILayout.Label("Lst:", GUILayout.Width(26));
                _filterListener = GUILayout.TextField(_filterListener, EditorStyles.toolbarTextField, GUILayout.Width(120));

                GUILayout.FlexibleSpace();

                GUILayout.Label("Zoom", GUILayout.Width(36));
                _pixelsPerSecond = GUILayout.HorizontalSlider(_pixelsPerSecond, _minPixelsPerSecond, _maxPixelsPerSecond, GUILayout.Width(120));
                if (GUILayout.Button("Fit", EditorStyles.toolbarButton, GUILayout.Width(40)))
                {
                    ZoomToFit();
                }

                GUILayout.Space(8);
                // Replay controls
                _replayMode = GUILayout.Toggle(_replayMode, new GUIContent("Replay"), EditorStyles.toolbarButton);
                using (new EditorGUI.DisabledScope(!_replayMode))
                {
                    if (GUILayout.Button(_replayPlaying ? "Pause▶" : "Play▶", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    {
                        _replayPlaying = !_replayPlaying;
                    }
                }

                if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton))
                {
                    ExportJson();
                }
                if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton))
                {
                    ExportCsv();
                }
            }
        }

        private void DrawChannelList()
        {
            GUILayout.Label("Channels", EditorStyles.boldLabel);
            var grouped = EventCapture.Enumerate().GroupBy(e => e.category ?? "Uncategorized").OrderBy(g => g.Key);
            foreach (var g in grouped)
            {
                if (!_visibleChannels.ContainsKey(g.Key)) _visibleChannels[g.Key] = true;
                _visibleChannels[g.Key] = EditorGUILayout.ToggleLeft(g.Key + " (" + g.Count() + ")", _visibleChannels[g.Key]);
            }
        }

        private void DrawTimelineArea(Rect rect)
        {
            var events = ApplyFilters(EventCapture.Enumerate());
            if (!events.Any())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("No events.", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                return;
            }

            // Compute time bounds
            double minT = events.Min(e => e.timeRealtime);
            double maxT = events.Max(e => e.timeRealtime);
            if (_viewEndTime <= _viewStartTime)
            {
                _viewStartTime = minT;
                _viewEndTime = minT + 10.0;
            }

            // Scrollable timeline canvas
            float totalWidth = Mathf.Max((float)((maxT - minT) * _pixelsPerSecond) + 200f, rect.width);
            _timelineScroll = GUILayout.BeginScrollView(_timelineScroll, GUILayout.ExpandHeight(true));

            var canvas = GUILayoutUtility.GetRect(totalWidth, rect.height - 8f);
            DrawTimeGrid(canvas, minT, maxT);

            // Layout channels vertically
            var rows = events.GroupBy(e => e.category ?? "Uncategorized").OrderBy(g => g.Key).ToList();
            float rowHeight = 24f;
            float rowGap = 6f;
            float y = canvas.y + 24f;

            foreach (var row in rows)
            {
                if (_visibleChannels.TryGetValue(row.Key, out var visible) && !visible) continue;
                var labelRect = new Rect(canvas.x + 4, y, 200, rowHeight);
                EditorGUI.LabelField(labelRect, row.Key, EditorStyles.miniBoldLabel);

                foreach (var ev in row)
                {
                    float x = canvas.x + 200 + (float)((ev.timeRealtime - minT) * _pixelsPerSecond);
                    var r = new Rect(x, y, 120, rowHeight);
                    ev.lastDrawRect = r;
                    ev.cachedColor = GetColorFor(ev);
                    EditorGUI.DrawRect(r, new Color(ev.cachedColor.r, ev.cachedColor.g, ev.cachedColor.b, 0.85f));
                    var label = new GUIContent(ev.name);
                    var style = EditorStyles.whiteMiniLabel;
                    var labelRect2 = new Rect(r.x + 4, r.y + 4, r.width - 8, r.height - 8);
                    Color old = GUI.color;
                    if (_selected == ev) GUI.color = Color.yellow;
                    GUI.Label(labelRect2, label, style);
                    GUI.color = old;

                    // Selection
                    if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                    {
                        _selected = ev;
                        Repaint();
                        Event.current.Use();
                    }

                    // Draw link lines to listeners when selected
                    if (_selected == ev && ev.listeners != null && ev.listeners.Count > 0)
                    {
                        DrawListenerLinks(r, ev.listeners, y + rowHeight + 3);
                    }
                }

                y += rowHeight + rowGap;
            }

            GUILayout.EndScrollView();
        }

        private void DrawTimeGrid(Rect canvas, double minT, double maxT)
        {
            EditorGUI.DrawRect(canvas, new Color(0.12f, 0.12f, 0.12f, 1f));
            Handles.color = new Color(1, 1, 1, 0.06f);
            float secondsStep = Mathf.Max(0.1f, 1f * (100f / _pixelsPerSecond));
            double start = Math.Floor(minT / secondsStep) * secondsStep;
            for (double t = start; t < maxT + 2 * secondsStep; t += secondsStep)
            {
                float x = canvas.x + 200 + (float)((t - minT) * _pixelsPerSecond);
                Handles.DrawLine(new Vector2(x, canvas.y), new Vector2(x, canvas.yMax));
                var rect = new Rect(x + 2, canvas.y, 100, 18);
                GUI.Label(rect, t.ToString("F2") + "s", EditorStyles.miniLabel);
            }
        }

        private void DrawListenerLinks(Rect from, List<ListenerRecord> listeners, float baseY)
        {
            Handles.color = Color.yellow;
            float startX = from.xMax;
            float startY = from.center.y;
            float offset = 0f;
            foreach (var _ in listeners)
            {
                var end = new Vector2(startX + 80 + offset, baseY + offset);
                Handles.DrawAAPolyLine(2f, new Vector3(startX, startY), end);
                offset += 10f;
            }
        }

        private void DrawDetailsPanel()
        {
            GUILayout.Label("Event Details", EditorStyles.boldLabel);
            if (_selected == null)
            {
                GUILayout.Label("Select an event to inspect.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

            EditorGUILayout.LabelField("Name", _selected.name);
            EditorGUILayout.LabelField("Category", _selected.category);
            EditorGUILayout.LabelField("Time (realtime)", _selected.timeRealtime.ToString("F3") + "s");
            EditorGUILayout.LabelField("Game Time", _selected.gameTime.ToString("F3") + "s");

            GUILayout.Space(6);
            GUILayout.Label("Source", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Object", _selected.sourceInfo.objectName);
            EditorGUILayout.LabelField("Type", _selected.sourceInfo.typeName);
            EditorGUILayout.LabelField("Path", _selected.sourceInfo.hierarchyPath);

            GUILayout.Space(6);
            GUILayout.Label("Payload", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.TextArea(_selected.payloadPreview ?? string.Empty, GUILayout.MinHeight(60));
            }

            GUILayout.Space(6);
            GUILayout.Label("Listeners", EditorStyles.miniBoldLabel);
            if (_selected.listeners == null || _selected.listeners.Count == 0)
            {
                GUILayout.Label("None", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                foreach (var l in _selected.listeners)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Name", l.name);
                        EditorGUILayout.LabelField("Target", l.targetInfo.objectName + " (" + l.targetInfo.typeName + ")");
                        EditorGUILayout.LabelField("Duration", l.durationMs.ToString("F3") + " ms");
                        if (!string.IsNullOrEmpty(l.exception))
                        {
                            EditorGUILayout.LabelField("Exception", l.exception);
                        }
                    }
                }
            }
        }

        private void DrawWatchAndBreakpoints()
        {
            GUILayout.Label("Watch List", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int i = 0; i < _watchList.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _watchList[i] = EditorGUILayout.TextField(_watchList[i]);
                        if (GUILayout.Button("-", GUILayout.Width(22)))
                        {
                            _watchList.RemoveAt(i);
                            i--;
                        }
                    }
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+ Add Watch")) _watchList.Add("");
                    if (GUILayout.Button("Preserve", GUILayout.Width(70))) { /* reserved flag if needed */ }
                }
            }

            GUILayout.Space(6);
            GUILayout.Label("Breakpoints", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _breakOnMatch = EditorGUILayout.ToggleLeft("Pause on match", _breakOnMatch);
                _breakpointPattern = EditorGUILayout.TextField("Event Name Contains", _breakpointPattern);
            }
        }

        private IEnumerable<EventRecord> ApplyFilters(IEnumerable<EventRecord> events)
        {
            var e = events;
            if (!string.IsNullOrEmpty(_searchText))
            {
                StringComparison cmp = _caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                e = e.Where(x => (!string.IsNullOrEmpty(x.name) && x.name.IndexOf(_searchText, cmp) >= 0));
            }
            if (!string.IsNullOrEmpty(_filterCategory))
            {
                StringComparison cmp = _caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                e = e.Where(x => (!string.IsNullOrEmpty(x.category) && x.category.IndexOf(_filterCategory, cmp) >= 0));
            }
            if (!string.IsNullOrEmpty(_filterSource))
            {
                StringComparison cmp = _caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                e = e.Where(x => (!string.IsNullOrEmpty(x.sourceInfo?.objectName) && x.sourceInfo.objectName.IndexOf(_filterSource, cmp) >= 0)
                                 || (!string.IsNullOrEmpty(x.sourceInfo?.typeName) && x.sourceInfo.typeName.IndexOf(_filterSource, cmp) >= 0));
            }
            if (!string.IsNullOrEmpty(_filterListener))
            {
                StringComparison cmp = _caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                e = e.Where(x => x.listeners != null && x.listeners.Any(l => !string.IsNullOrEmpty(l.name) && l.name.IndexOf(_filterListener, cmp) >= 0));
            }
            if (_timeMax > _timeMin)
            {
                e = e.Where(x => x.timeRealtime >= _timeMin && x.timeRealtime <= _timeMax);
            }
            return e;
        }

        private void ZoomToFit()
        {
            var events = EventCapture.Enumerate().ToList();
            if (events.Count < 2) return;
            double minT = events.Min(e => e.timeRealtime);
            double maxT = events.Max(e => e.timeRealtime);
            double span = Math.Max(0.1, maxT - minT);
            float available = position.width - 520f; // minus side panels
            _pixelsPerSecond = Mathf.Clamp((float)(available / span), _minPixelsPerSecond, _maxPixelsPerSecond);
        }

        private Color GetColorFor(EventRecord ev)
        {
            // Config-driven colors with fallback hashing
            if (_config != null)
            {
                var cc = _config.GetChannelColor(ev.category ?? "Uncategorized");
                if (cc.a > 0f) return cc;
            }
            int hash = (ev.category ?? "").GetHashCode();
            UnityEngine.Random.InitState(hash);
            return new Color(UnityEngine.Random.Range(0.2f,0.9f), UnityEngine.Random.Range(0.2f,0.9f), UnityEngine.Random.Range(0.2f,0.9f));
        }

        private void ExportJson()
        {
            string path = EditorUtility.SaveFilePanel("Export Event Log (JSON)", Application.dataPath, "event_log", "json");
            if (string.IsNullOrEmpty(path)) return;
            var export = new ExportPayload
            {
                version = 1,
                sessionId = Guid.NewGuid().ToString("N"),
                unityVersion = Application.unityVersion,
                projectName = Application.productName,
                captureStartTimeUtc = EventCapture.SessionStartUtc.ToString("o"),
                records = EventCapture.Enumerate().ToList()
            };
            var json = JsonUtility.ToJson(export, true);
            System.IO.File.WriteAllText(path, json);
            EditorUtility.RevealInFinder(path);
        }

        private void ExportCsv()
        {
            string path = EditorUtility.SaveFilePanel("Export Event Log (CSV)", Application.dataPath, "event_log", "csv");
            if (string.IsNullOrEmpty(path)) return;
            var rows = new List<string>();
            rows.Add("timeRealtime,gameTime,name,category,source,sourceType,listenersCount,durationAvgMs,exceptionsCount");
            foreach (var e in EventCapture.Enumerate())
            {
                int lc = e.listeners?.Count ?? 0;
                double avg = lc > 0 ? e.listeners.Average(l => l.durationMs) : 0;
                int exc = e.listeners?.Count(l => !string.IsNullOrEmpty(l.exception)) ?? 0;
                string line = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0},{1},\"{2}\",\"{3}\",\"{4}\",\"{5}\",{6},{7:F3},{8}",
                    e.timeRealtime, e.gameTime, e.name, e.category, e.sourceInfo.objectName, e.sourceInfo.typeName, lc, avg, exc);
                rows.Add(line);
            }
            System.IO.File.WriteAllLines(path, rows);
            EditorUtility.RevealInFinder(path);
        }

        [Serializable]
        private class ExportPayload
        {
            public int version;
            public string sessionId;
            public string unityVersion;
            public string projectName;
            public string captureStartTimeUtc;
            public List<EventRecord> records;
        }

        private void OnEventAppended(EventRecord rec)
        {
            // Watch highlight (future: panel pinning). For now, repaint is enough.
            // Breakpoint: pause when name contains pattern
            if (_breakOnMatch && !string.IsNullOrEmpty(_breakpointPattern))
            {
                if (!string.IsNullOrEmpty(rec.name) && rec.name.IndexOf(_breakpointPattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    EditorApplication.isPaused = true;
                    _selected = rec;
                }
            }
        }
    }
}
#endif


