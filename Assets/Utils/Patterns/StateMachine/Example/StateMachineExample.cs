using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TirexGame.Utils.Patterns.StateMachine;

namespace TirexGame.Utils.Patterns.StateMachine.Example
{
    /// <summary>
    /// Simple context for the movement state machine
    /// </summary>
    public class SimpleContext
    {
        public bool IsMoving { get; set; }
        public bool IsRunning { get; set; }
    }
    
    /// <summary>
    /// Example demonstrating how to use the new type-safe StateMachine
    /// </summary>
    public class StateMachineExample : MonoBehaviour
    {
        private StateMachine<SimpleContext> _stateMachine;
        private SimpleContext _context;
        
        private async void Start()
        {
            // Create context
            _context = new SimpleContext();
            
            // Create a new StateMachine instance with context
            _stateMachine = new StateMachine<SimpleContext>(_context, enableDebugLogs: true);
            
            // Create and add states
            var idleState = new IdleState();
            var walkingState = new WalkingState();
            var runningState = new RunningState();
            
            _stateMachine.AddState(idleState);
            _stateMachine.AddState(walkingState);
            _stateMachine.AddState(runningState);
            
            // Add transitions between states
            _stateMachine.AddTransition<IdleState, WalkingState>(() => Input.GetKey(KeyCode.W));
            _stateMachine.AddTransition<WalkingState, IdleState>(() => !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.LeftShift));
            _stateMachine.AddTransition<WalkingState, RunningState>(() => Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift));
            _stateMachine.AddTransition<RunningState, WalkingState>(() => Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.LeftShift));
            _stateMachine.AddTransition<RunningState, IdleState>(() => !Input.GetKey(KeyCode.W));
            
            // Start with Idle state
            // Pass destroyCancellationToken so the machine auto-stops when this object is destroyed
            await _stateMachine.StartAsync<IdleState>(destroyCancellationToken);
        }
        
        private async void Update()
        {
            // Check for automatic transitions
            await _stateMachine.CheckTransitionsAsync();
            
            // Manual tick for states that need it
            _stateMachine.Tick();
        }
        
        private async void OnDestroy()
        {
            if (_stateMachine != null)
            {
                await _stateMachine.StopAsync();
            }
        }
    }
    
    // Example state implementations
    public class IdleState : BaseTickableState<SimpleContext>
    {
        public override UniTask OnEnter(CancellationToken ct = default)
        {
            Context.IsMoving = false;
            Debug.Log("Entered Idle State");
            return base.OnEnter(ct);
        }
        
        public override void OnTick()
        {
            // Called every frame via StateMachine.Tick()
        }
        
        public override UniTask OnExit(CancellationToken ct = default)
        {
            Debug.Log("Exited Idle State");
            return base.OnExit(ct);
        }
    }
    
    public class WalkingState : BaseState<SimpleContext>
    {
        public override UniTask OnEnter(CancellationToken ct = default)
        {
            Context.IsMoving = true;
            Context.IsRunning = false;
            Debug.Log("Entered Walking State");
            return base.OnEnter(ct);
        }
        
        public override UniTask OnExit(CancellationToken ct = default)
        {
            Debug.Log("Exited Walking State");
            return base.OnExit(ct);
        }
    }
    
    public class RunningState : BaseState<SimpleContext>
    {
        public override UniTask OnEnter(CancellationToken ct = default)
        {
            Context.IsMoving = true;
            Context.IsRunning = true;
            Debug.Log("Entered Running State");
            return base.OnEnter(ct);
        }
        
        public override UniTask OnExit(CancellationToken ct = default)
        {
            Debug.Log("Exited Running State");
            return base.OnExit(ct);
        }
    }
}