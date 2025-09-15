using UnityEngine;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Example struct event for player health changes
    /// </summary>
    public struct PlayerHealthEvent
    {
        public int PlayerId;
        public float CurrentHealth;
        public float MaxHealth;
        public float Damage;
        public Vector3 Position;
        
        public PlayerHealthEvent(int playerId, float currentHealth, float maxHealth, float damage, Vector3 position)
        {
            PlayerId = playerId;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            Damage = damage;
            Position = position;
        }
        
        public float HealthPercentage => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
        public bool IsAlive => CurrentHealth > 0;
    }
    
    /// <summary>
    /// Example struct event for item collection
    /// </summary>
    public struct ItemCollectedEvent
    {
        public int ItemId;
        public string ItemName;
        public int Quantity;
        public Vector3 CollectionPosition;
        public int CollectorId;
        
        public ItemCollectedEvent(int itemId, string itemName, int quantity, Vector3 position, int collectorId)
        {
            ItemId = itemId;
            ItemName = itemName;
            Quantity = quantity;
            CollectionPosition = position;
            CollectorId = collectorId;
        }
    }
    
    /// <summary>
    /// Example struct event for score changes
    /// </summary>
    public struct ScoreChangedEvent
    {
        public int PlayerId;
        public int PreviousScore;
        public int NewScore;
        public int ScoreDelta;
        public string Reason;
        
        public ScoreChangedEvent(int playerId, int previousScore, int newScore, string reason)
        {
            PlayerId = playerId;
            PreviousScore = previousScore;
            NewScore = newScore;
            ScoreDelta = newScore - previousScore;
            Reason = reason;
        }
    }
    
    /// <summary>
    /// Example struct event for weapon firing
    /// </summary>
    public struct WeaponFiredEvent
    {
        public int WeaponId;
        public Vector3 FirePosition;
        public Vector3 FireDirection;
        public float Damage;
        public int AmmoRemaining;
        public int ShooterId;
        
        public WeaponFiredEvent(int weaponId, Vector3 firePosition, Vector3 fireDirection, 
                               float damage, int ammoRemaining, int shooterId)
        {
            WeaponId = weaponId;
            FirePosition = firePosition;
            FireDirection = fireDirection;
            Damage = damage;
            AmmoRemaining = ammoRemaining;
            ShooterId = shooterId;
        }
    }
    
    /// <summary>
    /// Example struct event for level progression
    /// </summary>
    public struct LevelCompletedEvent
    {
        public int LevelId;
        public string LevelName;
        public float CompletionTime;
        public int CollectedItems;
        public int TotalItems;
        public bool IsPerfectRun;
        
        public LevelCompletedEvent(int levelId, string levelName, float completionTime, 
                                  int collectedItems, int totalItems)
        {
            LevelId = levelId;
            LevelName = levelName;
            CompletionTime = completionTime;
            CollectedItems = collectedItems;
            TotalItems = totalItems;
            IsPerfectRun = collectedItems >= totalItems;
        }
        
        public float CompletionPercentage => TotalItems > 0 ? (float)CollectedItems / TotalItems : 0f;
    }
}