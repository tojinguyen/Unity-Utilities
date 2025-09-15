using UnityEngine;
using TirexGame.Utils.EventCenter;
using TirexGame.Utils.EventCenter.Examples;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Complete example demonstrating the new static EventSystem API
    /// Shows how much simpler and cleaner the code becomes
    /// </summary>
    public class StaticEventSystemExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private bool autoStartDemo = true;
        [SerializeField] private float demoInterval = 2f;
        
        private IEventSubscription _healthSubscription;
        private IEventSubscription _itemSubscription;
        private IEventSubscription _scoreSubscription;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            SetupEventListeners();
            
            if (autoStartDemo)
            {
                InvokeRepeating(nameof(RunRandomDemo), 1f, demoInterval);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up subscriptions
            _healthSubscription?.Dispose();
            _itemSubscription?.Dispose();
            _scoreSubscription?.Dispose();
        }
        
        #endregion
        
        #region Event Setup
        
        private void SetupEventListeners()
        {
            Debug.Log("=== Setting up Static EventSystem Demo ===");
            
            // 1. Simple callback subscription
            _healthSubscription = EventSystem.Subscribe<PlayerHealthEvent>((healthEvent) =>
            {
                Debug.Log($"💗 Player {healthEvent.PlayerId}: {healthEvent.CurrentHealth}/{healthEvent.MaxHealth} HP " +
                         $"({healthEvent.HealthPercentage:P0})");
                
                if (!healthEvent.IsAlive)
                {
                    Debug.Log($"💀 Player {healthEvent.PlayerId} has died!");
                }
            }, priority: 100);
            
            // 2. Item collection with conditional logic
            _itemSubscription = EventSystem.Subscribe<ItemCollectedEvent>((itemEvent) =>
            {
                Debug.Log($"📦 Collected {itemEvent.Quantity}x {itemEvent.ItemName}");
                
                // Chain another event based on item type
                if (itemEvent.ItemName.Contains("Potion"))
                {
                    var healEvent = new PlayerHealthEvent(itemEvent.CollectorId, 100f, 100f, -50f, itemEvent.CollectionPosition);
                    EventSystem.Publish(healEvent); // Heal the player
                }
            });
            
            // 3. Score tracking with high priority
            _scoreSubscription = EventSystem.Subscribe<ScoreChangedEvent>((scoreEvent) =>
            {
                Debug.Log($"🎯 Score: {scoreEvent.PreviousScore} → {scoreEvent.NewScore} " +
                         $"(+{scoreEvent.ScoreDelta}) - {scoreEvent.Reason}");
            }, priority: 200);
            
            // 4. One-time subscription example
            EventSystem.SubscribeOnce<LevelCompletedEvent>((levelEvent) =>
            {
                Debug.Log($"🏆 Level completed: {levelEvent.LevelName} in {levelEvent.CompletionTime:F1}s " +
                         $"({levelEvent.CompletionPercentage:P0} completion)");
            });
            
            // 5. Conditional subscription example
            EventSystem.SubscribeWhen<WeaponFiredEvent>((weaponEvent) =>
            {
                Debug.Log($"⚠️ Low ammo warning! Only {weaponEvent.AmmoRemaining} rounds left!");
            }, (weaponEvent) => weaponEvent.AmmoRemaining <= 3);
            
            Debug.Log("✅ All event listeners set up using static API!");
        }
        
        #endregion
        
        #region Demo Methods
        
        [ContextMenu("Run Random Demo")]
        private void RunRandomDemo()
        {
            var randomAction = Random.Range(0, 5);
            
            switch (randomAction)
            {
                case 0: DemoHealthEvents(); break;
                case 1: DemoItemCollection(); break;
                case 2: DemoScoreEvents(); break;
                case 3: DemoWeaponEvents(); break;
                case 4: DemoLevelCompletion(); break;
            }
        }
        
        private void DemoHealthEvents()
        {
            var playerId = Random.Range(1, 4);
            var damage = Random.Range(10f, 30f);
            var currentHealth = Random.Range(20f, 100f);
            
            var healthEvent = new PlayerHealthEvent(playerId, currentHealth, 100f, damage, transform.position);
            
            // Static API - so simple!
            EventSystem.Publish(healthEvent);
        }
        
        private void DemoItemCollection()
        {
            var items = new[] { "Health Potion", "Mana Potion", "Sword", "Shield", "Key", "Coin" };
            var itemName = items[Random.Range(0, items.Length)];
            var quantity = itemName == "Coin" ? Random.Range(5, 20) : 1;
            
            var itemEvent = new ItemCollectedEvent(
                Random.Range(100, 999),
                itemName,
                quantity,
                transform.position,
                Random.Range(1, 4)
            );
            
            EventSystem.Publish(itemEvent);
        }
        
        private void DemoScoreEvents()
        {
            var playerId = Random.Range(1, 4);
            var currentScore = Random.Range(500, 2000);
            var scoreIncrease = Random.Range(50, 500);
            var reasons = new[] { "Enemy Defeated", "Item Collected", "Quest Completed", "Bonus Found" };
            
            var scoreEvent = new ScoreChangedEvent(
                playerId,
                currentScore,
                currentScore + scoreIncrease,
                reasons[Random.Range(0, reasons.Length)]
            );
            
            EventSystem.Publish(scoreEvent);
        }
        
        private void DemoWeaponEvents()
        {
            var weaponEvent = new WeaponFiredEvent(
                Random.Range(1, 10),
                transform.position,
                Random.onUnitSphere,
                Random.Range(25f, 100f),
                Random.Range(0, 30), // Sometimes low ammo
                Random.Range(1, 4)
            );
            
            EventSystem.Publish(weaponEvent);
        }
        
        private void DemoLevelCompletion()
        {
            var levels = new[] { "Tutorial", "Forest Temple", "Dark Cave", "Fire Mountain", "Ice Palace" };
            var levelName = levels[Random.Range(0, levels.Length)];
            
            var levelEvent = new LevelCompletedEvent(
                Random.Range(1, 10),
                levelName,
                Random.Range(120f, 600f),
                Random.Range(8, 15),
                15
            );
            
            EventSystem.Publish(levelEvent);
        }
        
        #endregion
        
        #region Batch Operations Demo
        
        [ContextMenu("Demo Batch Events")]
        private void DemoBatchEvents()
        {
            Debug.Log("🚀 Demonstrating batch event publishing...");
            
            // Create multiple events
            var healthEvents = new PlayerHealthEvent[]
            {
                new PlayerHealthEvent(1, 75f, 100f, 25f, Vector3.zero),
                new PlayerHealthEvent(2, 50f, 100f, 50f, Vector3.zero),
                new PlayerHealthEvent(3, 90f, 100f, 10f, Vector3.zero)
            };
            
            // Batch publish - so convenient!
            EventSystem.PublishBatch(healthEvents, priority: 150);
            
            Debug.Log($"📨 Published {healthEvents.Length} health events in batch");
        }
        
        #endregion
        
        #region Performance Demo
        
        [ContextMenu("Performance Test - Static API")]
        private void PerformanceTestStaticAPI()
        {
            Debug.Log("⚡ Starting performance test with static API...");
            
            var startTime = Time.realtimeSinceStartup;
            const int eventCount = 50000;
            
            for (int i = 0; i < eventCount; i++)
            {
                var healthEvent = new PlayerHealthEvent(
                    i % 10,
                    Random.Range(0f, 100f),
                    100f,
                    Random.Range(1f, 20f),
                    Vector3.zero
                );
                
                // Static API - no EventCenter.Instance calls!
                EventSystem.Publish(healthEvent);
            }
            
            var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
            var eventsPerSecond = eventCount / (duration / 1000f);
            
            Debug.Log($"📊 Performance Results:\n" +
                     $"- Events published: {eventCount:N0}\n" +
                     $"- Total time: {duration:F2}ms\n" +
                     $"- Events/second: {eventsPerSecond:F0}\n" +
                     $"- Average time per event: {duration / eventCount:F4}ms");
            
            // Log system stats
            EventSystem.LogStatus();
        }
        
        #endregion
        
        #region Static API Showcase
        
        [ContextMenu("Showcase Static API Features")]
        private void ShowcaseStaticAPIFeatures()
        {
            Debug.Log("🌟 Showcasing Static API features...");
            
            // Feature 1: Simple publish
            EventSystem.Publish(new PlayerHealthEvent(1, 100f, 100f, 0f, Vector3.zero));
            
            // Feature 2: Immediate publish
            EventSystem.PublishImmediate(new ScoreChangedEvent(1, 1000, 1500, "Immediate Event"), priority: 999);
            
            // Feature 3: One-time subscription
            var onceSubscription = EventSystem.SubscribeOnce<ItemCollectedEvent>((item) =>
            {
                Debug.Log($"🎉 This will only run once: {item.ItemName}");
            });
            
            // Feature 4: Conditional subscription
            var whenSubscription = EventSystem.SubscribeWhen<PlayerHealthEvent>((health) =>
            {
                Debug.Log("🚨 CRITICAL HEALTH ALERT!");
            }, (health) => health.HealthPercentage <= 0.1f);
            
            // Trigger the one-time event
            EventSystem.Publish(new ItemCollectedEvent(1, "Demo Item", 1, Vector3.zero, 1));
            
            // Trigger the conditional event
            EventSystem.Publish(new PlayerHealthEvent(1, 5f, 100f, 95f, Vector3.zero));
            
            Debug.Log("✨ Static API showcase complete!");
        }
        
        #endregion
    }
}