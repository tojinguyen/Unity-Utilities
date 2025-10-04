using UnityEngine;
using TirexGame.Utils.EventCenter;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Test component to verify EventSystem initialization works correctly
    /// This can be added to any GameObject to test the event system
    /// </summary>
    public class EventSystemTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool testInAwake = false;
        [SerializeField] private bool testInStart = true;
        [SerializeField] private bool enableVerboseLogging = false;
        
        // Test event
        public struct TestEvent
        {
            public string Message;
            
            public TestEvent(string message)
            {
                Message = message;
            }
        }
        
        private void Awake()
        {
            if (testInAwake)
            {
                TestEventSystem("Awake");
            }
        }
        
        private void Start()
        {
            if (testInStart)
            {
                TestEventSystem("Start");
            }
        }
        
        private void TestEventSystem(string phase)
        {
            Log($"[EventSystemTest] Testing EventSystem in {phase}...");
            
            // Check if EventCenterService is available
            if (!EventCenterService.IsAvailable)
            {
                LogError($"[EventSystemTest] ❌ EventCenterService not available in {phase}");
                return;
            }
            
            // Check if EventSystem is initialized
            Log($"[EventSystemTest] EventSystem.IsInitialized: {EventSystem.IsInitialized}");
            
            try
            {
                // Try to subscribe to an event
                this.SubscribeWithCleanup<TestEvent>(OnTestEvent);
                Log($"[EventSystemTest] ✅ Successfully subscribed in {phase}");
                
                // Try to publish an event
                EventSystem.Publish(new TestEvent($"Test from {phase}"));
                Log($"[EventSystemTest] ✅ Successfully published event in {phase}");
            }
            catch (System.Exception ex)
            {
                LogError($"[EventSystemTest] ❌ Failed to use EventSystem in {phase}: {ex.Message}");
                LogError($"[EventSystemTest] Exception: {ex}");
            }
        }
        
        private void OnTestEvent(TestEvent evt)
        {
            Log($"[EventSystemTest] Received TestEvent: {evt.Message}");
        }
        
        /// <summary>
        /// Manual test method that can be called from inspector
        /// </summary>
        [ContextMenu("Test EventSystem Now")]
        public void TestEventSystemNow()
        {
            TestEventSystem("Manual Test");
        }
        
        /// <summary>
        /// Check EventSystem status
        /// </summary>
        [ContextMenu("Check EventSystem Status")]
        public void CheckEventSystemStatus()
        {
            Log("=== EventSystem Status ===");
            Log($"EventCenterService.IsAvailable: {EventCenterService.IsAvailable}");
            Log($"EventSystem.IsInitialized: {EventSystem.IsInitialized}");
            
            var eventCenter = FindFirstObjectByType<EventCenter>();
            if (eventCenter != null)
            {
                Log($"✅ EventCenter found: {eventCenter.name}");
                
                // Check if it's auto-created
                if (eventCenter.name.Contains("Auto Created"))
                {
                    Log("🚀 This EventCenter was auto-created - Zero configuration working!");
                }
                
                // Check if it's DontDestroyOnLoad
                if (eventCenter.gameObject.scene.name == "DontDestroyOnLoad")
                {
                    Log("🔒 EventCenter is persistent across scenes (DontDestroyOnLoad)");
                }
            }
            else
            {
                Log("❌ No EventCenter found in scene - This should not happen with auto-creation!");
            }
            
            var eventCenterSetup = FindFirstObjectByType<EventCenterSetup>();
            if (eventCenterSetup != null)
            {
                Log($"✅ EventCenterSetup found: {eventCenterSetup.name}");
            }
            else
            {
                Log("ℹ️ No EventCenterSetup found - using auto-created EventCenter");
            }
            
            Log("=== Test Complete ===");
        }
        
        private void Log(string message)
        {
            if (enableVerboseLogging)
            {
                Debug.Log(message);
            }
        }
        
        private void LogError(string message)
        {
            // Always log errors
            Debug.LogError(message);
        }
    }
}