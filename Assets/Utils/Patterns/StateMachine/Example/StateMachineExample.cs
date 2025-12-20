using System;
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
            await _stateMachine.StartAsync<IdleState>();
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
        public override async UniTask OnEnter()
        {
            Context.IsMoving = false;
            Debug.Log("Entered Idle State");
            await base.OnEnter();
        }
        
        public override void OnTick()
        {
            // This will be called when StateMachine.Tick() is called
            // Use this for states that need periodic updates
        }
        
        public override async UniTask OnExit()
        {
            Debug.Log("Exited Idle State");
            await base.OnExit();
        }
    }
    
    public class WalkingState : BaseState<SimpleContext>
    {
        public override async UniTask OnEnter()
        {
            Context.IsMoving = true;
            Context.IsRunning = false;
            Debug.Log("Entered Walking State");
            await base.OnEnter();
        }
        
        public override async UniTask OnExit()
        {
            Debug.Log("Exited Walking State");
            await base.OnExit();
        }
    }
    
    public class RunningState : BaseState<SimpleContext>
    {
        public override async UniTask OnEnter()
        {
            Context.IsMoving = true;
            Context.IsRunning = true;
            Debug.Log("Entered Running State");
            await base.OnEnter();
        }
        
        public override async UniTask OnExit()
        {
            Debug.Log("Exited Running State");
            await base.OnExit();
        }
    }
}