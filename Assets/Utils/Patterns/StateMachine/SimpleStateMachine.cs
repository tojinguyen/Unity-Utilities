using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// Simple state machine using enum as states with builder pattern
    /// </summary>
    public class SimpleStateMachine<TState> where TState : Enum
    {
        private readonly Dictionary<TState, StateNode> _states = new();
        private readonly Dictionary<TState, List<Transition>> _transitions = new();
        private readonly bool _enableLogs;
        
        private TState _currentState;
        private StateNode _currentNode;
        private bool _isTransitioning;
        private bool _isRunning;
        
        public TState CurrentState => _currentState;
        public bool IsTransitioning => _isTransitioning;
        public bool IsRunning => _isRunning;
        
        public event Action<TState, TState> OnStateChanged;
        
        public SimpleStateMachine(bool enableLogs = false)
        {
            _enableLogs = enableLogs;
        }
        
        /// <summary>
        /// Configure a state using builder pattern
        /// </summary>
        public StateBuilder State(TState state)
        {
            if (!_states.ContainsKey(state))
            {
                _states[state] = new StateNode();
                _transitions[state] = new List<Transition>();
            }
            
            return new StateBuilder(this, state);
        }
        
        /// <summary>
        /// Start the state machine with initial state
        /// </summary>
        public async UniTask StartAsync(TState initialState)
        {
            if (_isRunning)
            {
                LogWarning("State machine is already running");
                return;
            }
            
            if (!_states.ContainsKey(initialState))
            {
                LogError($"Initial state '{initialState}' not configured");
                return;
            }
            
            _isRunning = true;
            await TransitionToAsync(initialState);
            Log($"Started with state: {initialState}");
        }
        
        /// <summary>
        /// Stop the state machine
        /// </summary>
        public async UniTask StopAsync()
        {
            if (!_isRunning) return;
            
            if (_currentNode != null)
            {
                await ExitStateAsync(_currentNode, _currentState);
            }
            
            _isRunning = false;
            Log("State machine stopped");
        }
        
        /// <summary>
        /// Manually transition to a state
        /// </summary>
        public async UniTask<bool> TransitionToAsync(TState toState)
        {
            if (_isTransitioning)
            {
                LogWarning("Already transitioning");
                return false;
            }
            
            if (!_states.ContainsKey(toState))
            {
                LogError($"State '{toState}' not configured");
                return false;
            }
            
            _isTransitioning = true;
            
            try
            {
                var previousState = _currentState;
                var previousNode = _currentNode;
                
                // Exit current state
                if (previousNode != null)
                {
                    await ExitStateAsync(previousNode, previousState);
                }
                
                // Enter new state
                _currentState = toState;
                _currentNode = _states[toState];
                await EnterStateAsync(_currentNode, toState);
                
                OnStateChanged?.Invoke(previousState, toState);
                Log($"Transitioned: {previousState} -> {toState}");
                
                return true;
            }
            catch (Exception e)
            {
                LogError($"Transition error: {e.Message}");
                return false;
            }
            finally
            {
                _isTransitioning = false;
            }
        }
        
        /// <summary>
        /// Check and execute automatic transitions
        /// </summary>
        public async UniTask CheckTransitionsAsync()
        {
            if (!_isRunning || _isTransitioning || _currentNode == null)
                return;
            
            // Check sub-state transitions first
            if (_currentNode.SubStateMachine != null)
            {
                await _currentNode.SubStateMachine.CheckTransitionsAsync();
            }
            
            // Check main transitions
            if (!_transitions.ContainsKey(_currentState))
                return;
            
            foreach (var transition in _transitions[_currentState])
            {
                if (transition.Condition == null || transition.Condition())
                {
                    await TransitionToAsync(transition.ToState);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Manual tick for states that need updates
        /// </summary>
        public void Tick()
        {
            if (!_isRunning || _isTransitioning || _currentNode == null)
                return;
            
            // Tick current state
            _currentNode.OnTick?.Invoke();
            
            // Tick sub-state machine
            _currentNode.SubStateMachine?.Tick();
        }
        
        private async UniTask EnterStateAsync(StateNode node, TState state)
        {
            if (node.OnEnter != null)
            {
                await node.OnEnter();
            }
            
            // Start sub-state machine if exists
            if (node.SubStateMachine != null && node.InitialSubState != null)
            {
                await node.SubStateMachine.StartAsync(node.InitialSubState.Value);
            }
            
            Log($"Entered: {state}");
        }
        
        private async UniTask ExitStateAsync(StateNode node, TState state)
        {
            // Stop sub-state machine first
            if (node.SubStateMachine != null)
            {
                await node.SubStateMachine.StopAsync();
            }
            
            if (node.OnExit != null)
            {
                await node.OnExit();
            }
            
            Log($"Exited: {state}");
        }
        
        private void Log(string message)
        {
            if (_enableLogs)
                Debug.Log($"[SimpleStateMachine] {message}");
        }
        
        private void LogWarning(string message)
        {
            if (_enableLogs)
                Debug.LogWarning($"[SimpleStateMachine] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[SimpleStateMachine] {message}");
        }
        
        #region Internal Classes
        
        private class StateNode
        {
            public Func<UniTask> OnEnter;
            public Func<UniTask> OnExit;
            public Action OnTick;
            public object SubStateMachine;
            public object InitialSubState;
        }
        
        private class Transition
        {
            public TState ToState;
            public Func<bool> Condition;
        }
        
        #endregion
        
        #region Builder
        
        public class StateBuilder
        {
            private readonly SimpleStateMachine<TState> _machine;
            private readonly TState _state;
            private readonly StateNode _node;
            
            internal StateBuilder(SimpleStateMachine<TState> machine, TState state)
            {
                _machine = machine;
                _state = state;
                _node = machine._states[state];
            }
            
            /// <summary>
            /// Set OnEnter callback
            /// </summary>
            public StateBuilder OnEnter(Func<UniTask> callback)
            {
                _node.OnEnter = callback;
                return this;
            }
            
            /// <summary>
            /// Set OnEnter callback (non-async)
            /// </summary>
            public StateBuilder OnEnter(Action callback)
            {
                _node.OnEnter = () =>
                {
                    callback?.Invoke();
                    return UniTask.CompletedTask;
                };
                return this;
            }
            
            /// <summary>
            /// Set OnExit callback
            /// </summary>
            public StateBuilder OnExit(Func<UniTask> callback)
            {
                _node.OnExit = callback;
                return this;
            }
            
            /// <summary>
            /// Set OnExit callback (non-async)
            /// </summary>
            public StateBuilder OnExit(Action callback)
            {
                _node.OnExit = () =>
                {
                    callback?.Invoke();
                    return UniTask.CompletedTask;
                };
                return this;
            }
            
            /// <summary>
            /// Set OnTick callback
            /// </summary>
            public StateBuilder OnTick(Action callback)
            {
                _node.OnTick = callback;
                return this;
            }
            
            /// <summary>
            /// Add transition to another state
            /// </summary>
            public StateBuilder TransitionTo(TState toState, Func<bool> condition = null)
            {
                _machine._transitions[_state].Add(new Transition
                {
                    ToState = toState,
                    Condition = condition
                });
                
                _machine.Log($"Added transition: {_state} -> {toState}");
                return this;
            }
            
            /// <summary>
            /// Add sub-states to this state (composite state)
            /// </summary>
            public SubStateMachineBuilder<TSubState> WithSubStates<TSubState>(TSubState initialSubState) 
                where TSubState : Enum
            {
                var subMachine = new SimpleStateMachine<TSubState>(_machine._enableLogs);
                _node.SubStateMachine = subMachine;
                _node.InitialSubState = initialSubState;
                
                return new SubStateMachineBuilder<TSubState>(this, subMachine);
            }
        }
        
        public class SubStateMachineBuilder<TSubState> where TSubState : Enum
        {
            private readonly StateBuilder _parentBuilder;
            private readonly SimpleStateMachine<TSubState> _subMachine;
            
            internal SubStateMachineBuilder(StateBuilder parentBuilder, SimpleStateMachine<TSubState> subMachine)
            {
                _parentBuilder = parentBuilder;
                _subMachine = subMachine;
            }
            
            /// <summary>
            /// Configure a sub-state
            /// </summary>
            public SubStateBuilder SubState(TSubState subState)
            {
                return new SubStateBuilder(this, _subMachine.State(subState));
            }
            
            /// <summary>
            /// Finish configuring sub-states and return to parent state builder
            /// </summary>
            public StateBuilder EndSubStates()
            {
                return _parentBuilder;
            }
            
            public class SubStateBuilder
            {
                private readonly SubStateMachineBuilder<TSubState> _subMachineBuilder;
                private readonly SimpleStateMachine<TSubState>.StateBuilder _stateBuilder;
                
                internal SubStateBuilder(SubStateMachineBuilder<TSubState> subMachineBuilder, 
                    SimpleStateMachine<TSubState>.StateBuilder stateBuilder)
                {
                    _subMachineBuilder = subMachineBuilder;
                    _stateBuilder = stateBuilder;
                }
                
                public SubStateBuilder OnEnter(Func<UniTask> callback)
                {
                    _stateBuilder.OnEnter(callback);
                    return this;
                }
                
                public SubStateBuilder OnEnter(Action callback)
                {
                    _stateBuilder.OnEnter(callback);
                    return this;
                }
                
                public SubStateBuilder OnExit(Func<UniTask> callback)
                {
                    _stateBuilder.OnExit(callback);
                    return this;
                }
                
                public SubStateBuilder OnExit(Action callback)
                {
                    _stateBuilder.OnExit(callback);
                    return this;
                }
                
                public SubStateBuilder OnTick(Action callback)
                {
                    _stateBuilder.OnTick(callback);
                    return this;
                }
                
                public SubStateBuilder TransitionTo(TSubState toState, Func<bool> condition = null)
                {
                    _stateBuilder.TransitionTo(toState, condition);
                    return this;
                }
                
                /// <summary>
                /// Return to sub-machine builder to configure more sub-states
                /// </summary>
                public SubStateMachineBuilder<TSubState> And()
                {
                    return _subMachineBuilder;
                }
            }
        }
        
        #endregion
    }
}