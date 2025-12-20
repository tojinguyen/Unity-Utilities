using UnityEngine;
using Cysharp.Threading.Tasks;
using TirexGame.Utils.Patterns.StateMachine;

namespace TirexGame.Utils.Patterns.StateMachine.Example
{
    /// <summary>
    /// Example demonstrating SimpleStateMachine with enum states and builder pattern
    /// </summary>
    public class SimpleStateMachineExample : MonoBehaviour
    {
        // Define states as enums
        public enum PlayerState
        {
            Idle,
            Moving,
            Combat,
            Dead
        }
        
        public enum CombatState
        {
            Attacking,
            Defending,
            Casting
        }
        
        private SimpleStateMachine<PlayerState> _playerStateMachine;
        private int _health = 100;
        private float _moveSpeed;
        
        private async void Start()
        {
            await SetupStateMachine();
        }
        
        private async UniTask SetupStateMachine()
        {
            // Create state machine with logging enabled
            _playerStateMachine = new SimpleStateMachine<PlayerState>(enableLogs: true);
            
            // Configure states using builder pattern
            _playerStateMachine
                .State(PlayerState.Idle)
                    .OnEnter(() => Debug.Log("Player is now idle"))
                    .OnExit(() => Debug.Log("Player leaving idle state"))
                    .OnTick(() => 
                    {
                        // Check for movement input
                        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                            Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                        {
                            _moveSpeed = Input.GetKey(KeyCode.LeftShift) ? 10f : 5f;
                        }
                    })
                    .TransitionTo(PlayerState.Moving, () => 
                        Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                    .TransitionTo(PlayerState.Combat, () => Input.GetKeyDown(KeyCode.Space))
                    .TransitionTo(PlayerState.Dead, () => _health <= 0);
            
            _playerStateMachine
                .State(PlayerState.Moving)
                    .OnEnter(() => Debug.Log($"Player started moving (speed: {_moveSpeed})"))
                    .OnExit(() => Debug.Log("Player stopped moving"))
                    .OnTick(() =>
                    {
                        // Simulate movement
                        Vector3 movement = Vector3.zero;
                        if (Input.GetKey(KeyCode.W)) movement += Vector3.forward;
                        if (Input.GetKey(KeyCode.S)) movement += Vector3.back;
                        if (Input.GetKey(KeyCode.A)) movement += Vector3.left;
                        if (Input.GetKey(KeyCode.D)) movement += Vector3.right;
                        
                        if (movement != Vector3.zero)
                        {
                            transform.position += movement.normalized * _moveSpeed * Time.deltaTime;
                        }
                    })
                    .TransitionTo(PlayerState.Idle, () => 
                        !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && 
                        !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                    .TransitionTo(PlayerState.Combat, () => Input.GetKeyDown(KeyCode.Space))
                    .TransitionTo(PlayerState.Dead, () => _health <= 0);
            
            // Combat state with sub-states (composite state)
            _playerStateMachine
                .State(PlayerState.Combat)
                    .OnEnter(() => Debug.Log("=== ENTERED COMBAT ==="))
                    .OnExit(() => Debug.Log("=== LEFT COMBAT ==="))
                    .TransitionTo(PlayerState.Idle, () => Input.GetKeyDown(KeyCode.Escape))
                    .TransitionTo(PlayerState.Dead, () => _health <= 0)
                    .WithSubStates(CombatState.Attacking) // Initial sub-state
                        .SubState(CombatState.Attacking)
                            .OnEnter(() => Debug.Log("  > Ready to attack"))
                            .OnExit(() => Debug.Log("  < Stopped attacking"))
                            .OnTick(() =>
                            {
                                if (Input.GetMouseButtonDown(0))
                                {
                                    Debug.Log("  * Attack executed!");
                                    _health -= 5; // Simulate damage taken
                                }
                            })
                            .TransitionTo(CombatState.Defending, () => Input.GetKey(KeyCode.LeftControl))
                            .TransitionTo(CombatState.Casting, () => Input.GetKeyDown(KeyCode.E))
                        .And()
                        .SubState(CombatState.Defending)
                            .OnEnter(() => Debug.Log("  > Defending (blocking damage)"))
                            .OnExit(() => Debug.Log("  < Stopped defending"))
                            .OnTick(() =>
                            {
                                // Simulate defense
                                if (Input.GetMouseButtonDown(0))
                                {
                                    Debug.Log("  * Blocked attack!");
                                }
                            })
                            .TransitionTo(CombatState.Attacking, () => !Input.GetKey(KeyCode.LeftControl))
                        .And()
                        .SubState(CombatState.Casting)
                            .OnEnter(async () =>
                            {
                                Debug.Log("  > Casting spell...");
                                await UniTask.Delay(1000); // Simulate cast time
                                Debug.Log("  * Spell cast complete!");
                            })
                            .OnExit(() => Debug.Log("  < Spell casting interrupted"))
                            .TransitionTo(CombatState.Attacking, () => Input.GetKeyDown(KeyCode.Q))
                        .And()
                        .EndSubStates(); // Return to parent state builder
            
            _playerStateMachine
                .State(PlayerState.Dead)
                    .OnEnter(() => 
                    {
                        Debug.Log("=== PLAYER DIED ===");
                        Debug.Log("Press R to restart");
                    })
                    .TransitionTo(PlayerState.Idle, () => 
                    {
                        if (Input.GetKeyDown(KeyCode.R))
                        {
                            _health = 100;
                            return true;
                        }
                        return false;
                    });
            
            // Start with Idle state
            await _playerStateMachine.StartAsync(PlayerState.Idle);
            
            Debug.Log("=== CONTROLS ===");
            Debug.Log("WASD - Move");
            Debug.Log("Shift - Run");
            Debug.Log("Space - Enter Combat");
            Debug.Log("F - Force Enter Combat (manual transition)");
            Debug.Log("G - Force Back to Idle (manual transition)");
            Debug.Log("In Combat:");
            Debug.Log("  Left Mouse - Attack");
            Debug.Log("  Left Ctrl (Hold) - Defend");
            Debug.Log("  E - Cast Spell");
            Debug.Log("  Q - Back to Attack");
            Debug.Log("  Escape - Leave Combat");
            Debug.Log("R - Restart (when dead)");
        }
        
        private async void Update()
        {
            if (_playerStateMachine != null && _playerStateMachine.IsRunning)
            {
                // Manual transition example - Press F to force enter combat
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Debug.Log("Force entering combat!");
                    await _playerStateMachine.TransitionToAsync(PlayerState.Combat);
                }
                
                // Press G to force back to idle
                if (Input.GetKeyDown(KeyCode.G))
                {
                    Debug.Log("Force back to idle!");
                    await _playerStateMachine.TransitionToAsync(PlayerState.Idle);
                }
                
                // Check for automatic transitions
                await _playerStateMachine.CheckTransitionsAsync();
                
                // Tick the state machine
                _playerStateMachine.Tick();
            }
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"State: {_playerStateMachine?.CurrentState}");
            GUILayout.Label($"Health: {_health}/100");
            GUILayout.Label($"Position: {transform.position}");
            
            if (_playerStateMachine?.CurrentState == PlayerState.Combat)
            {
                // Try to access sub-state (would need to expose this in the API)
                GUILayout.Label("In Combat Mode");
            }
            GUILayout.EndArea();
        }
        
