using UnityEngine;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Comparison between old EventCenterService pattern and new Static EventSystem API
    /// 
    /// MIGRATION GUIDE: FROM INSTANCE TO STATIC API
    /// ============================================
    /// 
    /// The new static API provides these benefits:
    /// 1. Cleaner, more concise code
    /// 2. No need to get EventCenter instances
    /// 3. Better IntelliSense support
    /// 4. Consistent API across the codebase
    /// 5. Automatic initialization
    /// 6. Additional convenience methods
    /// 
    /// BACKWARD COMPATIBILITY:
    /// ======================
    /// The old EventCenterService API still works unchanged.
    /// You can migrate gradually or use both APIs side by side.
    /// </summary>
    public class APIComparisonGuide : MonoBehaviour
    {
        #region OLD WAY vs NEW WAY - Basic Usage
        
        // ❌ OLD WAY - EventCenterService Instance API
        private void OldWaySubscription()
        {
            // Step 1: Get EventCenter instance
            var eventCenter = EventCenterService.Instance.GetEventCenter();
            
            // Step 2: Subscribe to events
            var subscription = eventCenter.Subscribe<PlayerHealthEvent>((healthEvent) =>
            {
                Debug.Log($"Health: {healthEvent.CurrentHealth}");
            });
            
            // Step 3: Publish events
            var healthEvent = new PlayerHealthEvent(1, 75f, 100f, 25f, Vector3.zero);
            eventCenter.PublishEvent(healthEvent);
        }
        
        // ✅ NEW WAY - Static EventSystem API
        private void NewWaySubscription()
        {
            // Step 1: Subscribe directly (no instance needed!)
            var subscription = EventSystem.Subscribe<PlayerHealthEvent>((healthEvent) =>
            {
                Debug.Log($"Health: {healthEvent.CurrentHealth}");
            });
            
            // Step 2: Publish directly
            var healthEvent = new PlayerHealthEvent(1, 75f, 100f, 25f, Vector3.zero);
            EventSystem.Publish(healthEvent);
        }
        
        #endregion
        
        #region OLD WAY vs NEW WAY - Advanced Features
        
        // ❌ OLD WAY - Complex subscription management
        private void OldWayAdvanced()
        {
            var eventCenter = EventCenterService.Instance.GetEventCenter();
            
            // Multiple subscriptions require multiple calls
            var healthSub = eventCenter.Subscribe<PlayerHealthEvent>((e) => Debug.Log("Health"));
            var itemSub = eventCenter.Subscribe<ItemCollectedEvent>((e) => Debug.Log("Item"));
            var scoreSub = eventCenter.Subscribe<ScoreChangedEvent>((e) => Debug.Log("Score"));
            
            // Immediate publishing
            var event1 = new PlayerHealthEvent(1, 50f, 100f, 0f, Vector3.zero);
            eventCenter.PublishEventImmediate(event1, 100);
            
            // Batch publishing requires loops
            var events = new[]
            {
                new PlayerHealthEvent(1, 75f, 100f, 25f, Vector3.zero),
                new PlayerHealthEvent(2, 50f, 100f, 50f, Vector3.zero)
            };
            
            foreach (var evt in events)
            {
                eventCenter.PublishEvent(evt);
            }
        }
        
        // ✅ NEW WAY - Enhanced convenience methods
        private void NewWayAdvanced()
        {
            // Simple, clean subscriptions
            var healthSub = EventSystem.Subscribe<PlayerHealthEvent>((e) => Debug.Log("Health"));
            var itemSub = EventSystem.Subscribe<ItemCollectedEvent>((e) => Debug.Log("Item"));
            var scoreSub = EventSystem.Subscribe<ScoreChangedEvent>((e) => Debug.Log("Score"));
            
            // Immediate publishing with cleaner API
            var event1 = new PlayerHealthEvent(1, 50f, 100f, 0f, Vector3.zero);
            EventSystem.PublishImmediate(event1, 100);
            
            // Built-in batch publishing
            var events = new[]
            {
                new PlayerHealthEvent(1, 75f, 100f, 25f, Vector3.zero),
                new PlayerHealthEvent(2, 50f, 100f, 50f, Vector3.zero)
            };
            
            EventSystem.PublishBatch(events);
            
            // NEW CONVENIENCE FEATURES:
            
            // One-time subscription
            EventSystem.SubscribeOnce<LevelCompletedEvent>((level) =>
            {
                Debug.Log("Level completed - this runs only once!");
            });
            
            // Conditional subscription
            EventSystem.SubscribeWhen<PlayerHealthEvent>((health) =>
            {
                Debug.Log("Critical health!");
            }, (health) => health.HealthPercentage <= 0.25f);
        }
        
        #endregion
        
        #region Migration Examples
        
        // MIGRATION EXAMPLE 1: Simple event listener component
        public class OldEventListener : MonoBehaviour, IEventListener<PlayerHealthEvent>
        {
            public int Priority => 0;
            public bool IsActive => true;
            public bool HandleEvent(BaseEvent eventData) => false;
            
            private IEventSubscription _subscription;
            
            // ❌ Old way
            private void StartOld()
            {
                var eventCenter = EventCenterService.Instance.GetEventCenter();
                _subscription = eventCenter.Subscribe<PlayerHealthEvent>(this);
            }
            
            // ✅ New way
            private void StartNew()
            {
                _subscription = EventSystem.Subscribe<PlayerHealthEvent>(this);
            }
            
            public bool HandleEvent(PlayerHealthEvent healthEvent)
            {
                Debug.Log($"Player health: {healthEvent.CurrentHealth}");
                return true;
            }
        }
        
        // MIGRATION EXAMPLE 2: Event publisher component
        public class EventPublisher : MonoBehaviour
        {
            // ❌ Old way
            [ContextMenu("Publish Event - Old Way")]
            private void PublishEventOld()
            {
                var eventCenter = EventCenterService.Instance.GetEventCenter();
                var healthEvent = new PlayerHealthEvent(1, 75f, 100f, 25f, transform.position);
                eventCenter.PublishEvent(healthEvent);
            }
            
            // ✅ New way
            [ContextMenu("Publish Event - New Way")]
            private void PublishEventNew()
            {
                var healthEvent = new PlayerHealthEvent(1, 75f, 100f, 25f, transform.position);
                EventSystem.Publish(healthEvent);
            }
            
            // ❌ Old way - Performance test
            [ContextMenu("Performance Test - Old Way")]
            private void PerformanceTestOld()
            {
                var eventCenter = EventCenterService.Instance.GetEventCenter();
                var startTime = Time.realtimeSinceStartup;
                
                for (int i = 0; i < 10000; i++)
                {
                    var evt = new PlayerHealthEvent(i % 10, 100f, 100f, 0f, Vector3.zero);
                    eventCenter.PublishEvent(evt);
                }
                
                var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log($"Old API: {duration:F2}ms for 10k events");
            }
            
            // ✅ New way - Performance test
            [ContextMenu("Performance Test - New Way")]
            private void PerformanceTestNew()
            {
                var startTime = Time.realtimeSinceStartup;
                
                for (int i = 0; i < 10000; i++)
                {
                    var evt = new PlayerHealthEvent(i % 10, 100f, 100f, 0f, Vector3.zero);
                    EventSystem.Publish(evt);
                }
                
                var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log($"New API: {duration:F2}ms for 10k events");
            }
        }
        
        #endregion
        
        #region Code Comparison Summary
        
        /*
         * CODE COMPARISON SUMMARY:
         * ========================
         * 
         * SUBSCRIBING TO EVENTS:
         * ----------------------
         * 
         * ❌ Old Way:
         * var eventCenter = EventCenterService.Instance.GetEventCenter();
         * var subscription = eventCenter.Subscribe<PlayerHealthEvent>(listener);
         * 
         * ✅ New Way:
         * var subscription = EventSystem.Subscribe<PlayerHealthEvent>(listener);
         * 
         * 
         * PUBLISHING EVENTS:
         * ------------------
         * 
         * ❌ Old Way:
         * var eventCenter = EventCenterService.Instance.GetEventCenter();
         * eventCenter.PublishEvent(healthEvent);
         * 
         * ✅ New Way:
         * EventSystem.Publish(healthEvent);
         * 
         * 
         * LAMBDA SUBSCRIPTIONS:
         * ---------------------
         * 
         * ❌ Old Way:
         * var eventCenter = EventCenterService.Instance.GetEventCenter();
         * var sub = eventCenter.Subscribe<PlayerHealthEvent>((e) => Debug.Log("Health"), 100);
         * 
         * ✅ New Way:
         * var sub = EventSystem.Subscribe<PlayerHealthEvent>((e) => Debug.Log("Health"), 100);
         * 
         * 
         * IMMEDIATE PUBLISHING:
         * ---------------------
         * 
         * ❌ Old Way:
         * var eventCenter = EventCenterService.Instance.GetEventCenter();
         * eventCenter.PublishEventImmediate(healthEvent, 100);
         * 
         * ✅ New Way:
         * EventSystem.PublishImmediate(healthEvent, 100);
         * 
         * 
         * NEW CONVENIENCE FEATURES (Not available in old API):
         * ====================================================
         * 
         * ✨ One-time subscription:
         * EventSystem.SubscribeOnce<GameEndEvent>((e) => Debug.Log("Game ended!"));
         * 
         * ✨ Conditional subscription:
         * EventSystem.SubscribeWhen<HealthEvent>((e) => Debug.Log("Low health!"), 
         *                                       (e) => e.HealthPercentage <= 0.25f);
         * 
         * ✨ Batch publishing:
         * EventSystem.PublishBatch(healthEvents, priority: 100);
         * 
         * ✨ System status:
         * EventSystem.LogStatus();
         * 
         * 
         * BENEFITS OF NEW STATIC API:
         * ============================
         * 
         * 1. 📝 LESS CODE: 50% fewer lines for basic operations
         * 2. 🚀 CLEANER: No instance management required
         * 3. 🔧 EASIER: Better IntelliSense and discoverability
         * 4. ⚡ SAME PERFORMANCE: Zero overhead compared to instance API
         * 5. 🛡️ SAFER: Automatic initialization and error handling
         * 6. 🎯 FOCUSED: API designed specifically for common use cases
         * 7. 🔄 COMPATIBLE: Works alongside existing EventCenterService code
         * 
         */
        
        #endregion
        
        #region Example: Before and After a Real Component
        
        // REAL WORLD EXAMPLE: Health System Component
        
        // ❌ BEFORE - Using EventCenterService (Still works!)
        /*
        public class HealthSystemOld : MonoBehaviour
        {
            private IEventCenter _eventCenter;
            private IEventSubscription _damageSubscription;
            private IEventSubscription _healSubscription;
            
            private void Start()
            {
                _eventCenter = EventCenterService.Instance.GetEventCenter();
                
                _damageSubscription = _eventCenter.Subscribe<DamageEvent>((damageEvent) =>
                {
                    ApplyDamage(damageEvent.PlayerId, damageEvent.Damage);
                }, priority: 100);
                
                _healSubscription = _eventCenter.Subscribe<HealEvent>((healEvent) =>
                {
                    ApplyHealing(healEvent.PlayerId, healEvent.HealAmount);
                }, priority: 50);
            }
            
            private void OnPlayerDeath(int playerId)
            {
                var deathEvent = new PlayerDeathEvent(playerId, transform.position);
                _eventCenter.PublishEventImmediate(deathEvent, 999);
            }
            
            private void OnDestroy()
            {
                _damageSubscription?.Dispose();
                _healSubscription?.Dispose();
            }
        }
        */
        
        // ✅ AFTER - Using Static EventSystem API (Much cleaner!)
        public class HealthSystemNew : MonoBehaviour
        {
            private IEventSubscription _damageSubscription;
            private IEventSubscription _healSubscription;
            
            private void Start()
            {
                _damageSubscription = EventSystem.Subscribe<DamageEvent>((damageEvent) =>
                {
                    ApplyDamage(damageEvent.PlayerId, damageEvent.Damage);
                }, priority: 100);
                
                _healSubscription = EventSystem.Subscribe<HealEvent>((healEvent) =>
                {
                    ApplyHealing(healEvent.PlayerId, healEvent.HealAmount);
                }, priority: 50);
            }
            
            private void OnPlayerDeath(int playerId)
            {
                var deathEvent = new PlayerDeathEvent(playerId, transform.position);
                EventSystem.PublishImmediate(deathEvent, 999);
            }
            
            private void OnDestroy()
            {
                _damageSubscription?.Dispose();
                _healSubscription?.Dispose();
            }
            
            // Dummy methods for compilation
            private void ApplyDamage(int playerId, float damage) { }
            private void ApplyHealing(int playerId, float heal) { }
        }
        
        // Example struct events for the health system
        public struct DamageEvent
        {
            public int PlayerId;
            public float Damage;
            public Vector3 Position;
        }
        
        public struct HealEvent
        {
            public int PlayerId;
            public float HealAmount;
        }
        
        public struct PlayerDeathEvent
        {
            public int PlayerId;
            public Vector3 DeathPosition;
            
            public PlayerDeathEvent(int playerId, Vector3 position)
            {
                PlayerId = playerId;
                DeathPosition = position;
            }
        }
        
        #endregion
    }
}