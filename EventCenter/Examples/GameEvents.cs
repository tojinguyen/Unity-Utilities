using UnityEngine;

namespace TirexGame.Utils.EventCenter.Examples
{
    /// <summary>
    /// Example game events for demonstration
    /// </summary>
    
    // Player events
    public class PlayerHealthChanged : BaseEvent
    {
        public float PreviousHealth { get; set; }
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public GameObject Player { get; set; }
        
        public override int Priority => 10; // High priority for health changes
    }
    
    public class PlayerDied : BaseEvent
    {
        public GameObject Player { get; set; }
        public Vector3 DeathPosition { get; set; }
        public string CauseOfDeath { get; set; }
        
        public override int Priority => 20; // Very high priority
        public override bool IsImmediate => true; // Process immediately
    }
    
    public class PlayerLevelUp : BaseEvent
    {
        public int PreviousLevel { get; set; }
        public int NewLevel { get; set; }
        public int ExperienceGained { get; set; }
        public GameObject Player { get; set; }
    }
    
    // Game state events
    public class GameStarted : BaseEvent
    {
        public string LevelName { get; set; }
        public int PlayerCount { get; set; }
        public float TimeStamp { get; set; }
        
        public override bool IsImmediate => true;
    }
    
    public class GamePaused : BaseEvent
    {
        public bool IsPaused { get; set; }
        public string Reason { get; set; }
    }
    
    public class ScoreChanged : BaseEvent
    {
        public int PreviousScore { get; set; }
        public int NewScore { get; set; }
        public int ScoreDelta { get; set; }
        public string ScoreReason { get; set; }
    }
    
    // UI events
    public class ButtonClicked : BaseEvent
    {
        public string ButtonName { get; set; }
        public GameObject ButtonObject { get; set; }
        public Vector2 ClickPosition { get; set; }
    }
    
    public class MenuOpened : BaseEvent
    {
        public string MenuName { get; set; }
        public GameObject MenuObject { get; set; }
        public bool FromInGame { get; set; }
    }
    
    public class MenuClosed : BaseEvent
    {
        public string MenuName { get; set; }
        public float TimeOpen { get; set; }
    }
    
    // Audio events
    public class PlaySound : BaseEvent
    {
        public string SoundName { get; set; }
        public Vector3 Position { get; set; }
        public float Volume { get; set; } = 1f;
        public bool Loop { get; set; }
        
        public override int Priority => 5; // Medium priority
    }
    
    public class PlayMusic : BaseEvent
    {
        public string MusicName { get; set; }
        public float FadeInTime { get; set; } = 0f;
        public bool Loop { get; set; } = true;
    }
    
    // Performance-critical events using FastEvent
    public class EnemySpawned : FastEvent
    {
        public GameObject Enemy { get; set; }
        public Vector3 SpawnPosition { get; set; }
        public int EnemyType { get; set; }
    }
    
    public class ProjectileFired : FastEvent
    {
        public Vector3 StartPosition { get; set; }
        public Vector3 Direction { get; set; }
        public float Speed { get; set; }
        public int DamageAmount { get; set; }
    }
    
    public class PickupCollected : FastEvent
    {
        public Vector3 Position { get; set; }
        public int PickupType { get; set; }
        public int Value { get; set; }
    }
    
    // Batch processing events
    public class DamageDealt : BaseEvent
    {
        public GameObject Target { get; set; }
        public GameObject Source { get; set; }
        public int Damage { get; set; }
        public Vector3 HitPosition { get; set; }
        public string DamageType { get; set; }
    }
    
    public class ExperienceGained : BaseEvent
    {
        public GameObject Player { get; set; }
        public int Amount { get; set; }
        public string Source { get; set; }
        public Vector3 Position { get; set; }
    }
}