        private async void OnDestroy()
        {
            if (_playerStateMachine != null)
            {
                await _playerStateMachine.StopAsync();
            }
        }
    }
    
    /// <summary>
    /// Example 2: Simple traffic light state machine
    /// </summary>
    public class TrafficLightExample : MonoBehaviour
    {
        public enum LightState
        {
            Red,
            Yellow,
            Green
        }
        
        private SimpleStateMachine<LightState> _lightStateMachine;
        private float _stateTimer;
        
        private async void Start()
        {
            _lightStateMachine = new SimpleStateMachine<LightState>(enableLogs: true);
            
            // Red light: 5 seconds
            _lightStateMachine
                .State(LightState.Red)
                    .OnEnter(() => 
                    {
                        _stateTimer = 5f;
                        Debug.Log("🔴 RED LIGHT - STOP!");
                    })
                    .OnTick(() => _stateTimer -= Time.deltaTime)
                    .TransitionTo(LightState.Green, () => _stateTimer <= 0);
            
            // Green light: 8 seconds
            _lightStateMachine
                .State(LightState.Green)
                    .OnEnter(() => 
                    {
                        _stateTimer = 8f;
                        Debug.Log("🟢 GREEN LIGHT - GO!");
                    })
                    .OnTick(() => _stateTimer -= Time.deltaTime)
                    .TransitionTo(LightState.Yellow, () => _stateTimer <= 0);
            
            // Yellow light: 2 seconds
            _lightStateMachine
                .State(LightState.Yellow)
                    .OnEnter(() => 
                    {
                        _stateTimer = 2f;
                        Debug.Log("🟡 YELLOW LIGHT - CAUTION!");
                    })
                    .OnTick(() => _stateTimer -= Time.deltaTime)
                    .TransitionTo(LightState.Red, () => _stateTimer <= 0);
            
            await _lightStateMachine.StartAsync(LightState.Red);
        }
        
        private async void Update()
        {
            if (_lightStateMachine != null)
            {
                await _lightStateMachine.CheckTransitionsAsync();
                _lightStateMachine.Tick();
            }
        }
    }
}