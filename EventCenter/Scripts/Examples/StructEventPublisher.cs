using UnityEngine;
using System.Collections;
using TirexGame.Utils.EventCenter;
using TirexGame.Utils.EventCenter.Examples;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Example component that publishes struct events for performance testing
    /// </summary>
    public class StructEventPublisher : MonoBehaviour
    {
        [Header("Performance Test Settings")]
        [SerializeField] private int eventsPerFrame = 1000;
        [SerializeField] private bool continuousPublishing = false;
        [SerializeField] private float publishInterval = 0.1f;
        
        [Header("Event Settings")]
        [SerializeField] private bool publishHealthEvents = true;
        [SerializeField] private bool publishItemEvents = true;
        [SerializeField] private bool publishScoreEvents = true;
        [SerializeField] private bool publishWeaponEvents = true;
        [SerializeField] private bool publishLevelEvents = false;
        
        private IEventCenter _eventCenter;
        private Coroutine _publishingCoroutine;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            _eventCenter = EventCenterService.Instance.GetEventCenter();
            
            if (continuousPublishing)
            {
                StartContinuousPublishing();
            }
        }
        
        private void OnDestroy()
        {
            StopContinuousPublishing();
        }
        
        #endregion
        
        #region Public Methods
        
        [ContextMenu("Publish Single Batch")]
        public void PublishSingleBatch()
        {
            var startTime = Time.realtimeSinceStartup;
            
            for (int i = 0; i < eventsPerFrame; i++)
            {
                PublishRandomEvents();
            }
            
            var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
            Debug.Log($"Published {eventsPerFrame} struct events in {duration:F2}ms " +
                     $"({eventsPerFrame / duration * 1000f:F0} events/second)");
        }
        
        [ContextMenu("Start Continuous Publishing")]
        public void StartContinuousPublishing()
        {
            if (_publishingCoroutine == null)
            {
                _publishingCoroutine = StartCoroutine(ContinuousPublishingCoroutine());
                Debug.Log("Started continuous struct event publishing");
            }
        }
        
        [ContextMenu("Stop Continuous Publishing")]
        public void StopContinuousPublishing()
        {
            if (_publishingCoroutine != null)
            {
                StopCoroutine(_publishingCoroutine);
                _publishingCoroutine = null;
                Debug.Log("Stopped continuous struct event publishing");
            }
        }
        
        [ContextMenu("Publish Health Event")]
        public void PublishHealthEvent()
        {
            var healthEvent = new PlayerHealthEvent(
                Random.Range(1, 5),
                Random.Range(0f, 100f),
                100f,
                Random.Range(5f, 25f),
                transform.position + Random.insideUnitSphere * 10f
            );
            
            _eventCenter.PublishEvent(healthEvent);
        }
        
        [ContextMenu("Publish Item Event")]
        public void PublishItemEvent()
        {
            var items = new[] { "Health Potion", "Mana Potion", "Sword", "Shield", "Key" };
            var itemEvent = new ItemCollectedEvent(
                Random.Range(100, 200),
                items[Random.Range(0, items.Length)],
                Random.Range(1, 5),
                transform.position + Random.insideUnitSphere * 10f,
                Random.Range(1, 5)
            );
            
            _eventCenter.PublishEvent(itemEvent);
        }
        
        [ContextMenu("Publish Score Event")]
        public void PublishScoreEvent()
        {
            var reasons = new[] { "Enemy Defeated", "Item Collected", "Quest Completed", "Bonus Round" };
            var previousScore = Random.Range(0, 10000);
            var scoreEvent = new ScoreChangedEvent(
                Random.Range(1, 5),
                previousScore,
                previousScore + Random.Range(100, 1000),
                reasons[Random.Range(0, reasons.Length)]
            );
            
            _eventCenter.PublishEvent(scoreEvent);
        }
        
        [ContextMenu("Publish Weapon Event")]
        public void PublishWeaponEvent()
        {
            var weaponEvent = new WeaponFiredEvent(
                Random.Range(1, 10),
                transform.position,
                Random.onUnitSphere,
                Random.Range(10f, 50f),
                Random.Range(0, 30),
                Random.Range(1, 5)
            );
            
            _eventCenter.PublishEvent(weaponEvent);
        }
        
        [ContextMenu("Publish Level Event")]
        public void PublishLevelEvent()
        {
            var levelEvent = new LevelCompletedEvent(
                Random.Range(1, 20),
                $"Level {Random.Range(1, 20)}",
                Random.Range(30f, 300f),
                Random.Range(50, 100),
                100
            );
            
            _eventCenter.PublishEvent(levelEvent);
        }
        
        #endregion
        
        #region Private Methods
        
        private void PublishRandomEvents()
        {
            if (publishHealthEvents && Random.value < 0.3f)
                PublishHealthEvent();
                
            if (publishItemEvents && Random.value < 0.2f)
                PublishItemEvent();
                
            if (publishScoreEvents && Random.value < 0.3f)
                PublishScoreEvent();
                
            if (publishWeaponEvents && Random.value < 0.4f)
                PublishWeaponEvent();
                
            if (publishLevelEvents && Random.value < 0.1f)
                PublishLevelEvent();
        }
        
        private IEnumerator ContinuousPublishingCoroutine()
        {
            while (true)
            {
                var startTime = Time.realtimeSinceStartup;
                
                for (int i = 0; i < eventsPerFrame; i++)
                {
                    PublishRandomEvents();
                }
                
                var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
                
                if (duration > 16.67f) // More than one frame at 60fps
                {
                    Debug.LogWarning($"Event publishing took {duration:F2}ms - consider reducing eventsPerFrame");
                }
                
                yield return new WaitForSeconds(publishInterval);
            }
        }
        
        #endregion
    }