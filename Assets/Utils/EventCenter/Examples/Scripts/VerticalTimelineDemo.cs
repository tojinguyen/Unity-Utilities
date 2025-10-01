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
        
        public struct TimelineTestEvent
        {
            public string TestType;
            public int EventIndex;
            public float Intensity;
            
            public TimelineTestEvent(string type, int index, float intensity)
            {
                TestType = type;
                EventIndex = index;
                Intensity = intensity;
            }
        }
        
        public struct CollisionTestEvent
        {
            public string Category;
            public Vector3 Position;
            public bool IsImportant;
            
            public CollisionTestEvent(string category, Vector3 pos, bool important)
            {
                Category = category;
                Position = pos;
                IsImportant = important;
            }
        }
        
        public struct RapidFireEvent
        {
            public int BurstId;
            public int SequenceNumber;
            public float TimeStamp;
            
            public RapidFireEvent(int burstId, int seq, float timeStamp)
            {
                BurstId = burstId;
                SequenceNumber = seq;
                TimeStamp = timeStamp;
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
                
                // Setup event listeners
                this.SubscribeWithCleanup<TimelineTestEvent>(OnTimelineTestEvent);
                this.SubscribeWithCleanup<CollisionTestEvent>(OnCollisionTestEvent);
                this.SubscribeWithCleanup<RapidFireEvent>(OnRapidFireEvent);
                
                InvokeRepeating(nameof(GenerateTestEvents), 1f, eventInterval);
            }
        }
        
        private void GenerateTestEvents()
        {
            if (!EventSystem.IsInitialized) return;
            
            eventCounter++;
            
            // Generate different types of events to test timeline layout
            switch (eventCounter % 4)
            {
                case 0:
                    // Single timeline test event
                    var testEvent = new TimelineTestEvent("Normal", eventCounter, Random.Range(0.1f, 1f));
                    EventSystem.Publish(testEvent);
                    Log($"📊 Published TimelineTestEvent #{eventCounter}");
                    break;
                    
                case 1:
                    // Collision test - multiple events at similar times
                    StartCoroutine(GenerateCollisionBurst());
                    break;
                    
                case 2:
                    // Different category
                    var collisionEvent = new CollisionTestEvent("Physics", 
                        Random.insideUnitSphere * 5f, 
                        Random.Range(0, 100) < 30);
                    EventSystem.Publish(collisionEvent);
                    Log($"⚡ Published CollisionTestEvent #{eventCounter}");
                    break;
                    
                case 3:
                    // Rapid fire events to test collision system
                    StartCoroutine(GenerateRapidFireBurst());
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
                    i == 0); // First event is important
                    
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
                var rapidEvent = new RapidFireEvent(burstId, i, Time.time);
                EventSystem.Publish(rapidEvent);
                
                // Very small delays to test collision detection
                yield return new WaitForSeconds(Random.Range(0.005f, 0.02f));
            }
            
            Log($"🎯 Rapid fire burst #{burstId} completed with {rapidCount} events");
        }
        
        #region Event Handlers
        
        private void OnTimelineTestEvent(TimelineTestEvent evt)
        {
            Log($"📊 TimelineTest: {evt.TestType} #{evt.EventIndex} (Intensity: {evt.Intensity:F2})");
        }
        
        private void OnCollisionTestEvent(CollisionTestEvent evt)
        {
            string importance = evt.IsImportant ? "⭐IMPORTANT⭐" : "normal";
            Log($"⚡ Collision: {evt.Category} at {evt.Position} ({importance})");
        }
        
        private void OnRapidFireEvent(RapidFireEvent evt)
        {
            Log($"🔥 RapidFire: Burst{evt.BurstId} Seq{evt.SequenceNumber} @ {evt.TimeStamp:F3}s");
        }
        
        #endregion
        
        #region Context Menu Controls
        
        [ContextMenu("Generate Timeline Events")]
        private void MenuGenerateTimelineEvents()
        {
            for (int i = 0; i < 3; i++)
            {
                var evt = new TimelineTestEvent("Manual", i, Random.Range(0.5f, 1f));
                EventSystem.Publish(evt);
            }
            Log("📊 Generated 3 timeline test events");
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
        
        [ContextMenu("Clear All Events")]
        private void MenuClearEvents()
        {
            // This will be handled by the EventCapture.Clear() method in the visualizer
            Log("🧹 Use 'Clear' button in Event Visualizer to clear events");
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