using UnityEngine;
using Cysharp.Threading.Tasks;
using TirexGame.Utils.Patterns.StateMachine;

namespace TirexGame.Utils.Patterns.Examples
{
    /// <summary>
    /// Example game states for a simple game
    /// </summary>
    public class GameStates
    {
        public class MenuState : BaseState<GameController>
        {
            public override string StateName => "Menu";
            
            public override async UniTask OnEnter()
            {
                Debug.Log("Entered Menu State");
                // Show menu UI
                if (Context != null)
                {
                    await Context.ShowMenu();
                }
            }
            
            public override void OnUpdate()
            {
                // Handle menu input
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Context?.StartGame();
                }
            }
            
            public override async UniTask OnExit()
            {
                Debug.Log("Exited Menu State");
                // Hide menu UI
                if (Context != null)
                {
                    await Context.HideMenu();
                }
            }
        }
        
        public class PlayingState : BaseState<GameController>
        {
            public override string StateName => "Playing";
            
            public override async UniTask OnEnter()
            {
                Debug.Log("Entered Playing State");
                // Initialize game
                if (Context != null)
                {
                    await Context.InitializeGame();
                }
            }
            
            public override void OnUpdate()
            {
                // Handle game input and logic
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Context?.PauseGame();
                }
                
                if (Context != null && Context.IsGameOver())
                {
                    Context.EndGame();
                }
            }
            
            public override async UniTask OnExit()
            {
                Debug.Log("Exited Playing State");
                // Cleanup game
                if (Context != null)
                {
                    await Context.CleanupGame();
                }
            }
        }
        
        public class PausedState : BaseState<GameController>
        {
            public override string StateName => "Paused";
            
            public override async UniTask OnEnter()
            {
                Debug.Log("Entered Paused State");
                // Pause game and show pause menu
                if (Context != null)
                {
                    await Context.ShowPauseMenu();
                    Time.timeScale = 0f;
                }
            }
            
            public override void OnUpdate()
            {
                // Handle pause menu input
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Context?.ResumeGame();
                }
            }
            
            public override async UniTask OnExit()
            {
                Debug.Log("Exited Paused State");
                // Resume game and hide pause menu
                if (Context != null)
                {
                    await Context.HidePauseMenu();
                    Time.timeScale = 1f;
                }
            }
        }
        
        public class GameOverState : BaseState<GameController>
        {
            public override string StateName => "GameOver";
            
            public override async UniTask OnEnter()
            {
                Debug.Log("Entered GameOver State");
                // Show game over screen
                if (Context != null)
                {
                    await Context.ShowGameOverScreen();
                }
            }
            
            public override void OnUpdate()
            {
                // Handle game over input
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Context?.RestartGame();
                }
                
                if (Input.GetKeyDown(KeyCode.M))
                {
                    Context?.ReturnToMenu();
                }
            }
            
            public override async UniTask OnExit()
            {
                Debug.Log("Exited GameOver State");
                // Hide game over screen
                if (Context != null)
                {
                    await Context.HideGameOverScreen();
                }
            }
        }
    }
    
    /// <summary>
    /// Example game controller using state machine
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("State Machine")]
        [SerializeField] private StateMachine.StateMachine stateMachine;
        
        [Header("Game Settings")]
        [SerializeField] private int playerHealth = 100;
        [SerializeField] private float gameTime = 0f;
        
        private GameStates.MenuState _menuState;
        private GameStates.PlayingState _playingState;
        private GameStates.PausedState _pausedState;
        private GameStates.GameOverState _gameOverState;
        
        private void Awake()
        {
            // Initialize states
            _menuState = new GameStates.MenuState();
            _playingState = new GameStates.PlayingState();
            _pausedState = new GameStates.PausedState();
            _gameOverState = new GameStates.GameOverState();
            
            // Initialize states with context
            _menuState.Initialize(this);
            _playingState.Initialize(this);
            _pausedState.Initialize(this);
            _gameOverState.Initialize(this);
        }
        
        private async void Start()
        {
            await SetupStateMachine();
        }
        
        private async UniTask SetupStateMachine()
        {
            // Add states to state machine
            stateMachine.AddState(_menuState);
            stateMachine.AddState(_playingState);
            stateMachine.AddState(_pausedState);
            stateMachine.AddState(_gameOverState);
            
            // Add transitions
            stateMachine.AddTransition("Menu", "Playing");
            stateMachine.AddTransition("Playing", "Paused");
            stateMachine.AddTransition("Playing", "GameOver");
            stateMachine.AddTransition("Paused", "Playing");
            stateMachine.AddTransition("GameOver", "Menu");
            stateMachine.AddTransition("GameOver", "Playing");
            
            // Start with menu state
            await stateMachine.StartAsync("Menu");
        }
        
        // State transition methods
        public void StartGame()
        {
            stateMachine.TransitionToAsync("Playing").Forget();
        }
        
        public void PauseGame()
        {
            stateMachine.TransitionToAsync("Paused").Forget();
        }
        
        public void ResumeGame()
        {
            stateMachine.TransitionToAsync("Playing").Forget();
        }
        
        public void EndGame()
        {
            stateMachine.TransitionToAsync("GameOver").Forget();
        }
        
        public void RestartGame()
        {
            playerHealth = 100;
            gameTime = 0f;
            stateMachine.TransitionToAsync("Playing").Forget();
        }
        
        public void ReturnToMenu()
        {
            stateMachine.TransitionToAsync("Menu").Forget();
        }
        
        // Game logic methods
        public bool IsGameOver()
        {
            return playerHealth <= 0;
        }
        
        public void TakeDamage(int damage)
        {
            playerHealth -= damage;
            playerHealth = Mathf.Max(0, playerHealth);
        }
        
        // UI methods (implement based on your UI system)
        public async UniTask ShowMenu()
        {
            // Implementation depends on your UI system
            Debug.Log("Showing menu...");
            await UniTask.Delay(100);
        }
        
        public async UniTask HideMenu()
        {
            Debug.Log("Hiding menu...");
            await UniTask.Delay(100);
        }
        
        public async UniTask InitializeGame()
        {
            Debug.Log("Initializing game...");
            playerHealth = 100;
            gameTime = 0f;
            await UniTask.Delay(100);
        }
        
        public async UniTask CleanupGame()
        {
            Debug.Log("Cleaning up game...");
            await UniTask.Delay(100);
        }
        
        public async UniTask ShowPauseMenu()
        {
            Debug.Log("Showing pause menu...");
            await UniTask.Delay(100);
        }
        
        public async UniTask HidePauseMenu()
        {
            Debug.Log("Hiding pause menu...");
            await UniTask.Delay(100);
        }
        
        public async UniTask ShowGameOverScreen()
        {
            Debug.Log("Showing game over screen...");
            await UniTask.Delay(100);
        }
        
        public async UniTask HideGameOverScreen()
        {
            Debug.Log("Hiding game over screen...");
            await UniTask.Delay(100);
        }
    }
}
