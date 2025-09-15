using System.Collections.Generic;
using UnityEngine;
using TirexGame.Utils.EventCenter.Examples;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Example demonstrating high-performance event processing
    /// Shows how to handle thousands of events efficiently
    /// </summary>
    public class PerformanceEventExample : MonoBehaviour
    {
        [Header("Performance Test Settings")]
        [SerializeField] private int eventsPerSecond = 5000;
        [SerializeField] private bool useBatchProcessing = true;
        [SerializeField] private bool useEventBus = false;
        [SerializeField] private bool enableProfiling = true;
        
        [Header("Spawn Settings")]
        [SerializeField] private int maxEnemies = 1000;
        [SerializeField] private float spawnRadius = 50f;
        [SerializeField] private GameObject enemyPrefab;
        
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private EventBatchProcessor batchProcessor;
        private float lastSpawnTime;
        private int totalEventsProcessed;
        
        // Event subscriptions
        private IEventSubscription enemySpawnSubscription;
        private IEventSubscription projectileSubscription;
        private IEventSubscription pickupSubscription;
        
        void Start()
        {
            // Ensure EventCenter service is available
            if (!EventCenterService.EnsureAvailable())
            {
                Debug.LogError("Failed to initialize EventCenter service");
                return;
            }
            
            InitializeEventProcessing();
            Log("Performance Event Example started");
        }
        
        void Update()
        {
            if (useEventBus)
            {
                GenerateEventsWithEventBus();
            }
            else
            {
                GenerateEventsWithEventCenter();
            }
            
            if (useBatchProcessing && batchProcessor != null)
            {
                ProcessBatches();
            }
            
            if (enableProfiling && Time.frameCount % 60 == 0)
            {
                LogPerformanceStats();
            }
        }
        
        private void InitializeEventProcessing()
        {
            if (useBatchProcessing)
            {
                batchProcessor = new EventBatchProcessor(100, 0.016f, enableProfiling);
            }
            
            if (useEventBus)
            {
                // Subscribe to events using EventBus
                EventBus.Listen<EnemySpawned>(OnEnemySpawnedFast);
                EventBus.Listen<ProjectileFired>(OnProjectileFiredFast);
                EventBus.Listen<PickupCollected>(OnPickupCollectedFast);
            }
            else
            {
                // Subscribe to events using EventCenter
                enemySpawnSubscription = EventCenterService.Subscribe<EnemySpawned>(OnEnemySpawned);
                projectileSubscription = EventCenterService.Subscribe<ProjectileFired>(OnProjectileFired);
                pickupSubscription = EventCenterService.Subscribe<PickupCollected>(OnPickupCollected);
            }
        }
        
        private void GenerateEventsWithEventBus()
        {
            float eventsThisFrame = eventsPerSecond * Time.deltaTime;
            int eventCount = Mathf.RoundToInt(eventsThisFrame);
            
            for (int i = 0; i < eventCount; i++)
            {
                GenerateRandomEventForEventBus();
            }
        }
        
        private void GenerateEventsWithEventCenter()
        {
            float eventsThisFrame = eventsPerSecond * Time.deltaTime;
            int eventCount = Mathf.RoundToInt(eventsThisFrame);
            
            for (int i = 0; i < eventCount; i++)
            {
                GenerateRandomEventForEventCenter();
            }
        }
        
        private void GenerateRandomEventForEventBus()
        {
            int eventType = Random.Range(0, 3);
            
            switch (eventType)
            {
                case 0: // Enemy spawn
                    var enemyEvent = new EnemySpawned
                    {\n                        SpawnPosition = GetRandomSpawnPosition(),\n                        EnemyType = Random.Range(0, 5)\n                    };\n                    EventBus.Send(enemyEvent);\n                    break;\n                \n                case 1: // Projectile\n                    var projectileEvent = new ProjectileFired\n                    {\n                        StartPosition = GetRandomPosition(),\n                        Direction = Random.insideUnitSphere.normalized,\n                        Speed = Random.Range(10f, 50f),\n                        DamageAmount = Random.Range(5, 25)\n                    };\n                    EventBus.Send(projectileEvent);\n                    break;\n                \n                case 2: // Pickup\n                    var pickupEvent = new PickupCollected\n                    {\n                        Position = GetRandomPosition(),\n                        PickupType = Random.Range(0, 3),\n                        Value = Random.Range(1, 100)\n                    };\n                    EventBus.Send(pickupEvent);\n                    break;\n            }\n        }\n        \n        private void GenerateRandomEventForEventCenter()\n        {\n            int eventType = Random.Range(0, 3);\n            \n            switch (eventType)\n            {\n                case 0: // Enemy spawn\n                    if (useBatchProcessing)\n                    {\n                        var enemyEvent = EventCenterService.Current.CreateEvent<EnemySpawned>();\n                        enemyEvent.SpawnPosition = GetRandomSpawnPosition();\n                        enemyEvent.EnemyType = Random.Range(0, 5);\n                        \n                        batchProcessor.AddToBatch(enemyEvent, ProcessEnemySpawnBatch);\n                    }\n                    else\n                    {\n                        EventCenterService.CreateAndPublish<EnemySpawned>(this);\n                    }\n                    break;\n                \n                case 1: // Projectile\n                    var projectileEvent = EventCenterService.Current.CreateEvent<ProjectileFired>();\n                    projectileEvent.StartPosition = GetRandomPosition();\n                    projectileEvent.Direction = Random.insideUnitSphere.normalized;\n                    projectileEvent.Speed = Random.Range(10f, 50f);\n                    projectileEvent.DamageAmount = Random.Range(5, 25);\n                    \n                    EventCenterService.Current.PublishEvent(projectileEvent);\n                    break;\n                \n                case 2: // Pickup\n                    var pickupEvent = EventCenterService.Current.CreateEvent<PickupCollected>();\n                    pickupEvent.Position = GetRandomPosition();\n                    pickupEvent.PickupType = Random.Range(0, 3);\n                    pickupEvent.Value = Random.Range(1, 100);\n                    \n                    EventCenterService.Current.PublishEvent(pickupEvent);\n                    break;\n            }\n        }\n        \n        private void ProcessBatches()\n        {\n            if (batchProcessor != null)\n            {\n                batchProcessor.ProcessReadyBatches();\n            }\n        }\n        \n        #region Event Handlers\n        \n        // EventCenter handlers\n        private void OnEnemySpawned(EnemySpawned eventData)\n        {\n            SpawnEnemy(eventData.SpawnPosition, eventData.EnemyType);\n            totalEventsProcessed++;\n        }\n        \n        private void OnProjectileFired(ProjectileFired eventData)\n        {\n            // Handle projectile logic\n            totalEventsProcessed++;\n        }\n        \n        private void OnPickupCollected(PickupCollected eventData)\n        {\n            // Handle pickup logic\n            totalEventsProcessed++;\n        }\n        \n        // EventBus handlers (optimized)\n        private void OnEnemySpawnedFast(EnemySpawned eventData)\n        {\n            SpawnEnemy(eventData.SpawnPosition, eventData.EnemyType);\n            totalEventsProcessed++;\n        }\n        \n        private void OnProjectileFiredFast(ProjectileFired eventData)\n        {\n            // Handle projectile logic (fast)\n            totalEventsProcessed++;\n        }\n        \n        private void OnPickupCollectedFast(PickupCollected eventData)\n        {\n            // Handle pickup logic (fast)\n            totalEventsProcessed++;\n        }\n        \n        // Batch processors\n        private void ProcessEnemySpawnBatch(List<EnemySpawned> events)\n        {\n            foreach (var eventData in events)\n            {\n                SpawnEnemy(eventData.SpawnPosition, eventData.EnemyType);\n                totalEventsProcessed++;\n            }\n            \n            Log($"Processed batch of {events.Count} enemy spawn events");\n        }\n        \n        #endregion\n        \n        #region Helper Methods\n        \n        private void SpawnEnemy(Vector3 position, int enemyType)\n        {\n            if (spawnedEnemies.Count >= maxEnemies)\n            {\n                // Reuse existing enemy\n                var enemy = spawnedEnemies[Random.Range(0, spawnedEnemies.Count)];\n                if (enemy != null)\n                {\n                    enemy.transform.position = position;\n                }\n            }\n            else if (enemyPrefab != null)\n            {\n                // Spawn new enemy\n                var enemy = Instantiate(enemyPrefab, position, Quaternion.identity);\n                spawnedEnemies.Add(enemy);\n            }\n        }\n        \n        private Vector3 GetRandomSpawnPosition()\n        {\n            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;\n            var distance = Random.Range(spawnRadius * 0.8f, spawnRadius);\n            \n            return new Vector3(\n                transform.position.x + Mathf.Cos(angle) * distance,\n                transform.position.y,\n                transform.position.z + Mathf.Sin(angle) * distance\n            );\n        }\n        \n        private Vector3 GetRandomPosition()\n        {\n            return transform.position + Random.insideUnitSphere * spawnRadius * 0.5f;\n        }\n        \n        private void LogPerformanceStats()\n        {\n            if (useEventBus)\n            {\n                var busStats = EventBus.Instance.GetStats();\n                Log($"EventBus Stats - Subscriptions: {busStats.TotalSubscriptions}, " +\n                    $"Publishes: {busStats.TotalPublishes}, " +\n                    $"Avg Time: {busStats.AverageExecutionTime:F2}μs");\n            }\n            else\n            {\n                var centerStats = EventCenter.Instance.GetStats();\n                Log($"EventCenter Stats - Events/Frame: {centerStats.EventsProcessedThisFrame}, " +\n                    $"Queued: {centerStats.QueuedEvents}, " +\n                    $"Subscriptions: {centerStats.ActiveSubscriptions}");\n            }\n            \n            if (batchProcessor != null)\n            {\n                Log($"Batch Processor - Batches: {batchProcessor.TotalBatchesProcessed}, " +\n                    $"Events: {batchProcessor.TotalEventsProcessed}, " +\n                    $"Last Time: {batchProcessor.LastProcessTime:F2}ms");\n            }\n            \n            Log($"Total Events Processed: {totalEventsProcessed}");\n        }\n        \n        private void Log(string message)\n        {\n            if (enableProfiling)\n                Debug.Log($"[PerformanceExample] {message}");\n        }\n        \n        #endregion\n        \n        private void OnDestroy()\n        {\n            // Clean up subscriptions\n            enemySpawnSubscription?.Dispose();\n            projectileSubscription?.Dispose();\n            pickupSubscription?.Dispose();\n            \n            if (useEventBus)\n            {\n                EventBus.Unlisten<EnemySpawned>(OnEnemySpawnedFast);\n                EventBus.Unlisten<ProjectileFired>(OnProjectileFiredFast);\n                EventBus.Unlisten<PickupCollected>(OnPickupCollectedFast);\n            }\n            \n            // Clean up spawned objects\n            foreach (var enemy in spawnedEnemies)\n            {\n                if (enemy != null)\n                    Destroy(enemy);\n            }\n            \n            Log("Performance Event Example destroyed");\n        }\n    }\n}