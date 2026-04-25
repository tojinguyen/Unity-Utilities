using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// Interface for state machines to allow polymorphic sub-states
    /// </summary>
    internal interface ISimpleStateMachine
    {
        UniTask StartAsync(Enum initialState, CancellationToken ct = default);
        UniTask StopAsync();
        UniTask CheckTransitionsAsync(CancellationToken ct = default);
        void Tick();
    }

    /// <summary>
    /// Simple state machine using enum as states with a fluent builder pattern.
    /// Supports CancellationToken, nested sub-state machines, and IsInState checks.
    /// </summary>
    public class SimpleStateMachine<TState> : ISimpleStateMachine where TState : Enum
    {
        private readonly Dictionary<TState, StateNode> _states = new();
        private readonly Dictionary<TState, List<Transition>> _transitions = new();
        private readonly bool _enableLogs;

        private TState _currentState;
        private StateNode _currentNode;
        private bool _isTransitioning;
        private bool _isRunning;
        private CancellationTokenSource _cts;

        public TState CurrentState => _currentState;
        public bool IsTransitioning => _isTransitioning;
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Returns true if the machine is running and currently in the given state
        /// </summary>
        public bool IsInState(TState state)
            => _isRunning && EqualityComparer<TState>.Default.Equals(_currentState, state);

        public event Action<TState, TState> OnStateChanged;

        public SimpleStateMachine(bool enableLogs = false)
        {
            _enableLogs = enableLogs;
        }

        /// <summary>
        /// Configure a state using the fluent builder pattern
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
        /// Start the state machine with an initial state.
        /// Pass a CancellationToken (e.g. destroyCancellationToken) to auto-stop on destroy.
        /// </summary>
        public async UniTask StartAsync(TState initialState, CancellationToken externalCt = default)
        {
            if (_isRunning) { LogWarning("State machine is already running"); return; }
            if (!_states.ContainsKey(initialState)) { LogError($"Initial state '{initialState}' not configured"); return; }

            _cts = externalCt == default
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(externalCt);

            _isRunning = true;

            try
            {
                await TransitionToAsync(initialState, _cts.Token);
                Log($"Started with state: {initialState}");
            }
            catch (Exception ex)
            {
                LogError($"Error in StartAsync: {ex.Message}");
                _isRunning = false;
                _cts?.Dispose();
                _cts = null;
                throw;
            }
        }

        /// <summary>
        /// Explicit interface implementation for ISimpleStateMachine
        /// </summary>
        async UniTask ISimpleStateMachine.StartAsync(Enum initialState, CancellationToken ct)
        {
            await StartAsync((TState)initialState, ct);
        }

        /// <summary>
        /// Stop the state machine and release resources
        /// </summary>
        public async UniTask StopAsync()
        {
            if (!_isRunning) return;

            if (_currentNode != null)
                await ExitStateAsync(_currentNode, _currentState, _cts?.Token ?? default);

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _isRunning = false;

            Log("State machine stopped");
        }

        /// <summary>
        /// Manually transition to a specific state
        /// </summary>
        public async UniTask<bool> TransitionToAsync(TState toState, CancellationToken ct = default)
        {
            if (_isTransitioning) { LogWarning("Already transitioning"); return false; }
            if (!_states.ContainsKey(toState)) { LogError($"State '{toState}' not configured"); return false; }

            _isTransitioning = true;

            try
            {
                var previousState = _currentState;
                var previousNode = _currentNode;

                if (previousNode != null)
                    await ExitStateAsync(previousNode, previousState, ct);

                _currentState = toState;
                _currentNode = _states[toState];
                await EnterStateAsync(_currentNode, toState, ct);

                OnStateChanged?.Invoke(previousState, toState);
                Log($"Transitioned: {previousState} -> {toState}");
                return true;
            }
            catch (OperationCanceledException)
            {
                Log($"Transition to {toState} was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error transitioning to {toState}: {ex.Message}");
                return false;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        /// <summary>
        /// Check and execute automatic transitions.
        /// Sub-state machine transitions are evaluated first.
        /// </summary>
        public async UniTask CheckTransitionsAsync(CancellationToken ct = default)
        {
            if (!_isRunning || _isTransitioning || _currentNode == null) return;

            // Check sub-state transitions first
            if (_currentNode.SubStateMachine != null)
            {
                try { await _currentNode.SubStateMachine.CheckTransitionsAsync(ct); }
                catch (Exception ex) { LogError($"Error checking sub-state transitions: {ex.Message}"); }
            }

            if (!_transitions.ContainsKey(_currentState)) return;

            foreach (var transition in _transitions[_currentState])
            {
                if (transition.Condition == null || transition.Condition())
                {
                    await TransitionToAsync(transition.ToState, ct);
                    break; // Only execute first valid transition
                }
            }
        }

        /// <summary>
        /// Manual tick for states that need updates. Call from MonoBehaviour.Update().
        /// </summary>
        public void Tick()
        {
            if (!_isRunning || _isTransitioning || _currentNode == null) return;

            try { _currentNode.OnTick?.Invoke(); }
            catch (Exception ex) { LogError($"Error in OnTick for {_currentState}: {ex.Message}"); }

            try { _currentNode.SubStateMachine?.Tick(); }
            catch (Exception ex) { LogError($"Error ticking sub-state machine: {ex.Message}"); }
        }

        private async UniTask EnterStateAsync(StateNode node, TState state, CancellationToken ct)
        {
            if (node.OnEnter != null)
                await node.OnEnter(ct);

            if (node.SubStateMachine != null && node.InitialSubState != null)
                await node.SubStateMachine.StartAsync((Enum)node.InitialSubState, ct);

            Log($"Entered: {state}");
        }

        private async UniTask ExitStateAsync(StateNode node, TState state, CancellationToken ct)
        {
            if (node.SubStateMachine != null)
            {
                try { await node.SubStateMachine.StopAsync(); }
                catch (Exception ex) { LogError($"Error stopping sub-state machine for {state}: {ex.Message}"); }
            }

            if (node.OnExit != null)
                await node.OnExit(ct);

            Log($"Exited: {state}");
        }

        private void Log(string message)
        {
            if (_enableLogs) ConsoleLogger.Log($"[SimpleStateMachine] {message}");
        }

        private void LogWarning(string message)
        {
            if (_enableLogs) ConsoleLogger.LogWarning($"[SimpleStateMachine] {message}");
        }

        private void LogError(string message)
        {
            ConsoleLogger.LogError($"[SimpleStateMachine] {message}");
        }

        #region Internal Classes

        private class StateNode
        {
            public Func<CancellationToken, UniTask> OnEnter;
            public Func<CancellationToken, UniTask> OnExit;
            public Action OnTick;
            public ISimpleStateMachine SubStateMachine;
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

            /// <summary>Set async OnEnter callback</summary>
            public StateBuilder OnEnter(Func<CancellationToken, UniTask> callback)
            {
                _node.OnEnter = callback;
                return this;
            }

            /// <summary>Set async OnEnter callback (non-cancellable)</summary>
            public StateBuilder OnEnter(Func<UniTask> callback)
            {
                _node.OnEnter = _ => callback();
                return this;
            }

            /// <summary>Set synchronous OnEnter callback</summary>
            public StateBuilder OnEnter(Action callback)
            {
                _node.OnEnter = _ => { callback?.Invoke(); return UniTask.CompletedTask; };
                return this;
            }

            /// <summary>Set async OnExit callback</summary>
            public StateBuilder OnExit(Func<CancellationToken, UniTask> callback)
            {
                _node.OnExit = callback;
                return this;
            }

            /// <summary>Set async OnExit callback (non-cancellable)</summary>
            public StateBuilder OnExit(Func<UniTask> callback)
            {
                _node.OnExit = _ => callback();
                return this;
            }

            /// <summary>Set synchronous OnExit callback</summary>
            public StateBuilder OnExit(Action callback)
            {
                _node.OnExit = _ => { callback?.Invoke(); return UniTask.CompletedTask; };
                return this;
            }

            /// <summary>Set OnTick callback</summary>
            public StateBuilder OnTick(Action callback)
            {
                _node.OnTick = callback;
                return this;
            }

            /// <summary>Add a transition to another state</summary>
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
            /// Add sub-states to this state (composite state pattern).
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

            /// <summary>Configure a sub-state</summary>
            public SubStateBuilder SubState(TSubState subState)
            {
                return new SubStateBuilder(this, _subMachine.State(subState));
            }

            /// <summary>
            /// Finish configuring sub-states and return to the parent state builder.
            /// Alias: Done()
            /// </summary>
            public StateBuilder EndSubStates() => _parentBuilder;

            /// <summary>
            /// Finish configuring sub-states and return to the parent state builder.
            /// </summary>
            public StateBuilder Done() => _parentBuilder;

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

                public SubStateBuilder OnEnter(Func<CancellationToken, UniTask> callback)
                {
                    _stateBuilder.OnEnter(callback);
                    return this;
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

                public SubStateBuilder OnExit(Func<CancellationToken, UniTask> callback)
                {
                    _stateBuilder.OnExit(callback);
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
                /// Return to the sub-machine builder to configure more sub-states
                /// </summary>
                public SubStateMachineBuilder<TSubState> And() => _subMachineBuilder;
            }
        }

        #endregion
    }
}