using UnityEngine;
using TirexGame.Utils.EventCenter;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Demo script để test Vertical Timeline UX mới
    /// Tạo nhiều events với timing khác nhau để kiểm tra collision detection
    /// </summary>
    public class VerticalTimelineDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private float eventInterval = 0.5f;
        [SerializeField] private int eventsPerBurst = 5;
        [SerializeField] private bool enableLogs = true;
        
        #region Demo Events
        
        // Timeline test events with varied categories
        public struct TimelineTestEvent
        {
            public string TestType;
            public int EventIndex;
            public float Intensity;
            public string Category;
            
            public TimelineTestEvent(string type, int index, float intensity, string category = "Timeline")
            {
                TestType = type;
                EventIndex = index;
                Intensity = intensity;
                Category = category;
            }
        }
        
        // Collision test events with specific categories
        public struct CollisionTestEvent
        {
            public string Category;
            public Vector3 Position;
            public bool IsImportant;
            public string ActionType;
            
            public CollisionTestEvent(string category, Vector3 pos, bool important, string actionType = "Collision")
            {
                Category = category;
                Position = pos;
                IsImportant = important;
                ActionType = actionType;
            }
        }
        
        // Rapid fire events with burst patterns
        public struct RapidFireEvent
        {
            public int BurstId;
            public int SequenceNumber;
            public float TimeStamp;
            public string EventType;
            public string Category;
            
            public RapidFireEvent(int burstId, int seq, float timeStamp, string eventType, string category = "RapidFire")
            {
                BurstId = burstId;
                SequenceNumber = seq;
                TimeStamp = timeStamp;
                EventType = eventType;
                Category = category;
            }
        }
        
        // New event types for better visual diversity
        public struct PlayerActionEvent
        {
            public string Action;
            public Vector3 WorldPosition;
            public float Magnitude;
            public string Category;
            
            public PlayerActionEvent(string action, Vector3 pos, float magnitude)
            {
                Action = action;
                WorldPosition = pos;
                Magnitude = magnitude;
                Category = "Player";
            }
        }
        
        public struct SystemEvent
        {
            public string System;
            public string Message;
            public int Priority;
            public string Category;
            
            public SystemEvent(string system, string message, int priority)
            {
                System = system;
                Message = message;
                Priority = priority;
                Category = "System";
            }
        }
        
        public struct UIInteractionEvent
        {
            public string ElementName;
            public string InteractionType;
            public bool IsSuccessful;
            public string Category;
            
            public UIInteractionEvent(string element, string interaction, bool success)
            {
                ElementName = element;
                InteractionType = interaction;
                IsSuccessful = success;
                Category = "UI";
            }
        }
        
        #endregion
        
        private float lastEventTime;
        private int eventCounter = 0;
        
        private void Start()
        {
            if (autoStart)
            {
                Log("🎬 Vertical Timeline Demo started!");
                Log("📖 Mở Event Visualizer để xem timeline dọc mới!");
                Log("📖 Open Event Visualizer to see the new vertical timeline!");
                
                // Setup event listeners for all event types
                this.SubscribeWithCleanup<TimelineTestEvent>(OnTimelineTestEvent);
                this.SubscribeWithCleanup<CollisionTestEvent>(OnCollisionTestEvent);
                this.SubscribeWithCleanup<RapidFireEvent>(OnRapidFireEvent);
                this.SubscribeWithCleanup<PlayerActionEvent>(OnPlayerActionEvent);
                this.SubscribeWithCleanup<SystemEvent>(OnSystemEvent);
                this.SubscribeWithCleanup<UIInteractionEvent>(OnUIInteractionEvent);
                
                InvokeRepeating(nameof(GenerateTestEvents), 1f, eventInterval);
            }
        }
        
        private void GenerateTestEvents()
        {
            if (!EventSystem.IsInitialized) return;
            
            eventCounter++;
            
            // Generate different types of events with diverse categories
            switch (eventCounter % 8) // Expanded to 8 different patterns
            {
                case 0:
                    // Timeline test events with various categories
                    var categories = new[] { "Performance", "Analytics", "Debug", "Metrics" };
                    var category = categories[Random.Range(0, categories.Length)];
                    var testEvent = new TimelineTestEvent("Normal", eventCounter, Random.Range(0.1f, 1f), category);
                    EventSystem.Publish(testEvent);
                    Log($"📊 Published TimelineTestEvent #{eventCounter} - {category}");
                    break;
                    
                case 1:
                    // Collision test - multiple events at similar times
                    StartCoroutine(GenerateCollisionBurst());
                    break;
                    
                case 2:
                    // Physics collision events
                    var collisionEvent = new CollisionTestEvent("Physics", 
                        Random.insideUnitSphere * 5f, 
                        Random.Range(0, 100) < 30,
                        "Impact");
                    EventSystem.Publish(collisionEvent);
                    Log($"⚡ Published CollisionTestEvent #{eventCounter}");
                    break;
                    
                case 3:
                    // Rapid fire events to test collision system
                    StartCoroutine(GenerateRapidFireBurst());
                    break;
                    
                case 4:
                    // Player action events
                    var actions = new[] { "Jump", "Attack", "Defend", "Cast", "Move", "Interact" };
                    var action = actions[Random.Range(0, actions.Length)];
                    var playerEvent = new PlayerActionEvent(action, 
                        Random.insideUnitSphere * 10f, 
                        Random.Range(0.5f, 2f));
                    EventSystem.Publish(playerEvent);
                    Log($"🎮 Published PlayerActionEvent: {action}");
                    break;
                    
                case 5:
                    // System events
                    var systems = new[] { "Audio", "Renderer", "Network", "AI", "Memory" };
                    var system = systems[Random.Range(0, systems.Length)];
                    var messages = new[] { "Initialized", "Updated", "Warning", "Optimized", "Cached" };
                    var message = messages[Random.Range(0, messages.Length)];
                    var systemEvent = new SystemEvent(system, message, Random.Range(1, 5));
                    EventSystem.Publish(systemEvent);
                    Log($"🔧 Published SystemEvent: {system} - {message}");
                    break;
                    
                case 6:
                    // UI interaction events
                    var elements = new[] { "Button", "Slider", "Menu", "Panel", "Dialog", "Input" };
                    var interactions = new[] { "Click", "Hover", "Drag", "Focus", "Submit" };
                    var element = elements[Random.Range(0, elements.Length)];
                    var interaction = interactions[Random.Range(0, interactions.Length)];
                    var uiEvent = new UIInteractionEvent(element, interaction, Random.Range(0, 100) < 90);
                    EventSystem.Publish(uiEvent);
                    Log($"🖱️ Published UIInteractionEvent: {element} {interaction}");
                    break;
                    
                case 7:
                    // Mixed rapid events
                    StartCoroutine(GenerateMixedEventBurst());
                    break;
            }
        }
        
        private System.Collections.IEnumerator GenerateCollisionBurst()
        {
            int burstId = eventCounter;
            Log($"💥 Generating collision burst #{burstId} - {eventsPerBurst} events");
            
            for (int i = 0; i < eventsPerBurst; i++)
            {
                var categories = new[] { "UI", "Audio", "Physics", "Network", "AI" };
                var category = categories[Random.Range(0, categories.Length)];
                
                var collisionEvent = new CollisionTestEvent(category, 
                    Random.insideUnitSphere * 3f, 
                    i == 0, // First event is important
                    "CollisionBurst");
                    
                EventSystem.Publish(collisionEvent);
                
                // Small delay to create near-simultaneous events
                yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
            }
            
            Log($"✅ Collision burst #{burstId} completed");
        }
        
        private System.Collections.IEnumerator GenerateRapidFireBurst()
        {
            int burstId = eventCounter;
            int rapidCount = Random.Range(8, 15);
            Log($"🔥 Generating rapid fire burst #{burstId} - {rapidCount} events");
            
            for (int i = 0; i < rapidCount; i++)
            {
                var eventTypes = new[] { "Burst", "Sequence", "Chain", "Cascade", "Wave" };
                var eventType = eventTypes[Random.Range(0, eventTypes.Length)];
                var rapidEvent = new RapidFireEvent(burstId, i, Time.time, eventType, "RapidFire");
                EventSystem.Publish(rapidEvent);
                
                // Very small delays to test collision detection
                yield return new WaitForSeconds(Random.Range(0.005f, 0.02f));
            }
            
            Log($"🎯 Rapid fire burst #{burstId} completed with {rapidCount} events");
        }
        
        private System.Collections.IEnumerator GenerateMixedEventBurst()
        {
            int burstId = eventCounter;
            int mixedCount = Random.Range(6, 12);
            Log($"🌈 Generating mixed event burst #{burstId} - {mixedCount} diverse events");
            
            for (int i = 0; i < mixedCount; i++)
            {
                // Generate random event types for maximum collision testing
                switch (Random.Range(0, 6))
                {
                    case 0:
                        var playerActions = new[] { "Dash", "Block", "Heal", "Skill" };
                        var action = playerActions[Random.Range(0, playerActions.Length)];
                        EventSystem.Publish(new PlayerActionEvent(action, Random.insideUnitSphere * 5f, Random.Range(0.3f, 1.5f)));
                        break;
                        
                    case 1:
                        var systems = new[] { "Particle", "Lighting", "Sound", "Physics" };
                        var system = systems[Random.Range(0, systems.Length)];
                        EventSystem.Publish(new SystemEvent(system, "StateChange", Random.Range(1, 4)));
                        break;
                        
                    case 2:
                        var elements = new[] { "HUD", "Inventory", "Settings", "Chat" };
                        var element = elements[Random.Range(0, elements.Length)];
                        EventSystem.Publish(new UIInteractionEvent(element, "Update", true));
                        break;
                        
                    case 3:
                        EventSystem.Publish(new TimelineTestEvent("Mixed", i, Random.Range(0.2f, 0.8f), "Mixed"));
                        break;
                        
                    case 4:
                        var collCategories = new[] { "Projectile", "Environment", "Character" };
                        var category = collCategories[Random.Range(0, collCategories.Length)];
                        EventSystem.Publish(new CollisionTestEvent(category, Random.insideUnitSphere * 2f, false, "MixedBurst"));
                        break;
                        
                    case 5:
                        var rapidTypes = new[] { "Pulse", "Echo", "Resonance" };
                        var rapidType = rapidTypes[Random.Range(0, rapidTypes.Length)];
                        EventSystem.Publish(new RapidFireEvent(burstId, i, Time.time, rapidType, "Mixed"));
                        break;
                }
                
                // Very tight timing to stress test collision system
                yield return new WaitForSeconds(Random.Range(0.001f, 0.015f));
            }
            
            Log($"🌈 Mixed event burst #{burstId} completed with {mixedCount} diverse events");
        }
        
        #region Event Handlers
        
        private void OnTimelineTestEvent(TimelineTestEvent evt)
        {
            Log($"📊 TimelineTest: {evt.TestType} #{evt.EventIndex} (Intensity: {evt.Intensity:F2}) - {evt.Category}");
        }
        
        private void OnCollisionTestEvent(CollisionTestEvent evt)
        {
            string importance = evt.IsImportant ? "⭐IMPORTANT⭐" : "normal";
            Log($"⚡ {evt.ActionType}: {evt.Category} at {evt.Position} ({importance})");
        }
        
        private void OnRapidFireEvent(RapidFireEvent evt)
        {
            Log($"🔥 {evt.EventType}: Burst{evt.BurstId} Seq{evt.SequenceNumber} @ {evt.TimeStamp:F3}s - {evt.Category}");
        }
        
        private void OnPlayerActionEvent(PlayerActionEvent evt)
        {
            Log($"🎮 Player {evt.Action}: Position {evt.WorldPosition} (Magnitude: {evt.Magnitude:F1}) - {evt.Category}");
        }
        
        private void OnSystemEvent(SystemEvent evt)
        {
            string priority = evt.Priority >= 3 ? "HIGH" : "normal";
            Log($"🔧 System [{evt.System}]: {evt.Message} (Priority: {priority}) - {evt.Category}");
        }
        
        private void OnUIInteractionEvent(UIInteractionEvent evt)
        {
            string result = evt.IsSuccessful ? "✅" : "❌";
            Log($"🖱️ UI {evt.InteractionType}: {evt.ElementName} {result} - {evt.Category}");
        }
        
        #endregion
        
        #region Context Menu Controls
        
        [ContextMenu("Generate Timeline Events")]
        private void MenuGenerateTimelineEvents()
        {
            var categories = new[] { "Performance", "Analytics", "Debug" };
            for (int i = 0; i < 3; i++)
            {
                var category = categories[i % categories.Length];
                var evt = new TimelineTestEvent("Manual", i, Random.Range(0.5f, 1f), category);
                EventSystem.Publish(evt);
            }
            Log("📊 Generated 3 diverse timeline test events");
        }
        
        [ContextMenu("Generate Player Actions")]
        private void MenuGeneratePlayerActions()
        {
            var actions = new[] { "SuperJump", "PowerAttack", "Shield", "Teleport" };
            foreach (var action in actions)
            {
                var evt = new PlayerActionEvent(action, Random.insideUnitSphere * 8f, Random.Range(1f, 2f));
                EventSystem.Publish(evt);
            }
            Log("🎮 Generated diverse player action events");
        }
        
        [ContextMenu("Generate System Events")]
        private void MenuGenerateSystemEvents()
        {
            var systems = new[] { "Renderer", "Audio", "Network", "AI" };
            var messages = new[] { "Optimized", "Loaded", "Connected", "Thinking" };
            for (int i = 0; i < systems.Length; i++)
            {
                var evt = new SystemEvent(systems[i], messages[i], Random.Range(1, 5));
                EventSystem.Publish(evt);
            }
            Log("🔧 Generated diverse system events");
        }
        
        [ContextMenu("Generate UI Events")]
        private void MenuGenerateUIEvents()
        {
            var elements = new[] { "MainMenu", "InventoryPanel", "SettingsDialog", "ChatWindow" };
            var interactions = new[] { "Open", "Close", "Resize", "Focus" };
            for (int i = 0; i < elements.Length; i++)
            {
                var evt = new UIInteractionEvent(elements[i], interactions[i], Random.Range(0, 100) < 85);
                EventSystem.Publish(evt);
            }
            Log("🖱️ Generated diverse UI interaction events");
        }
        
        [ContextMenu("Generate Collision Burst")]
        private void MenuGenerateCollisionBurst()
        {
            StartCoroutine(GenerateCollisionBurst());
        }
        
        [ContextMenu("Generate Rapid Fire")]
        private void MenuGenerateRapidFire()
        {
            StartCoroutine(GenerateRapidFireBurst());
        }
        
        [ContextMenu("Stop Auto Generation")]
        private void MenuStopAuto()
        {
            CancelInvoke(nameof(GenerateTestEvents));
            Log("⏹️ Auto event generation stopped");
        }
        
        [ContextMenu("Start Auto Generation")]
        private void MenuStartAuto()
        {
            CancelInvoke(nameof(GenerateTestEvents));
            InvokeRepeating(nameof(GenerateTestEvents), 0.5f, eventInterval);
            Log("▶️ Auto event generation started");
        }
        
        [ContextMenu("Test Horizontal Scrolling")]
        private void MenuTestHorizontalScrolling()
        {
            Log("📏 Testing horizontal scrolling with collision events");
            
            // Generate many events at same time to force multiple layers
            for (int i = 0; i < 15; i++)
            {
                var evt = new TimelineTestEvent($"Layer{i}", i, Random.Range(0.1f, 1f), $"Category{i % 3}");
                EventSystem.Publish(evt);
                
                // Very small delay to create collision
                System.Threading.Thread.Sleep(5);
            }
            
            Log("📏 Horizontal scrolling test completed - check for layers and scroll!");
        }
        
        #endregion
        
        #region GUI Demo
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 400));
            
            var headerStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                fixedHeight = 40
            };
            
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fixedHeight = 35
            };
            
            GUILayout.Label("🎬 Vertical Timeline Demo", headerStyle);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("📊 Generate Timeline Events", buttonStyle))
            {
                MenuGenerateTimelineEvents();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("💥 Generate Collision Burst", buttonStyle))
            {
                MenuGenerateCollisionBurst();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🔥 Generate Rapid Fire", buttonStyle))
            {
                MenuGenerateRapidFire();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🎮 Generate Player Actions", buttonStyle))
            {
                MenuGeneratePlayerActions();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🔧 Generate System Events", buttonStyle))
            {
                MenuGenerateSystemEvents();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🖱️ Generate UI Events", buttonStyle))
            {
                MenuGenerateUIEvents();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🌈 Generate Mixed Burst", buttonStyle))
            {
                StartCoroutine(GenerateMixedEventBurst());
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("📏 Test Horizontal Scroll", buttonStyle))
            {
                MenuTestHorizontalScrolling();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("⏹️ Stop Auto Generation", buttonStyle))
            {
                MenuStopAuto();
            }
            
            if (GUILayout.Button("▶️ Start Auto Generation", buttonStyle))
            {
                MenuStartAuto();
            }
            
            GUILayout.Space(10);
            
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true
            };
            
            GUILayout.Label("📖 Mở Event Visualizer:\nTirexGame > Event Center > Event Visualizer", labelStyle);
            GUILayout.Label($"Events Generated: {eventCounter}", labelStyle);
            
            GUILayout.EndArea();
        }
        
        #endregion
        
        private void Log(string message)
        {
            if (enableLogs)
            {
                Debug.Log($"[VerticalTimelineDemo] {message}");
            }
        }
    }
}