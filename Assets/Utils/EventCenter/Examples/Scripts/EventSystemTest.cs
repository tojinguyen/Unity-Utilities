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
            Debug.Log($"[EventSystemTest] Testing EventSystem in {phase}...");
            
            // Check if EventCenterService is available
            if (!EventCenterService.IsAvailable)
            {
                Debug.LogError($"[EventSystemTest] ❌ EventCenterService not available in {phase}");
                return;
            }
            
            // Check if EventSystem is initialized
            Debug.Log($"[EventSystemTest] EventSystem.IsInitialized: {EventSystem.IsInitialized}");
            
            try
            {
                // Try to subscribe to an event
                this.SubscribeWithCleanup<TestEvent>(OnTestEvent);
                Debug.Log($"[EventSystemTest] ✅ Successfully subscribed in {phase}");
                
                // Try to publish an event
                EventSystem.Publish(new TestEvent($"Test from {phase}"));
                Debug.Log($"[EventSystemTest] ✅ Successfully published event in {phase}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventSystemTest] ❌ Failed to use EventSystem in {phase}: {ex.Message}");
                Debug.LogError($"[EventSystemTest] Exception: {ex}");
            }
        }
        
        private void OnTestEvent(TestEvent evt)
        {
            Debug.Log($"[EventSystemTest] Received TestEvent: {evt.Message}");
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
            Debug.Log("=== EventSystem Status ===");
            Debug.Log($"EventCenterService.IsAvailable: {EventCenterService.IsAvailable}");
            Debug.Log($"EventSystem.IsInitialized: {EventSystem.IsInitialized}");
            
            var eventCenter = FindFirstObjectByType<EventCenter>();
            if (eventCenter != null)
            {
                Debug.Log($"✅ EventCenter found: {eventCenter.name}");
                
                // Check if it's auto-created
                if (eventCenter.name.Contains("Auto Created"))
                {
                    Debug.Log("🚀 This EventCenter was auto-created - Zero configuration working!");
                }
                
                // Check if it's DontDestroyOnLoad
                if (eventCenter.gameObject.scene.name == "DontDestroyOnLoad")
                {
                    Debug.Log("🔒 EventCenter is persistent across scenes (DontDestroyOnLoad)");
                }
            }
            else
            {
                Debug.Log("❌ No EventCenter found in scene - This should not happen with auto-creation!");
            }
            
            var eventCenterSetup = FindFirstObjectByType<EventCenterSetup>();
            if (eventCenterSetup != null)
            {
                Debug.Log($"✅ EventCenterSetup found: {eventCenterSetup.name}");
            }
            else
            {
                Debug.Log("ℹ️ No EventCenterSetup found - using auto-created EventCenter");
            }
            
            Debug.Log("=== Test Complete ===");
        }
    }
}