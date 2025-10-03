using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TirexGame.Utils.UI;

namespace TirexGame.Utils.UI.Example
{
    /// <summary>
    /// Performance test controller for testing RecycleView with large datasets.
    /// </summary>
    public class PerformanceTestController : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private RecycleView recycleView;
        [SerializeField] private int testDataSize = 10000;
        [SerializeField] private bool runTestOnStart = false;

        [Header("Performance Metrics")]
        [SerializeField] private bool logPerformanceMetrics = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                RunPerformanceTest();
            }
        }

        [ContextMenu("Run Performance Test (1K items)")]
        public void RunSmallTest()
        {
            RunPerformanceTestWithSize(1000);
        }

        [ContextMenu("Run Performance Test (10K items)")]
        public void RunMediumTest()
        {
            RunPerformanceTestWithSize(10000);
        }

        [ContextMenu("Run Performance Test (50K items)")]
        public void RunLargeTest()
        {
            RunPerformanceTestWithSize(50000);
        }

        public void RunPerformanceTest()
        {
            RunPerformanceTestWithSize(testDataSize);
        }

        private void RunPerformanceTestWithSize(int dataSize)
        {
            if (recycleView == null)
            {
                UnityEngine.Debug.LogError("RecycleView reference is not set!");
                return;
            }

            UnityEngine.Debug.Log($"=== Starting Performance Test with {dataSize} items ===");

            // Measure data generation time
            Stopwatch dataGenStopwatch = Stopwatch.StartNew();
            var testData = GenerateTestData(dataSize);
            dataGenStopwatch.Stop();

            if (logPerformanceMetrics)
            {
                UnityEngine.Debug.Log($"Data generation time: {dataGenStopwatch.ElapsedMilliseconds}ms for {dataSize} items");
                UnityEngine.Debug.Log($"Memory usage - Data: ~{(dataSize * 32) / 1024}KB"); // Rough estimate
            }

            // Measure RecycleView setup time
            Stopwatch setupStopwatch = Stopwatch.StartNew();
            recycleView.SetData(testData);
            setupStopwatch.Stop();

            if (logPerformanceMetrics)
            {
                UnityEngine.Debug.Log($"RecycleView setup time: {setupStopwatch.ElapsedMilliseconds}ms");
                UnityEngine.Debug.Log($"Total initialization time: {dataGenStopwatch.ElapsedMilliseconds + setupStopwatch.ElapsedMilliseconds}ms");
                UnityEngine.Debug.Log($"=== Performance Test Complete ===");
            }
        }

        private List<IRecycleViewData> GenerateTestData(int count)
        {
            var data = new List<IRecycleViewData>(count);

            for (int i = 0; i < count; i++)
            {
                // Alternate between different item types for variety
                if (i % 2 == 0)
                {
                    data.Add(new TextMessageData
                    {
                        Message = $"Text Item {i}: This is a test message for performance testing. Item number {i} with some sample content."
                    });
                }
                else
                {
                    data.Add(new ImageMessageData
                    {
                        Image = null, // We'll use null for performance testing to avoid loading actual sprites
                        Caption = $"Image Item {i}: Performance test caption for item {i}"
                    });
                }
            }

            return data;
        }

        [ContextMenu("Log Memory Usage")]
        public void LogMemoryUsage()
        {
            long totalMemory = System.GC.GetTotalMemory(false);
            UnityEngine.Debug.Log($"Current memory usage: {totalMemory / 1024 / 1024}MB");
            
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Unity Profiler Memory: {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024}MB");
#endif
        }

        [ContextMenu("Force Garbage Collection")]
        public void ForceGC()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            UnityEngine.Debug.Log("Forced garbage collection completed");
        }

        /// <summary>
        /// Test scrolling performance by programmatically scrolling through the list.
        /// </summary>
        [ContextMenu("Test Scroll Performance")]
        public void TestScrollPerformance()
        {
            if (recycleView == null) return;

            StartCoroutine(ScrollPerformanceTest());
        }

        private System.Collections.IEnumerator ScrollPerformanceTest()
        {
            UnityEngine.Debug.Log("Starting scroll performance test...");
            
            var scrollRect = recycleView.GetComponent<UnityEngine.UI.ScrollRect>();
            if (scrollRect == null)
            {
                UnityEngine.Debug.LogError("ScrollRect not found!");
                yield break;
            }

            const int scrollSteps = 100;
            const float scrollDelay = 0.02f; // 50 FPS
            
            for (int i = 0; i <= scrollSteps; i++)
            {
                float normalizedPosition = (float)i / scrollSteps;
                scrollRect.verticalNormalizedPosition = 1.0f - normalizedPosition; // Scroll from top to bottom
                
                yield return new WaitForSeconds(scrollDelay);
            }

            UnityEngine.Debug.Log("Scroll performance test completed");
        }
    }
}