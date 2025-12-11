using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TirexGame.Utils.Patterns.StateMachine;

namespace ChessDungeonCrawler.GameStates
{
    /// <summary>
    /// Game context containing all game data
    /// </summary>
    public class GameContext
    {
        public int CurrentFloor { get; set; }
        public int TotalFloors { get; set; } = 5;
        public List<Vector2Int> PlayerPosition { get; set; }
        public List<EnemyData> Enemies { get; set; }
        public List<Vector2Int> CollectedItems { get; set; }
        public int ObjectivesCleared { get; set; }
        public int ObjectivesRequired { get; set; }
        public bool BossDefeated { get; set; }
        public TurnPhase CurrentTurnPhase { get; set; }
        public ChessPieceType PlayerPieceType { get; set; }
        
        public GameContext()
        {
            CurrentFloor = 1;
            PlayerPosition = new List<Vector2Int> { new Vector2Int(0, 0) };
            Enemies = new List<EnemyData>();
            CollectedItems = new List<Vector2Int>();
            PlayerPieceType = ChessPieceType.Knight;
        }
    }
    
    public enum TurnPhase
    {
        PlayerTurn,
        EnemyTurn,
        Processing
    }
    
    public enum ChessPieceType
    {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King,
        MUnit // Special intelligent unit
    }
    
    public class EnemyData
    {
        public ChessPieceType Type { get; set; }
        public Vector2Int Position { get; set; }
        public int Health { get; set; }
        public bool IsAlive => Health > 0;
    }
    
    // ============================================
    // MAIN GAME STATE MACHINE CONTROLLER
    // ============================================
    
    public class ChessDungeonGameManager : MonoBehaviour
    {
        private StateMachine _mainStateMachine;
        private GameContext _gameContext;
        
        [Header("Game Settings")]
        [SerializeField] private int totalFloors = 5;
        
        private async void Start()
        {
            await InitializeGameAsync();
        }
        
        private async UniTask InitializeGameAsync()
        {
            // Initialize game context
            _gameContext = new GameContext
            {
                TotalFloors = totalFloors
            };
            
            // Create main state machine
            _mainStateMachine = new StateMachine(enableConsoleLoggerLogs: true);
            
            // Create and add states
            var mainMenuState = new MainMenuState();
            var gameplayState = new GameplayState();
            var pauseState = new PauseState();
            var gameOverState = new GameOverState();
            var victoryState = new VictoryState();
            
            // Initialize states with context
            mainMenuState.Initialize(_gameContext);
            gameplayState.Initialize(_gameContext);
            pauseState.Initialize(_gameContext);
            gameOverState.Initialize(_gameContext);
            victoryState.Initialize(_gameContext);
            
            _mainStateMachine.AddState(mainMenuState);
            _mainStateMachine.AddState(gameplayState);
            _mainStateMachine.AddState(pauseState);
            _mainStateMachine.AddState(gameOverState);
            _mainStateMachine.AddState(victoryState);
            
            // Setup transitions
            _mainStateMachine.AddTransition<MainMenuState, GameplayState>(
                () => Input.GetKeyDown(KeyCode.Return));
            
            _mainStateMachine.AddTransition<GameplayState, PauseState>(
                () => Input.GetKeyDown(KeyCode.Escape));
            
            _mainStateMachine.AddTransition<PauseState, GameplayState>(
                () => Input.GetKeyDown(KeyCode.Escape));
            
            _mainStateMachine.AddTransition<GameplayState, GameOverState>(
                () => _gameContext.PlayerPosition.Count == 0);
            
            _mainStateMachine.AddTransition<GameplayState, VictoryState>(
                () => _gameContext.CurrentFloor > _gameContext.TotalFloors && 
                      _gameContext.BossDefeated);
            
            _mainStateMachine.AddTransition<GameOverState, MainMenuState>(
                () => Input.GetKeyDown(KeyCode.R));
            
            _mainStateMachine.AddTransition<VictoryState, MainMenuState>(
                () => Input.GetKeyDown(KeyCode.Return));
            
            // Start with main menu
            await _mainStateMachine.StartAsync<MainMenuState>();
        }
        
        private async void Update()
        {
            if (_mainStateMachine != null)
            {
                await _mainStateMachine.CheckTransitionsAsync();
                _mainStateMachine.Tick();
            }
        }
        
        private async void OnDestroy()
        {
            if (_mainStateMachine != null)
            {
                await _mainStateMachine.StopAsync();
            }
        }
    }
    
    // ============================================
    // TOP LEVEL STATES
    // ============================================
    
