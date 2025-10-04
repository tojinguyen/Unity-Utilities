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
        
        // UI state
        private bool _showWatchList = true;
        private bool _showBreakpoints = true;
        private bool _showChannels = true;

        // Vertical Timeline Layout Data
        private struct EventLayout
        {
            public EventRecord eventRecord;
            public Rect rect;
            public int layer; // 0 = closest to timeline axis, 1+ = further right
            public Vector2 connectionPoint; // Point on timeline axis for connection line
        }

        // Custom styles for better contrast
        private GUIStyle _timelineLabelStyle;
        private GUIStyle _eventLabelStyle;
        private GUIStyle _timeLabelStyle;
        
        // Color palette for diverse and appealing colors
        private static readonly Color[] ColorPalette = new Color[]
        {
            new Color(0.9f, 0.3f, 0.3f),  // Soft Red
            new Color(0.3f, 0.7f, 0.9f),  // Sky Blue  
            new Color(0.4f, 0.8f, 0.4f),  // Fresh Green
            new Color(0.9f, 0.6f, 0.2f),  // Orange
            new Color(0.7f, 0.4f, 0.8f),  // Purple
            new Color(0.9f, 0.8f, 0.3f),  // Yellow
            new Color(0.5f, 0.8f, 0.8f),  // Cyan
            new Color(0.8f, 0.5f, 0.6f),  // Pink
            new Color(0.6f, 0.7f, 0.4f),  // Olive Green
            new Color(0.7f, 0.6f, 0.9f),  // Lavender
            new Color(0.9f, 0.7f, 0.5f),  // Peach
            new Color(0.4f, 0.6f, 0.8f),  // Steel Blue
            new Color(0.8f, 0.3f, 0.5f),  // Deep Pink
            new Color(0.5f, 0.9f, 0.4f),  // Lime Green
            new Color(0.9f, 0.5f, 0.3f),  // Coral
            new Color(0.4f, 0.8f, 0.9f),  // Light Blue
            new Color(0.8f, 0.8f, 0.4f),  // Gold
            new Color(0.6f, 0.4f, 0.9f),  // Violet
            new Color(0.9f, 0.6f, 0.7f),  // Rose
            new Color(0.4f, 0.9f, 0.8f)   // Mint
        };
        
        // Event counter for simple color cycling (1 event = 1 color change)
        private static int _eventColorCounter = 0;
        
        private void InitializeStyles()
        {
            if (_timelineLabelStyle == null)
            {
                _timelineLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(0.9f, 0.9f, 0.9f, 1f) }, // Light gray for good contrast
                    fontSize = 10,
                    fontStyle = FontStyle.Normal
                };
            }
            
            if (_eventLabelStyle == null)
            {
                _eventLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = Color.black }, // Black text for event labels
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }
            
            if (_timeLabelStyle == null)
            {
                _timeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(1f, 1f, 1f, 0.9f) }, // High contrast white
                    fontSize = 9,
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }

        [MenuItem("TirexGame/Event Center/Event Visualizer")] 
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
                double deltaTime = EditorApplication.timeSinceStartup - _lastUpdateTime;
                _replayPlayhead += deltaTime;
                
                // Auto-pause at end
                var events = EventCapture.Enumerate().ToList();
                if (events.Any())
                {
                    double maxT = events.Max(e => e.timeRealtime);
                    if (_replayPlayhead >= maxT)
                    {
                        _replayPlayhead = maxT;
                        _replayPlaying = false;
                    }
                }
                
                Repaint();
            }
            _lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        private double _lastUpdateTime;

        private void OnGUI()
        {
            try
            {
                DrawToolbar();

                var rect = position;
                float leftWidth = 250f; // Tăng từ 200f để có nhiều không gian hơn
                float rightWidth = 320f;
                float toolbarHeight = 22f;

                var body = new Rect(0, toolbarHeight, rect.width, rect.height - toolbarHeight);
                var left = new Rect(body.x, body.y, leftWidth, body.height);
                var center = new Rect(left.xMax, body.y, body.width - leftWidth - rightWidth, body.height);
                var right = new Rect(center.xMax, body.y, rightWidth, body.height);

                GUILayout.BeginArea(left, EditorStyles.helpBox);
                _leftScroll = GUILayout.BeginScrollView(_leftScroll);
                DrawChannelList();
                GUILayout.Space(12);
                DrawWatchAndBreakpoints();
                GUILayout.FlexibleSpace(); // Đẩy nội dung lên trên, để scroll tự nhiên hơn
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
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventVisualizerWindow] OnGUI Exception: {ex.Message}");
                Debug.LogError($"[EventVisualizerWindow] Stack: {ex.StackTrace}");
                // Force repaint to try to recover
                Repaint();
            }
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
                
                // Time range filter
                GUILayout.Space(8);
                GUILayout.Label("Time:", GUILayout.Width(32));
                float min = (float)_timeMin;
                float max = (float)_timeMax;
                EditorGUILayout.MinMaxSlider(ref min, ref max, 0f, 60f, GUILayout.Width(100));
                _timeMin = min;
                _timeMax = max;
                if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(40)))
                {
                    _timeMin = 0;
                    _timeMax = 0;
                }
                
                // Preset filters
                GUILayout.Space(8);
                if (GUILayout.Button("Errors", EditorStyles.toolbarButton, GUILayout.Width(45)))
                {
                    _filterListener = "exception";
                    _searchText = "";
                }
                if (GUILayout.Button("Player", EditorStyles.toolbarButton, GUILayout.Width(45)))
                {
                    _filterCategory = "Player";
                    _searchText = "";
                }
                if (GUILayout.Button("UI", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    _filterCategory = "UI";
                    _searchText = "";
                }
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(40)))
                {
                    _searchText = "";
                    _filterCategory = "";
                    _filterSource = "";
                    _filterListener = "";
                    _timeMin = 0;
                    _timeMax = 0;
                }

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
                    if (GUILayout.Button(_replayPlaying ? "Pause" : "Play", EditorStyles.toolbarButton, GUILayout.Width(50)))
                    {
                        _replayPlaying = !_replayPlaying;
                        if (_replayPlaying && _replayPlayhead <= 0)
                        {
                            var events = EventCapture.Enumerate().ToList();
                            if (events.Any())
                            {
                                _replayPlayhead = events.Min(e => e.timeRealtime);
                            }
                        }
                    }
                    if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(40)))
                    {
                        _replayPlaying = false;
                        var events = EventCapture.Enumerate().ToList();
                        if (events.Any())
                        {
                            _replayPlayhead = events.Min(e => e.timeRealtime);
                        }
                        else
                        {
                            _replayPlayhead = 0;
                        }
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
                
                GUILayout.Space(8);
                
                // Color cycling controls
                GUILayout.Label($"Color: {_eventColorCounter % ColorPalette.Length + 1}/{ColorPalette.Length}", GUILayout.Width(80));
                if (GUILayout.Button("Reset Colors", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    ResetColorCycling();
                }
                
                GUILayout.Space(8);
                
                // Layer information for horizontal scrolling
                var currentEvents = ApplyFilters(EventCapture.Enumerate())?.Where(e => e != null).ToList() ?? new List<EventRecord>();
                if (currentEvents.Any())
                {
                    var tempRect = new Rect(0, 0, 800, 600);
                    var tempLayouts = CalculateEventLayout(currentEvents, tempRect, 0, 10);
                    int maxLayer = tempLayouts.Any() ? tempLayouts.Max(l => l.layer) + 1 : 0;
                    
                    GUILayout.Label($"Layers: {maxLayer}", GUILayout.Width(60));
                    if (maxLayer > 3) // Show scroll hint when many layers
                    {
                        GUILayout.Label("→ Scroll →", EditorStyles.miniLabel, GUILayout.Width(50));
                    }
                }
            }
        }

        private void DrawChannelList()
        {
            try
            {
                _showChannels = EditorGUILayout.Foldout(_showChannels, "Channels", true, EditorStyles.foldoutHeader);
                if (_showChannels)
                {
                    var allEvents = EventCapture.Enumerate()?.Where(e => e != null).ToList() ?? new List<EventRecord>();
                    var grouped = allEvents.GroupBy(e => e?.category ?? "Uncategorized").OrderBy(g => g.Key);
                    
                    if (!grouped.Any())
                    {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField("No events captured", EditorStyles.centeredGreyMiniLabel);
                            EditorGUILayout.LabelField($"Buffer count: {EventCapture.Count}", EditorStyles.centeredGreyMiniLabel);
                            EditorGUILayout.LabelField($"Recording: {EventCapture.IsRecording}", EditorStyles.centeredGreyMiniLabel);
                            EditorGUILayout.LabelField($"Paused: {EventCapture.IsPaused}", EditorStyles.centeredGreyMiniLabel);
                        }
                    }
                    else
                    {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            foreach (var g in grouped)
                            {
                                if (!_visibleChannels.ContainsKey(g.Key)) _visibleChannels[g.Key] = true;
                                _visibleChannels[g.Key] = EditorGUILayout.ToggleLeft($"{g.Key} ({g.Count()})", _visibleChannels[g.Key]);
                            }
                            
                            GUILayout.Space(4);
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("All", GUILayout.Width(30)))
                                {
                                    foreach (var key in _visibleChannels.Keys.ToList())
                                    {
                                        _visibleChannels[key] = true;
                                    }
                                }
                                if (GUILayout.Button("None", GUILayout.Width(35)))
                                {
                                    foreach (var key in _visibleChannels.Keys.ToList())
                                    {
                                        _visibleChannels[key] = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventVisualizerWindow] DrawChannelList Exception: {ex.Message}");
                EditorGUILayout.LabelField("Error drawing channels", EditorStyles.centeredGreyMiniLabel);
            }
        }

        private void DrawTimelineArea(Rect rect)
        {
            try
            {
                var events = ApplyFilters(EventCapture.Enumerate());
                var eventsList = events?.Where(e => e != null).ToList() ?? new List<EventRecord>();
            
                if (!eventsList.Any())
                {
                    _timelineScroll = GUILayout.BeginScrollView(_timelineScroll, GUILayout.ExpandHeight(true));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("No events captured", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndScrollView();
                    return;
                }

                // Calculate time bounds
                double minT = eventsList.Min(e => e.timeRealtime);
                double maxT = eventsList.Max(e => e.timeRealtime);
                double span = Math.Max(0.1, maxT - minT);
                _viewStartTime = minT;
                _viewEndTime = maxT;

                // Pre-calculate event layouts to determine required width
                var tempRect = new Rect(0, 0, rect.width, (float)(span * _pixelsPerSecond) + 200f);
                var eventLayouts = CalculateEventLayout(eventsList, tempRect, minT, maxT);
                
                // Calculate required width based on maximum layer
                int maxLayer = eventLayouts.Any() ? eventLayouts.Max(l => l.layer) : 0;
                const float timelineAxisX = 60f;
                const float layerOffset = 135f;
                const float eventWidth = 120f;
                const float padding = 50f;
                
                float requiredWidth = timelineAxisX + 20f + (maxLayer + 1) * layerOffset + eventWidth + padding;
                float actualWidth = Math.Max(rect.width, requiredWidth);

                // Vertical timeline: Height is based on time span
                float totalTimelineHeight = (float)(span * _pixelsPerSecond) + 200f;
                
                // Start scroll view with both horizontal and vertical scrolling
                _timelineScroll = GUILayout.BeginScrollView(_timelineScroll, 
                    GUILayout.ExpandHeight(true), 
                    GUILayout.ExpandWidth(true));
                
                var timelineRect = GUILayoutUtility.GetRect(actualWidth, totalTimelineHeight);
                
                // Draw vertical timeline
                DrawVerticalTimelineGrid(timelineRect, minT, maxT);
                
                // Recalculate layouts with actual rect
                eventLayouts = CalculateEventLayout(eventsList, timelineRect, minT, maxT);
                
                // Draw events
                DrawEventsWithConnections(eventLayouts, timelineRect);
                
                // Draw playhead in replay mode
                if (_replayMode && eventsList.Any())
                {
                    DrawVerticalPlayhead(timelineRect, minT, maxT);
                }

                GUILayout.EndScrollView();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventVisualizerWindow] DrawTimelineArea Exception: {ex.Message}");
                try
                {
                    GUILayout.EndScrollView();
                }
                catch { }
                
                _timelineScroll = GUILayout.BeginScrollView(_timelineScroll, GUILayout.ExpandHeight(true));
                GUILayout.FlexibleSpace();
                GUILayout.Label("Error drawing timeline", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndScrollView();
            }
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

        // ✨ Vertical Timeline Methods - New UX Implementation ✨
        
        private void DrawVerticalTimelineGrid(Rect canvas, double minT, double maxT)
        {
            InitializeStyles();
            
            // Background - darker for better contrast
            EditorGUI.DrawRect(canvas, new Color(0.08f, 0.08f, 0.08f, 1f));
            
            // Timeline axis position (left side of canvas)
            float timelineAxisX = canvas.x + 60f;
            
            // Draw main vertical timeline axis
            Handles.color = new Color(1f, 1f, 1f, 0.9f); // Brighter axis
            Handles.DrawLine(new Vector2(timelineAxisX, canvas.y), new Vector2(timelineAxisX, canvas.yMax));
            
            // Draw time markers along the axis
            Handles.color = new Color(1, 1, 1, 0.4f); // Slightly brighter markers
            float secondsStep = Mathf.Max(0.1f, 1f * (100f / _pixelsPerSecond));
            double start = Math.Floor(minT / secondsStep) * secondsStep;
            
            for (double t = start; t < maxT + 2 * secondsStep; t += secondsStep)
            {
                float y = canvas.y + (float)((t - minT) * _pixelsPerSecond);
                if (y >= canvas.y && y <= canvas.yMax)
                {
                    // Time marker tick - longer for better visibility
                    Handles.DrawLine(new Vector2(timelineAxisX - 8, y), new Vector2(timelineAxisX + 8, y));
                    
                    // Time label with better contrast
                    var labelRect = new Rect(canvas.x + 2, y - 8, 55, 16);
                    GUI.Label(labelRect, t.ToString("F2") + "s", _timelineLabelStyle);
                    
                    // Optional horizontal grid lines
                    Handles.color = new Color(1, 1, 1, 0.08f); // Subtle grid lines
                    Handles.DrawLine(new Vector2(timelineAxisX, y), new Vector2(canvas.xMax, y));
                    Handles.color = new Color(1, 1, 1, 0.4f);
                }
            }
        }
        
        private List<EventLayout> CalculateEventLayout(List<EventRecord> events, Rect canvas, double minT, double maxT)
        {
            var layouts = new List<EventLayout>();
            var eventsByTime = events.OrderBy(e => e.timeRealtime).ToList();
            
            const float eventWidth = 120f;
            const float eventHeight = 24f;
            const float layerOffset = 135f; // Horizontal spacing between layers
            const float timelineAxisX = 60f;
            const float minVerticalSpacing = 6f; // Minimum vertical spacing between events
            const double timeCollisionThreshold = 0.1; // 100ms threshold for collision detection
            
            // Advanced collision detection with both time and space considerations
            var occupiedRegions = new List<Rect>(); // Track occupied screen space
            
            foreach (var ev in eventsByTime)
            {
                float baseY = canvas.y + (float)((ev.timeRealtime - minT) * _pixelsPerSecond);
                int bestLayer = 0;
                bool foundValidPosition = false;
                
                // Try each layer until we find one without collisions
                for (int layer = 0; layer < 10 && !foundValidPosition; layer++) // Max 10 layers
                {
                    float x = canvas.x + timelineAxisX + 20f + (layer * layerOffset);
                    float y = baseY;
                    
                    // Check for collisions with existing events in both time and space
                    bool hasCollision = false;
                    
                    // Create proposed rect for this event
                    var proposedRect = new Rect(x, y, eventWidth, eventHeight);
                    
                    // Check collision with all existing events
                    foreach (var existingRegion in occupiedRegions)
                    {
                        // Check both spatial overlap and time proximity
                        if (proposedRect.Overlaps(existingRegion))
                        {
                            hasCollision = true;
                            break;
                        }
                    }
                    
                    // Also check time-based collision with events in the same layer
                    foreach (var existingLayout in layouts.Where(l => l.layer == layer))
                    {
                        double timeDiff = Math.Abs(existingLayout.eventRecord.timeRealtime - ev.timeRealtime);
                        if (timeDiff <= timeCollisionThreshold)
                        {
                            hasCollision = true;
                            break;
                        }
                    }
                    
                    if (!hasCollision)
                    {
                        bestLayer = layer;
                        foundValidPosition = true;
                        
                        // Add some padding to prevent tight packing
                        var occupiedRect = new Rect(x - 2, y - minVerticalSpacing/2, 
                                                   eventWidth + 4, eventHeight + minVerticalSpacing);
                        occupiedRegions.Add(occupiedRect);
                    }
                }
                
                // If we couldn't find a good position, use the best layer we found
                if (!foundValidPosition)
                {
                    bestLayer = layouts.Where(l => Math.Abs(l.eventRecord.timeRealtime - ev.timeRealtime) <= timeCollisionThreshold)
                                     .Select(l => l.layer)
                                     .DefaultIfEmpty(-1)
                                     .Max() + 1;
                }
                
                float finalX = canvas.x + timelineAxisX + 20f + (bestLayer * layerOffset);
                float finalY = baseY;
                
                var eventRect = new Rect(finalX, finalY, eventWidth, eventHeight);
                var connectionPoint = new Vector2(canvas.x + timelineAxisX, baseY);
                
                layouts.Add(new EventLayout
                {
                    eventRecord = ev,
                    rect = eventRect,
                    layer = bestLayer,
                    connectionPoint = connectionPoint
                });
                
                // Add final occupied region
                var finalOccupiedRect = new Rect(finalX - 2, finalY - minVerticalSpacing/2, 
                                               eventWidth + 4, eventHeight + minVerticalSpacing);
                occupiedRegions.Add(finalOccupiedRect);
            }
            
            return layouts;
        }
        
        private void DrawEventsWithConnections(List<EventLayout> eventLayouts, Rect canvas)
        {
            foreach (var layout in eventLayouts)
            {
                var ev = layout.eventRecord;
                var rect = layout.rect;
                
                // Skip if event is filtered out by channel visibility
                if (_visibleChannels.TryGetValue(ev.category ?? "Uncategorized", out bool visible) && !visible)
                    continue;
                
                // Force color regeneration for simple cycling system
                ev.cachedColor = GetColorFor(ev);
                
                // Check if watched
                bool isWatched = _watchList.Any(w => !string.IsNullOrEmpty(w) && !string.IsNullOrEmpty(ev.name) && 
                                                     ev.name.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0);
                
                // Draw event box with better colors
                Color bgColor = ev.cachedColor;
                // Ensure background is bright enough for dark text
                float brightness = bgColor.r * 0.299f + bgColor.g * 0.587f + bgColor.b * 0.114f;
                if (brightness < 0.5f)
                {
                    // Lighten dark colors for better contrast
                    bgColor = Color.Lerp(bgColor, Color.white, 0.4f);
                }
                bgColor.a = 0.9f;
                
                if (isWatched)
                {
                    bgColor = Color.Lerp(bgColor, new Color(1f, 0.6f, 0.6f), 0.4f); // Light red for watched
                }
                
                EditorGUI.DrawRect(rect, bgColor);
                
                // Draw border for selected events
                if (_selected == ev)
                {
                    var borderColor = new Color(1f, 0.8f, 0f, 1f); // Golden border
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 2), borderColor);
                    EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2, rect.width, 2), borderColor);
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, 2, rect.height), borderColor);
                    EditorGUI.DrawRect(new Rect(rect.xMax - 2, rect.y, 2, rect.height), borderColor);
                }
                
                // Draw event label with high contrast
                var labelRect = new Rect(rect.x + 4, rect.y + 2, rect.width - 8, rect.height - 4);
                GUI.Label(labelRect, ev.name, _eventLabelStyle);
                
                // Handle selection
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    _selected = ev;
                    Repaint();
                    Event.current.Use();
                }
                
                // Update cached rect for other systems
                ev.lastDrawRect = rect;
            }
        }
        
        private void DrawVerticalPlayhead(Rect canvas, double minT, double maxT)
        {
            float playheadY = canvas.y + (float)((_replayPlayhead - minT) * _pixelsPerSecond);
            if (playheadY >= canvas.y && playheadY <= canvas.yMax)
            {
                // Draw horizontal playhead line
                Handles.color = Color.red;
                Handles.DrawLine(new Vector2(canvas.x, playheadY), new Vector2(canvas.xMax, playheadY));
                
                // Draw playhead indicator on timeline axis
                float timelineAxisX = canvas.x + 60f;
                var playheadRect = new Rect(timelineAxisX - 8, playheadY - 4, 16, 8);
                EditorGUI.DrawRect(playheadRect, Color.red);
                
                // Scrubbing interaction
                var interactionRect = new Rect(canvas.x, playheadY - 5, canvas.width, 10);
                if (Event.current.type == EventType.MouseDown && interactionRect.Contains(Event.current.mousePosition))
                {
                    _replayPlaying = false;
                    Event.current.Use();
                }
                if (Event.current.type == EventType.MouseDrag && Vector2.Distance(Event.current.mousePosition, new Vector2(Event.current.mousePosition.x, playheadY)) < 20)
                {
                    _replayPlayhead = minT + (Event.current.mousePosition.y - canvas.y) / _pixelsPerSecond;
                    _replayPlayhead = Math.Max(minT, Math.Min(maxT, _replayPlayhead));
                    Repaint();
                    Event.current.Use();
                }
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

            // Event basic info with proper text wrapping
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Name", _selected.name, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Category", _selected.category, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Time (realtime)", _selected.timeRealtime.ToString("F3") + "s");
                EditorGUILayout.LabelField("Game Time", _selected.gameTime.ToString("F3") + "s");
            }

            GUILayout.Space(6);
            GUILayout.Label("Source", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Object", _selected.sourceInfo.objectName, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Type", _selected.sourceInfo.typeName, EditorStyles.wordWrappedLabel);
                if (!string.IsNullOrEmpty(_selected.sourceInfo.hierarchyPath))
                {
                    EditorGUILayout.LabelField("Path", _selected.sourceInfo.hierarchyPath, EditorStyles.wordWrappedLabel);
                }
            }

            GUILayout.Space(6);
            GUILayout.Label("Payload", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var payloadText = _selected.payloadPreview ?? string.Empty;
                var style = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true
                };
                EditorGUILayout.TextArea(payloadText, style, GUILayout.MinHeight(60), GUILayout.ExpandHeight(true));
            }

            GUILayout.Space(6);
            GUILayout.Label("Listeners", EditorStyles.miniBoldLabel);
            if (_selected.listeners == null || _selected.listeners.Count == 0)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label("No listeners captured", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.HelpBox("Listener information may not be available for this event type or the event was published before listener tracking was implemented.", MessageType.Info);
                }
            }
            else
            {
                foreach (var l in _selected.listeners)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Name", l.name, EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Target", l.targetInfo.objectName + " (" + l.targetInfo.typeName + ")", EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Duration", l.durationMs.ToString("F3") + " ms");
                        if (!string.IsNullOrEmpty(l.exception))
                        {
                            EditorGUILayout.LabelField("Exception", l.exception, EditorStyles.wordWrappedLabel);
                        }
                    }
                }
            }
        }

        private void DrawWatchAndBreakpoints()
        {
            try
            {
                // Watch List with foldout
                _showWatchList = EditorGUILayout.Foldout(_showWatchList, "Watch List", true, EditorStyles.foldoutHeader);
                if (_showWatchList)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        if (_watchList.Count == 0)
                        {
                            EditorGUILayout.LabelField("No watch patterns", EditorStyles.centeredGreyMiniLabel);
                        }
                        else
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
                        }
                        
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("+ Add Watch")) 
                            {
                                _watchList.Add("");
                            }
                            if (GUILayout.Button("Clear All", GUILayout.Width(70)) && _watchList.Count > 0) 
                            {
                                _watchList.Clear();
                            }
                        }
                    }
                }

                GUILayout.Space(6);
                
                // Breakpoints with foldout
                _showBreakpoints = EditorGUILayout.Foldout(_showBreakpoints, "Breakpoints", true, EditorStyles.foldoutHeader);
                if (_showBreakpoints)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        _breakOnMatch = EditorGUILayout.ToggleLeft("Pause on match", _breakOnMatch);
                        GUI.enabled = _breakOnMatch;
                        _breakpointPattern = EditorGUILayout.TextField("Event Name Contains", _breakpointPattern);
                        GUI.enabled = true;
                        
                        if (_breakOnMatch && !string.IsNullOrEmpty(_breakpointPattern))
                        {
                            EditorGUILayout.HelpBox($"Will pause when event name contains: '{_breakpointPattern}'", MessageType.Info);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventVisualizerWindow] DrawWatchAndBreakpoints Exception: {ex.Message}");
                EditorGUILayout.LabelField("Error drawing watch/breakpoints", EditorStyles.centeredGreyMiniLabel);
            }
        }

        private IEnumerable<EventRecord> ApplyFilters(IEnumerable<EventRecord> events)
        {
            var e = events?.Where(x => x != null) ?? Enumerable.Empty<EventRecord>();
            if (!string.IsNullOrEmpty(_searchText))
            {
                StringComparison cmp = _caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                e = e.Where(x => x != null && !string.IsNullOrEmpty(x.name) && x.name.IndexOf(_searchText, cmp) >= 0);
            }
            if (!string.IsNullOrEmpty(_filterCategory))
            {
                StringComparison cmp = _caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                e = e.Where(x => x != null && !string.IsNullOrEmpty(x.category) && x.category.IndexOf(_filterCategory, cmp) >= 0);
            }
            if (!string.IsNullOrEmpty(_filterSource))
            {
                StringComparison cmp = _caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                e = e.Where(x => x != null && ((!string.IsNullOrEmpty(x.sourceInfo?.objectName) && x.sourceInfo.objectName.IndexOf(_filterSource, cmp) >= 0)
                                 || (!string.IsNullOrEmpty(x.sourceInfo?.typeName) && x.sourceInfo.typeName.IndexOf(_filterSource, cmp) >= 0)));
            }
            if (!string.IsNullOrEmpty(_filterListener))
            {
                StringComparison cmp = _caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                e = e.Where(x => x != null && x.listeners != null && x.listeners.Any(l => l != null && !string.IsNullOrEmpty(l.name) && l.name.IndexOf(_filterListener, cmp) >= 0));
            }
            if (_timeMax > _timeMin)
            {
                e = e.Where(x => x != null && x.timeRealtime >= _timeMin && x.timeRealtime <= _timeMax);
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
            // Config-driven colors with fallback to simple cycling
            if (_config != null)
            {
                var cc = _config.GetChannelColor(ev.category ?? "Uncategorized");
                if (cc.a > 0f) return EnsureGoodContrast(cc);
            }
            
            // Simple strategy: just cycle through colors for each event
            _eventColorCounter++;
            int colorIndex = _eventColorCounter % ColorPalette.Length;
            
            Color baseColor = ColorPalette[colorIndex];
            
            // Add very subtle variation based on event ID to avoid identical colors
            UnityEngine.Random.InitState(ev.id.GetHashCode());
            float hueVariation = UnityEngine.Random.Range(-0.05f, 0.05f);
            
            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            h = Mathf.Repeat(h + hueVariation, 1f);
            
            Color finalColor = Color.HSVToRGB(h, s, v);
            return EnsureGoodContrast(finalColor);
        }
        
        private Color EnsureGoodContrast(Color color)
        {
            // Ensure minimum brightness for text readability
            float brightness = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
            if (brightness < 0.6f)
            {
                color = Color.Lerp(color, Color.white, 0.3f);
            }
            
            // Ensure the color is vibrant enough
            Color.RGBToHSV(color, out float h, out float s, out float v);
            if (s < 0.4f) s = 0.4f; // Minimum saturation
            if (v < 0.7f) v = 0.7f; // Minimum brightness
            
            return Color.HSVToRGB(h, s, v);
        }
        
        // Method to reset color cycling for testing
        private void ResetColorCycling()
        {
            _eventColorCounter = 0;
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


