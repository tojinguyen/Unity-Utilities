using System.Collections.Generic;
using UnityEngine;
using TirexGame.Utils.EventCenter.Examples;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Example demonstrating basic EventCenter usage
    /// </summary>
    public class BasicEventExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private float healthDecreaseRate = 10f;
        [SerializeField] private float maxHealth = 100f;
        
        private float currentHealth;
        private IEventSubscription healthSubscription;
        private IEventSubscription deathSubscription;
        
        void Start()
        {
            currentHealth = maxHealth;
            
            // Ensure EventCenter service is available
            if (!EventCenterService.EnsureAvailable())
            {
                Debug.LogError("Failed to initialize EventCenter service");
                return;
            }
            
            // Subscribe to health change events
            healthSubscription = EventCenterService.Subscribe<PlayerHealthChanged>(OnPlayerHealthChanged);
            
            // Subscribe to death events with higher priority
            deathSubscription = EventCenterService.Subscribe<PlayerDied>(OnPlayerDied, priority: 100);
            
            Log("Basic Event Example started - subscribed to events");
        }
        
        void Update()
        {
            // Simulate health decrease
            if (currentHealth > 0)
            {
                float previousHealth = currentHealth;
                currentHealth -= healthDecreaseRate * Time.deltaTime;
                
                // Publish health change event
                var healthEvent = EventCenterService.CreateAndPublish<PlayerHealthChanged>(this);
                healthEvent.PreviousHealth = previousHealth;
                healthEvent.CurrentHealth = currentHealth;
                healthEvent.MaxHealth = maxHealth;
                healthEvent.Player = gameObject;
                
                // Check for death
                if (currentHealth <= 0 && previousHealth > 0)
                {
                    // Publish death event immediately
                    var deathEvent = EventCenterService.CreateAndPublish<PlayerDied>(this);
                    deathEvent.Player = gameObject;
                    deathEvent.DeathPosition = transform.position;
                    deathEvent.CauseOfDeath = "Health depleted";
                    
                    EventCenterService.PublishImmediate(deathEvent);
                }
            }
        }
        
        private void OnPlayerHealthChanged(PlayerHealthChanged eventData)
        {
            if (eventData.Player == gameObject)
            {
                Log($"Health changed: {eventData.PreviousHealth:F1} -> {eventData.CurrentHealth:F1}");\n                \n                // Update UI, play effects, etc.\n                UpdateHealthUI();\n                \n                if (eventData.CurrentHealth < eventData.PreviousHealth)\n                {\n                    PlayDamageEffect();\n                }\n            }\n        }\n        \n        private void OnPlayerDied(PlayerDied eventData)\n        {\n            if (eventData.Player == gameObject)\n            {\n                Log($"Player died at {eventData.DeathPosition} - {eventData.CauseOfDeath}");\n                \n                // Handle death logic\n                HandlePlayerDeath();\n            }\n        }\n        \n        private void UpdateHealthUI()\n        {\n            // Update health bar, etc.\n            Log($"Updated health UI: {currentHealth:F1}/{maxHealth}");\n        }\n        \n        private void PlayDamageEffect()\n        {\n            // Play damage visual/audio effects\n            Log("Playing damage effect");\n        }\n        \n        private void HandlePlayerDeath()\n        {\n            // Handle player death\n            Log("Handling player death - stopping health decrease");\n            \n            // Stop this example\n            enabled = false;\n        }\n        \n        private void OnDestroy()\n        {\n            // Clean up subscriptions\n            healthSubscription?.Dispose();\n            deathSubscription?.Dispose();\n            \n            Log("Basic Event Example destroyed - unsubscribed from events");\n        }\n        \n        private void Log(string message)\n        {\n            if (enableLogging)\n                Debug.Log($"[BasicEventExample] {message}");\n        }\n    }\n}