    /// <summary>
    /// Main Menu State
    /// </summary>
    public class MainMenuState : BaseState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log("=== CHESS DUNGEON CRAWLER ===");
            ConsoleLogger.Log("Press ENTER to start");
            ConsoleLogger.Log("Navigate the dungeon using chess piece movements");
            await base.OnEnter();
        }
    }
    
    /// <summary>
    /// Gameplay State - Contains sub-states for different gameplay phases
    /// </summary>
    public class GameplayState : BaseTickableCompositeState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log($"Starting Floor {Context.CurrentFloor}/{Context.TotalFloors}");
            
            // Setup sub-states
            SetupSubStates();
            
            await base.OnEnter();
        }
        
        private void SetupSubStates()
        {
            // Create floor sub-states
            var floorEntryState = new FloorEntryState();
            var explorationState = new ExplorationState();
            var bossState = new BossState();
            var floorCompleteState = new FloorCompleteState();
            
            // Initialize with context
            floorEntryState.Initialize(Context);
            explorationState.Initialize(Context);
            bossState.Initialize(Context);
            floorCompleteState.Initialize(Context);
            
            // Add sub-states
            AddSubState(floorEntryState, isInitial: true);
            AddSubState(explorationState);
            AddSubState(bossState);
            AddSubState(floorCompleteState);
            
            // Setup sub-state transitions
            AddSubTransition<FloorEntryState, ExplorationState>(
                () => Context.CurrentFloor < Context.TotalFloors);
            
            AddSubTransition<FloorEntryState, BossState>(
                () => Context.CurrentFloor == Context.TotalFloors);
            
            AddSubTransition<ExplorationState, FloorCompleteState>(
                () => Context.ObjectivesCleared >= Context.ObjectivesRequired);
            
            AddSubTransition<BossState, FloorCompleteState>(
                () => Context.BossDefeated);
        }
        
        public override void OnTick()
        {
            base.OnTick();
            
            // Display floor info
            if (Time.frameCount % 60 == 0) // Every second
            {
                ConsoleLogger.Log($"Floor: {Context.CurrentFloor} | " +
                         $"Objectives: {Context.ObjectivesCleared}/{Context.ObjectivesRequired} | " +
                         $"Enemies: {Context.Enemies.FindAll(e => e.IsAlive).Count}");
            }
        }
    }
    
    /// <summary>
    /// Pause State
    /// </summary>
    public class PauseState : BaseState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log("=== GAME PAUSED ===");
            ConsoleLogger.Log("Press ESC to resume");
            Time.timeScale = 0f;
            await base.OnEnter();
        }
        
        public override async UniTask OnExit()
        {
            Time.timeScale = 1f;
            await base.OnExit();
        }
    }
    
    /// <summary>
    /// Game Over State
    /// </summary>
    public class GameOverState : BaseState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log("=== GAME OVER ===");
            ConsoleLogger.Log($"You reached Floor {Context.CurrentFloor}/{Context.TotalFloors}");
            ConsoleLogger.Log("Press R to return to Main Menu");
            await base.OnEnter();
        }
    }
    
    /// <summary>
    /// Victory State
    /// </summary>
    public class VictoryState : BaseState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log("=== VICTORY! ===");
            ConsoleLogger.Log($"You conquered all {Context.TotalFloors} floors!");
            ConsoleLogger.Log($"Items collected: {Context.CollectedItems.Count}");
            ConsoleLogger.Log("Press ENTER to return to Main Menu");
            await base.OnEnter();
        }
    }
    
    // ============================================
    // GAMEPLAY SUB-STATES
    // ============================================
    
    /// <summary>
    /// Floor Entry - Initialize floor data
    /// </summary>
    public class FloorEntryState : BaseState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log($"╔══════════════════════════════╗");
            ConsoleLogger.Log($"║   ENTERING FLOOR {Context.CurrentFloor}          ║");
            ConsoleLogger.Log($"╚══════════════════════════════╝");
            
            // Initialize floor
            InitializeFloor();
            
            await base.OnEnter();
            
            // Auto-transition after initialization
            await UniTask.Delay(1000);
        }
        
        private void InitializeFloor()
        {
            // Reset objectives
            Context.ObjectivesCleared = 0;
            Context.ObjectivesRequired = 3 + Context.CurrentFloor;
            
            // Spawn enemies based on floor
            Context.Enemies.Clear();
            SpawnEnemies();
            
            // Reset player position
            Context.PlayerPosition = new List<Vector2Int> { new Vector2Int(0, 0) };
            
            ConsoleLogger.Log($"Objectives required: {Context.ObjectivesRequired}");
            ConsoleLogger.Log($"Enemies spawned: {Context.Enemies.Count}");
        }
        
        private void SpawnEnemies()
        {
            int enemyCount = 3 + Context.CurrentFloor * 2;
            
            for (int i = 0; i < enemyCount; i++)
            {
                var enemyType = (ChessPieceType)(i % 6);
                Context.Enemies.Add(new EnemyData
                {
                    Type = enemyType,
                    Position = new Vector2Int(
                        UnityEngine.Random.Range(1, 8),
                        UnityEngine.Random.Range(1, 8)
                    ),
                    Health = 1 + Context.CurrentFloor / 2
                });
            }
            
            // Add M-Unit (intelligent enemy)
            if (Context.CurrentFloor >= 2)
            {
                Context.Enemies.Add(new EnemyData
                {
                    Type = ChessPieceType.MUnit,
                    Position = new Vector2Int(
                        UnityEngine.Random.Range(1, 8),
                        UnityEngine.Random.Range(1, 8)
                    ),
                    Health = 3
                });
                ConsoleLogger.Log("⚠️ M-Unit spawned! This enemy will strategize against you!");
            }
        }
    }
    
    /// <summary>
    /// Exploration State - Main gameplay with turn-based combat
    /// Contains nested sub-states for turn phases
    /// </summary>
    public class ExplorationState : BaseTickableCompositeState<GameContext>
    {
        private float _turnTimer;
        private const float TurnDuration = 1.5f;
        
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log(">>> Exploration Phase Started");
            ConsoleLogger.Log("Move using chess piece rules. Clear objectives to proceed.");
            
            // Setup turn-based sub-states
            SetupTurnStates();
            
            await base.OnEnter();
        }
        
        private void SetupTurnStates()
        {
            var playerTurnState = new PlayerTurnState();
            var enemyTurnState = new EnemyTurnState();
            var processingTurnState = new ProcessingTurnState();
            
            playerTurnState.Initialize(Context);
            enemyTurnState.Initialize(Context);
            processingTurnState.Initialize(Context);
            
            AddSubState(playerTurnState, isInitial: true);
            AddSubState(enemyTurnState);
            AddSubState(processingTurnState);
            
            // Turn cycle transitions
            AddSubTransition<PlayerTurnState, ProcessingTurnState>(
                () => Context.CurrentTurnPhase == TurnPhase.Processing);
            
            AddSubTransition<ProcessingTurnState, EnemyTurnState>(
                () => Context.CurrentTurnPhase == TurnPhase.EnemyTurn);
            
            AddSubTransition<EnemyTurnState, PlayerTurnState>(
                () => Context.CurrentTurnPhase == TurnPhase.PlayerTurn);
        }
        
        public override void OnTick()
        {
            base.OnTick();
            
            _turnTimer += Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Boss State - Final floor boss encounter
    /// </summary>
    public class BossState : BaseTickableCompositeState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log("╔═══════════════════════════════╗");
            ConsoleLogger.Log("║   ⚔️  BOSS CHAMBER  ⚔️        ║");
            ConsoleLogger.Log("╚═══════════════════════════════╝");
            
            // Spawn boss
            SpawnBoss();
            
            // Setup boss fight sub-states
            SetupBossFightStates();
            
            await base.OnEnter();
        }
        
        private void SpawnBoss()
        {
            Context.Enemies.Clear();
            Context.Enemies.Add(new EnemyData
            {
                Type = ChessPieceType.Queen, // Boss uses Queen movement
                Position = new Vector2Int(4, 7),
                Health = 10
            });
            
            ConsoleLogger.Log("👑 Boss spawned: The Chess Queen!");
            ConsoleLogger.Log("Health: 10 | Moves like a Queen piece");
        }
        
        private void SetupBossFightStates()
        {
            var bossCombatState = new BossCombatState();
            var bossDefeatState = new BossDefeatState();
            
            bossCombatState.Initialize(Context);
            bossDefeatState.Initialize(Context);
            
            AddSubState(bossCombatState, isInitial: true);
            AddSubState(bossDefeatState);
            
            AddSubTransition<BossCombatState, BossDefeatState>(
                () => Context.Enemies.Count == 0 || !Context.Enemies[0].IsAlive);
        }
    }
    
    /// <summary>
    /// Floor Complete State
    /// </summary>
    public class FloorCompleteState : BaseState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log("✓ Floor Complete!");
            
            Context.CurrentFloor++;
            
            if (Context.CurrentFloor <= Context.TotalFloors)
            {
                ConsoleLogger.Log($"Advancing to Floor {Context.CurrentFloor}...");
                await UniTask.Delay(2000);
            }
            else
            {
                Context.BossDefeated = true;
                ConsoleLogger.Log("All floors cleared! Returning to victory...");
            }
            
            await base.OnEnter();
        }
    }
    
    // ============================================
    // TURN-BASED SUB-STATES
    // ============================================
    
    /// <summary>
    /// Player Turn State
    /// </summary>
    public class PlayerTurnState : BaseTickableState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log("→ Your Turn");
            Context.CurrentTurnPhase = TurnPhase.PlayerTurn;
            await base.OnEnter();
        }
        
        public override void OnTick()
        {
            // Handle player input for chess piece movement
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ConsoleLogger.Log($"  Player moves ({Context.PlayerPieceType})");
                SimulatePlayerMove();
                Context.CurrentTurnPhase = TurnPhase.Processing;
            }
        }
        
        private void SimulatePlayerMove()
        {
            // Simulate collecting objective or defeating enemy
            if (UnityEngine.Random.value > 0.5f)
            {
                Context.ObjectivesCleared++;
                ConsoleLogger.Log($"  ✓ Objective cleared! ({Context.ObjectivesCleared}/{Context.ObjectivesRequired})");
            }
        }
    }
    
    /// <summary>
    /// Processing Turn State
    /// </summary>
    public class ProcessingTurnState : BaseState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            Context.CurrentTurnPhase = TurnPhase.Processing;
            await UniTask.Delay(500); // Process animations, effects
            Context.CurrentTurnPhase = TurnPhase.EnemyTurn;
            await base.OnEnter();
        }
    }
    
    /// <summary>
    /// Enemy Turn State
    /// </summary>
    public class EnemyTurnState : BaseState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log("→ Enemy Turn");
            Context.CurrentTurnPhase = TurnPhase.EnemyTurn;
            
            await ProcessEnemyTurns();
            
            Context.CurrentTurnPhase = TurnPhase.PlayerTurn;
            await base.OnEnter();
        }
        
        private async UniTask ProcessEnemyTurns()
        {
            foreach (var enemy in Context.Enemies)
            {
                if (!enemy.IsAlive) continue;
                
                if (enemy.Type == ChessPieceType.MUnit)
                {
                    ConsoleLogger.Log("  🧠 M-Unit strategizing...");
                    await UniTask.Delay(300);
                    ProcessMUnitMove(enemy);
                }
                else
                {
                    ProcessStandardEnemyMove(enemy);
                }
                
                await UniTask.Delay(200);
            }
        }
        
        private void ProcessMUnitMove(EnemyData mUnit)
        {
            // M-Unit uses AI to make strategic decisions
            ConsoleLogger.Log($"  M-Unit moves strategically from {mUnit.Position}");
            
            // Calculate best position (simplified)
            var playerPos = Context.PlayerPosition[0];
            var direction = new Vector2Int(
                Mathf.Clamp(playerPos.x - mUnit.Position.x, -1, 1),
                Mathf.Clamp(playerPos.y - mUnit.Position.y, -1, 1)
            );
            
            mUnit.Position += direction;
            ConsoleLogger.Log($"  → New position: {mUnit.Position}");
        }
        
        private void ProcessStandardEnemyMove(EnemyData enemy)
        {
            ConsoleLogger.Log($"  {enemy.Type} moves according to chess rules");
        }
    }
    
    // ============================================
    // BOSS FIGHT SUB-STATES
    // ============================================
    
    /// <summary>
    /// Boss Combat State
    /// </summary>
    public class BossCombatState : BaseTickableState<GameContext>
    {
        private int _turnCount;
        
        public override void OnTick()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _turnCount++;
                ConsoleLogger.Log($"⚔️ Boss Turn {_turnCount}");
                
                // Damage boss
                if (Context.Enemies.Count > 0)
                {
                    Context.Enemies[0].Health--;
                    ConsoleLogger.Log($"Boss Health: {Context.Enemies[0].Health}/10");
                    
                    if (Context.Enemies[0].Health <= 0)
                    {
                        Context.BossDefeated = true;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Boss Defeat State
    /// </summary>
    public class BossDefeatState : BaseState<GameContext>
    {
        public override async UniTask OnEnter()
        {
            ConsoleLogger.Log("╔═══════════════════════════════╗");
            ConsoleLogger.Log("║   👑 BOSS DEFEATED! 👑        ║");
            ConsoleLogger.Log("╚═══════════════════════════════╝");
            
            await UniTask.Delay(2000);
            await base.OnEnter();
        }
    }
}