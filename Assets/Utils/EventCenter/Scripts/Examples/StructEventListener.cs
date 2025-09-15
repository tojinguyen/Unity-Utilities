using UnityEngine;
using TirexGame.Utils.EventCenter;
using TirexGame.Utils.EventCenter.Examples;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Example listener for struct events
    /// </summary>
    public class StructEventListener : MonoBehaviour, 
        IEventListener<PlayerHealthEvent>, 
        IEventListener<ItemCollectedEvent>,
        IEventListener<ScoreChangedEvent>
    {
        [SerializeField] private int priority = 0;
        [SerializeField] private bool isActive = true;
        
        private IEventSubscription _healthSubscription;
        private IEventSubscription _itemSubscription;
        private IEventSubscription _scoreSubscription;
        
        #region IEventListener Implementation
        
        public int Priority => priority;
        public bool IsActive => isActive && gameObject.activeInHierarchy;
        
        public bool HandleEvent(BaseEvent eventData)
        {
            // Legacy method - not used for struct events
            return false;
        }
        
        #endregion
        
        #region Struct Event Handlers
        
        public bool HandleEvent(PlayerHealthEvent healthEvent)
        {
            Debug.Log($"Player {healthEvent.PlayerId} health: {healthEvent.CurrentHealth}/{healthEvent.MaxHealth} " +
                     $"({healthEvent.HealthPercentage:P0}) - Damage: {healthEvent.Damage}");
            
            if (!healthEvent.IsAlive)
            {
                Debug.Log($"Player {healthEvent.PlayerId} has died!");
            }
            
            return true;
        }
        
        public bool HandleEvent(ItemCollectedEvent itemEvent)
        {
            Debug.Log($"Player {itemEvent.CollectorId} collected {itemEvent.Quantity}x {itemEvent.ItemName} " +
                     $"at position {itemEvent.CollectionPosition}");
            
            return true;
        }
        
        public bool HandleEvent(ScoreChangedEvent scoreEvent)
        {
            Debug.Log($"Player {scoreEvent.PlayerId} score changed: {scoreEvent.PreviousScore} -> {scoreEvent.NewScore} " +
                     $"(+{scoreEvent.ScoreDelta}) - Reason: {scoreEvent.Reason}");
            
            return true;
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            // Subscribe to struct events using static API
            _healthSubscription = EventSystem.Subscribe<PlayerHealthEvent>(this);
            _itemSubscription = EventSystem.Subscribe<ItemCollectedEvent>(this);
            _scoreSubscription = EventSystem.Subscribe<ScoreChangedEvent>(this);
            
            Debug.Log("StructEventListener subscribed to struct events using static API");
        }
        
        private void OnDestroy()
        {
            // Clean up subscriptions
            _healthSubscription?.Dispose();
            _itemSubscription?.Dispose();
            _scoreSubscription?.Dispose();
        }
        
        #endregion
        
        #region Test Methods
        
        [ContextMenu("Test Health Event")]
        private void TestHealthEvent()
        {
            var healthEvent = new PlayerHealthEvent(1, 75f, 100f, 25f, transform.position);
            EventSystem.Publish(healthEvent);
        }
        
        [ContextMenu("Test Item Collection Event")]
        private void TestItemEvent()
        {
            var itemEvent = new ItemCollectedEvent(101, "Health Potion", 1, transform.position, 1);
            EventSystem.Publish(itemEvent);
        }
        
        [ContextMenu("Test Score Event")]
        private void TestScoreEvent()
        {
            var scoreEvent = new ScoreChangedEvent(1, 1000, 1500, "Enemy Defeated");
            EventSystem.Publish(scoreEvent);
        }
        
        #endregion
    }
}