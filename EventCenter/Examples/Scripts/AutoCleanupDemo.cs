using UnityEngine;
using TirexGame.Utils.EventCenter;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Demo script showcasing the auto-cleanup extension methods
    /// Script demo để trình bày extension methods tự động cleanup
    /// </summary>
    public class AutoCleanupDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private bool enableLogs = true;
        [SerializeField] private float autoDestroyDelay = 10f;
        
        #region Test Events
        
        public struct TestEvent1
        {
            public string Message;
            public int Value;
            
            public TestEvent1(string message, int value)
            {
                Message = message;
                Value = value;
            }
        }
        
        public struct TestEvent2
        {
            public bool IsImportant;
            public float Timestamp;
            
            public TestEvent2(bool important, float timestamp)
            {
                IsImportant = important;
                Timestamp = timestamp;
            }
        }
        
        #endregion
        
        private void Start()
        {
            Log("🎯 Auto Cleanup Demo Started!");
            Log("💡 This object will auto-destroy in " + autoDestroyDelay + " seconds");
            Log("🔍 Watch the console - events will auto-unsubscribe when object is destroyed");
            
            SetupAutoCleanupSubscriptions();
            
            // Auto destroy this object after delay to demonstrate cleanup
            Destroy(gameObject, autoDestroyDelay);
            
            // Start publishing test events
            InvokeRepeating(nameof(PublishTestEvents), 1f, 1f);
        }
        
        private void SetupAutoCleanupSubscriptions()
        {
            Log("📝 Setting up auto-cleanup subscriptions...");
            
            // 1. Normal subscription with auto cleanup
            this.SubscribeWithCleanup<TestEvent1>(OnTestEvent1);
            Log("✅ Subscribed to TestEvent1 with auto cleanup");
            
            // 2. Conditional subscription with auto cleanup
            this.SubscribeWhenWithCleanup<TestEvent2>(OnImportantTestEvent2, 
                evt => evt.IsImportant);
            Log("✅ Subscribed to TestEvent2 (important only) with auto cleanup");
            
            // 3. One-time subscription with auto cleanup
            this.SubscribeOnceWithCleanup<TestEvent1>(OnFirstTestEvent1);
            Log("✅ Subscribed once to TestEvent1 with auto cleanup");
            
            // 4. One-time conditional subscription with auto cleanup
            this.SubscribeOnceWithCleanup<TestEvent2>(OnFirstImportantEvent2, 
                evt => evt.IsImportant);
            Log("✅ Subscribed once to TestEvent2 (important only) with auto cleanup");
            
            Log($"🎯 Total subscriptions: 4 (all will auto-cleanup in {autoDestroyDelay} seconds)");
        }
        
        #region Event Handlers
        
        private void OnTestEvent1(TestEvent1 evt)
        {
            Log($"📨 [TestEvent1] {evt.Message} - Value: {evt.Value}");
        }
        
        private void OnImportantTestEvent2(TestEvent2 evt)
        {
            Log($"⚠️ [Important TestEvent2] Timestamp: {evt.Timestamp:F2}");
        }
        
        private void OnFirstTestEvent1(TestEvent1 evt)
        {
            Log($"🎯 [FIRST] TestEvent1 received: {evt.Message} (This handler will auto-remove after this)");
        }
        
        private void OnFirstImportantEvent2(TestEvent2 evt)
        {
            Log($"🎯 [FIRST IMPORTANT] TestEvent2 received at {evt.Timestamp:F2} (This handler will auto-remove after this)");
        }
        
        #endregion
        
        #region Test Event Publishing
        
        private void PublishTestEvents()
        {
            // Publish TestEvent1
            var event1 = new TestEvent1($"Hello from {gameObject.name}", Random.Range(1, 100));
            EventSystem.Publish(event1);
            
            // Publish TestEvent2 (sometimes important)
            var isImportant = Random.Range(0, 100) < 30; // 30% chance
            var event2 = new TestEvent2(isImportant, Time.time);
            EventSystem.Publish(event2);
            
            Log($"📤 Published events - Event1: {event1.Message}, Event2: Important={isImportant}");
        }
        
        #endregion
        
        #region Manual Tests
        
        [ContextMenu("Test Manual Event Publishing")]
        private void TestManualPublishing()
        {
            Log("🧪 Manual test - Publishing events...");
            
            EventSystem.Publish(new TestEvent1("Manual Test Event", 999));
            EventSystem.Publish(new TestEvent2(true, Time.time));
        }
        
        [ContextMenu("Check Cleanup Status")]
        private void CheckCleanupStatus()
        {
            Log("🔧 Using CancellationTokenOnDestroy - no extra components needed!");
            Log("✨ Cleanup will happen automatically when this GameObject is destroyed");
            Log($"⏰ Object will be destroyed in {autoDestroyDelay - Time.timeSinceLevelLoad:F1} seconds");
        }
        
        [ContextMenu("Destroy This Object (Test Cleanup)")]
        private void DestroyThisObject()
        {
            Log("💥 Manual destroy - testing auto cleanup...");
            Destroy(gameObject);
        }
        
        #endregion
        
        #region Lifecycle Events
        
        private void OnDestroy()
        {
            Log("💀 AutoCleanupDemo being destroyed...");
            Log("🧹 Extension methods will automatically unsubscribe all events!");
            Log("✨ No manual cleanup needed - that's the magic! ✨");
            
            // Không cần gọi Unsubscribe - extension methods sẽ tự động xử lý!
            // No need to call Unsubscribe - extension methods will handle it automatically!
        }
        
        #endregion
        
        #region Utility
        
        private void Log(string message)
        {
            if (enableLogs)
            {
                Debug.Log($"[AutoCleanupDemo] {message}");
            }
        }
        
        #endregion
        
        #region GUI
        
        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(10, Screen.height - 150, 400, 140));
            GUILayout.Label("🎯 Auto Cleanup Demo", GUI.skin.box);
            
            GUILayout.Label("🔧 Using CancellationTokenOnDestroy");
            GUILayout.Label($"Time until destroy: {autoDestroyDelay - Time.timeSinceLevelLoad:F1}s");
            
            if (GUILayout.Button("🧪 Publish Test Events"))
            {
                TestManualPublishing();
            }
            
            if (GUILayout.Button("🔧 Check Cleanup Status"))
            {
                CheckCleanupStatus();
            }
            
            if (GUILayout.Button("💥 Destroy Now (Test Cleanup)"))
            {
                DestroyThisObject();
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
    }
}