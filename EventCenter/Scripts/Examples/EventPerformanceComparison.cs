using UnityEngine;
using System.Diagnostics;
using TirexGame.Utils.EventCenter;
using TirexGame.Utils.EventCenter.Examples;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Performance comparison between struct events and legacy BaseEvent
    /// </summary>
    public class EventPerformanceComparison : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private int testIterations = 10000;
        [SerializeField] private bool runOnStart = false;
        
        private IEventCenter _eventCenter;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            _eventCenter = EventCenterService.Instance.GetEventCenter();
            
            if (runOnStart)
            {
                RunPerformanceComparison();
            }
        }
        
        #endregion
        
        #region Performance Tests
        
        [ContextMenu("Run Performance Comparison")]
        public void RunPerformanceComparison()
        {
            UnityEngine.Debug.Log("=== Event System Performance Comparison ===");
            UnityEngine.Debug.Log($"Test iterations: {testIterations:N0}");
            
            // Test static API struct events
            TestStaticAPIPerformance();
            
            // Force garbage collection
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            
            // Test instance API struct events
            TestInstanceAPIPerformance();
            
            // Force garbage collection before legacy test
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            
            // Test legacy events
            TestLegacyEventPerformance();
            
            // Display memory usage
            DisplayMemoryUsage();
        }
        
        private void TestStaticAPIPerformance()
        {
            UnityEngine.Debug.Log("\n--- Static API Struct Events Performance ---");
            
            var stopwatch = Stopwatch.StartNew();
            var initialMemory = System.GC.GetTotalMemory(false);
            
            for (int i = 0; i < testIterations; i++)
            {
                // Create and publish struct events using static API
                var healthEvent = new PlayerHealthEvent(i % 10, 75f, 100f, 25f, Vector3.zero);
                var itemEvent = new ItemCollectedEvent(i, "TestItem", 1, Vector3.zero, i % 10);
                var scoreEvent = new ScoreChangedEvent(i % 10, 1000, 1500, "Test");
                
                EventSystem.Publish(healthEvent);
                EventSystem.Publish(itemEvent);
                EventSystem.Publish(scoreEvent);
            }
            
            stopwatch.Stop();
            var finalMemory = System.GC.GetTotalMemory(false);
            
            UnityEngine.Debug.Log($"Static API - Time: {stopwatch.ElapsedMilliseconds}ms");
            UnityEngine.Debug.Log($"Static API - Memory allocated: {(finalMemory - initialMemory) / 1024f:F1} KB");
            UnityEngine.Debug.Log($"Static API - Events/second: {(testIterations * 3) / (stopwatch.ElapsedMilliseconds / 1000f):F0}");
        }
        
        private void TestInstanceAPIPerformance()
        {
            UnityEngine.Debug.Log("\n--- Instance API Struct Events Performance ---");
            
            var stopwatch = Stopwatch.StartNew();
            var initialMemory = System.GC.GetTotalMemory(false);
            
            for (int i = 0; i < testIterations; i++)
            {
                // Create and publish struct events using instance API
                var healthEvent = new PlayerHealthEvent(i % 10, 75f, 100f, 25f, Vector3.zero);
                var itemEvent = new ItemCollectedEvent(i, "TestItem", 1, Vector3.zero, i % 10);
                var scoreEvent = new ScoreChangedEvent(i % 10, 1000, 1500, "Test");
                
                _eventCenter.PublishEvent(healthEvent);
                _eventCenter.PublishEvent(itemEvent);
                _eventCenter.PublishEvent(scoreEvent);
            }
            
            stopwatch.Stop();
            var finalMemory = System.GC.GetTotalMemory(false);
            
            UnityEngine.Debug.Log($"Instance API - Time: {stopwatch.ElapsedMilliseconds}ms");
            UnityEngine.Debug.Log($"Instance API - Memory allocated: {(finalMemory - initialMemory) / 1024f:F1} KB");
            UnityEngine.Debug.Log($"Instance API - Events/second: {(testIterations * 3) / (stopwatch.ElapsedMilliseconds / 1000f):F0}");
        }
        
        private void TestLegacyEventPerformance()
        {
            UnityEngine.Debug.Log("\n--- Legacy Event Performance ---");
            
            var stopwatch = Stopwatch.StartNew();
            var initialMemory = System.GC.GetTotalMemory(false);
            
            for (int i = 0; i < testIterations; i++)
            {
                // Create and publish legacy events (if available)
                // Note: This would require legacy event implementations
                // For now, we'll simulate the overhead of object creation
                
                var legacyEvent1 = new TestLegacyEvent { Data = $"Health_{i}" };
                var legacyEvent2 = new TestLegacyEvent { Data = $"Item_{i}" };
                var legacyEvent3 = new TestLegacyEvent { Data = $"Score_{i}" };
                
                _eventCenter.PublishEvent(legacyEvent1);
                _eventCenter.PublishEvent(legacyEvent2);
                _eventCenter.PublishEvent(legacyEvent3);
            }
            
            stopwatch.Stop();
            var finalMemory = System.GC.GetTotalMemory(false);
            
            UnityEngine.Debug.Log($"Legacy Events - Time: {stopwatch.ElapsedMilliseconds}ms");
            UnityEngine.Debug.Log($"Legacy Events - Memory allocated: {(finalMemory - initialMemory) / 1024f:F1} KB");
            UnityEngine.Debug.Log($"Legacy Events - Events/second: {(testIterations * 3) / (stopwatch.ElapsedMilliseconds / 1000f):F0}");
        }
        
        private void DisplayMemoryUsage()
        {
            UnityEngine.Debug.Log("\n--- Memory Statistics ---");
            UnityEngine.Debug.Log($"Total memory: {System.GC.GetTotalMemory(false) / 1024f / 1024f:F2} MB");
            UnityEngine.Debug.Log($"Gen 0 collections: {System.GC.CollectionCount(0)}");
            UnityEngine.Debug.Log($"Gen 1 collections: {System.GC.CollectionCount(1)}");
            UnityEngine.Debug.Log($"Gen 2 collections: {System.GC.CollectionCount(2)}");
            
            var stats = _eventCenter.GetStats();
            UnityEngine.Debug.Log($"Event Center - Events processed: {stats.EventsProcessedThisFrame}");
            UnityEngine.Debug.Log($"Event Center - Queued events: {stats.QueuedEvents}");
            UnityEngine.Debug.Log($"Event Center - Active subscriptions: {stats.ActiveSubscriptions}");
        }
        
        #endregion
        
        #region Test Memory Allocation
        
        [ContextMenu("Test Memory Allocation - Structs")]
        public void TestStructMemoryAllocation()
        {
            var initialMemory = System.GC.GetTotalMemory(true);
            
            for (int i = 0; i < 100000; i++)
            {
                var structEvent = new PlayerHealthEvent(i, 100f, 100f, 0f, Vector3.zero);
                // Struct is allocated on stack, no heap allocation
            }
            
            var finalMemory = System.GC.GetTotalMemory(false);
            UnityEngine.Debug.Log($"Struct allocation test - Memory change: {(finalMemory - initialMemory) / 1024f:F1} KB");
        }
        
        [ContextMenu("Test Memory Allocation - Objects")]
        public void TestObjectMemoryAllocation()
        {
            var initialMemory = System.GC.GetTotalMemory(true);
            
            for (int i = 0; i < 100000; i++)
            {
                var objectEvent = new TestLegacyEvent { Data = $"Test_{i}" };
                // Object is allocated on heap
            }
            
            var finalMemory = System.GC.GetTotalMemory(false);
            UnityEngine.Debug.Log($"Object allocation test - Memory change: {(finalMemory - initialMemory) / 1024f:F1} KB");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Simple legacy event for comparison testing
    /// </summary>
    public class TestLegacyEvent : BaseEvent
    {
        public string Data { get; set; }
        
        public override void Reset()
        {
            base.Reset();
            Data = null;
        }
    }
}