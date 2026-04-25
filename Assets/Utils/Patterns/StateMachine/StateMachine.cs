using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// A flexible state machine implementation for game logic without MonoBehaviour dependency.
    /// Supports CancellationToken, global transitions, and zero-reflection design.
    /// </summary>
    /// <typeparam name="T">The context type for states</typeparam>
    public class StateMachine<T> where T : class
    {
        private readonly T _context;
        private readonly bool _enableDebugLogs;
        private Type _currentStateType;

        private readonly Dictionary<Type, IState> _states = new();
        private readonly Dictionary<Type, List<StateTransition>> _transitions = new();
        private readonly List<StateTransition> _globalTransitions = new();

        private IState _currentState;
        private bool _isTransitioning;
        private bool _isRunning;
        private CancellationTokenSource _cts;

        public event Action<IState, IState> OnStateChanged;
        public event Action<Type> OnTransitionFailed;

        /// <summary>
        /// Create a new StateMachine with context
        /// </summary>
        public StateMachine(T context, bool enableDebugLogs = true)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _enableDebugLogs = enableDebugLogs;
        }

        public IState CurrentState => _currentState;
        public Type CurrentStateType => _currentStateType;
        public bool IsTransitioning => _isTransitioning;
        public bool IsRunning => _isRunning;
        public T Context => _context;

        /// <summary>
        /// Returns true if the current active state is of type <typeparamref name="TState"/>
        /// </summary>
        public bool IsInState<TState>() where TState : class, IState
            => _currentStateType == typeof(TState);

        /// <summary>
        /// Add a state to the state machine
        /// </summary>
        public void AddState<TState>(TState state) where TState : class, IState
        {
            if (state == null) { LogError("Cannot add null state"); return; }

            var stateType = typeof(TState);
            if (_states.ContainsKey(stateType))
                LogWarning($"State '{stateType.Name}' already exists. Replacing...");

            _states[stateType] = state;
            if (!_transitions.ContainsKey(stateType))
                _transitions[stateType] = new List<StateTransition>();

            Log($"Added state: {stateType.Name}");
        }

        /// <summary>
        /// Remove a state from the state machine
        /// </summary>
        public void RemoveState<TState>() where TState : class, IState
        {
            var stateType = typeof(TState);
            if (!_states.ContainsKey(stateType)) { LogWarning($"State '{stateType.Name}' not found"); return; }
            if (_currentStateType == stateType) { LogError($"Cannot remove current state '{stateType.Name}'"); return; }

            _states.Remove(stateType);
            _transitions.Remove(stateType);

            foreach (var list in _transitions.Values)
                list.RemoveAll(t => t.ToStateType == stateType);

            Log($"Removed state: {stateType.Name}");
        }

        /// <summary>
        /// Add a conditional transition from TFrom to TTo.
        /// Transition fires when condition returns true (or condition is null).
        /// </summary>
        public void AddTransition<TFrom, TTo>(Func<bool> condition = null)
            where TFrom : class, IState
            where TTo : class, IState
        {
            var fromType = typeof(TFrom);
            var toType = typeof(TTo);

            if (!_states.ContainsKey(fromType)) { LogError($"From state '{fromType.Name}' not found"); return; }
            if (!_states.ContainsKey(toType)) { LogError($"To state '{toType.Name}' not found"); return; }

            // Store a delegate — avoids Reflection in CheckTransitionsAsync hot-path
            var transition = new StateTransition(toType, condition, ct => TransitionToAsync<TTo>(ct));
            _transitions[fromType].Add(transition);

            Log($"Added transition: {fromType.Name} -> {toType.Name}");
        }

        /// <summary>
        /// Add a global transition that can fire from ANY active state.
        /// Useful for "death", "game over", or "pause" states.
        /// Global transitions are checked before per-state transitions.
        /// </summary>
        public void AddGlobalTransition<TTo>(Func<bool> condition = null) where TTo : class, IState
        {
            var toType = typeof(TTo);
            if (!_states.ContainsKey(toType)) { LogError($"To state '{toType.Name}' not found"); return; }

            var transition = new StateTransition(toType, condition, ct => TransitionToAsync<TTo>(ct));
            _globalTransitions.Add(transition);

            Log($"Added global transition -> {toType.Name}");
        }

        /// <summary>
        /// Start the state machine with an initial state.
        /// Pass a CancellationToken (e.g. destroyCancellationToken) to auto-stop on destroy.
        /// </summary>
        public async UniTask StartAsync<TState>(CancellationToken externalCt = default) where TState : class, IState
        {
            var stateType = typeof(TState);
            if (!_states.ContainsKey(stateType)) { LogError($"Initial state '{stateType.Name}' not found"); return; }

            _cts = externalCt == default
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(externalCt);

            _isRunning = true;

            // Initialize context for all states that implement IState<T>
            foreach (var state in _states.Values)
            {
                if (state is IState<T> contextualState)
                {
                    try
                    {
                        contextualState.Initialize(_context);
                        Log($"Initialized: {state.GetType().Name}");
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error initializing {state.GetType().Name}: {ex.Message}");
                    }
                }
            }

            await TransitionToAsync<TState>(_cts.Token);
            Log($"Started with initial state: {stateType.Name}");
        }

        /// <summary>
        /// Manually trigger a transition to a specific state
        /// </summary>
        public async UniTask<bool> TransitionToAsync<TState>(CancellationToken ct = default) where TState : class, IState
        {
            var stateType = typeof(TState);

            if (_isTransitioning) { LogWarning("Already transitioning, ignoring request"); return false; }

            if (!_states.ContainsKey(stateType))
            {
                LogError($"State '{stateType.Name}' not found");
                OnTransitionFailed?.Invoke(stateType);
                return false;
            }

            var previousState = _currentState;
            var newState = _states[stateType];

            _isTransitioning = true;

            try
            {
                if (_currentState != null)
                {
                    await _currentState.OnExit(ct);
                    Log($"Exited state: {_currentStateType?.Name}");
                }

                _currentState = newState;
                _currentStateType = stateType;
                await _currentState.OnEnter(ct);

                Log($"Entered state: {_currentStateType.Name}");
                OnStateChanged?.Invoke(previousState, _currentState);
                return true;
            }
            catch (OperationCanceledException)
            {
                Log($"Transition to {stateType.Name} was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error transitioning to {stateType.Name}: {ex.Message}\n{ex.StackTrace}");
                OnTransitionFailed?.Invoke(stateType);
                return false;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        /// <summary>
        /// Check and execute automatic transitions.
        /// Call this from Update or a game loop tick.
        /// Global transitions are evaluated before per-state transitions.
        /// </summary>
        public async UniTask CheckTransitionsAsync(CancellationToken ct = default)
        {
            if (_currentState == null || _isTransitioning || _currentStateType == null) return;

            // Check global transitions first (e.g. game over from any state)
            foreach (var transition in _globalTransitions)
            {
                if (transition.Condition == null || transition.Condition())
                {
                    await transition.Execute(ct);
                    return;
                }
            }

            // Check per-state transitions
            if (!_transitions.TryGetValue(_currentStateType, out var transitions)) return;

            foreach (var transition in transitions)
            {
                if (transition.Condition == null || transition.Condition())
                {
                    await transition.Execute(ct);
                    break; // Only execute first valid transition
                }
            }
        }

        /// <summary>
        /// Stop the state machine and release resources
        /// </summary>
        public async UniTask StopAsync()
        {
            if (!_isRunning) return;

            var stopCt = _cts?.Token ?? default;

            if (_currentState != null)
            {
                await _currentState.OnExit(stopCt);
                Log($"Exited final state: {_currentStateType?.Name}");
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _currentState = null;
            _currentStateType = null;
            _isRunning = false;

            Log("State machine stopped");
        }

        /// <summary>
        /// Manual tick for states that implement ITickableState.
        /// Call this from MonoBehaviour.Update().
        /// </summary>
        public void Tick()
        {
            if (_currentState != null && !_isTransitioning && _currentState is ITickableState tickable)
            {
                try
                {
                    tickable.OnTick();
                }
                catch (Exception ex)
                {
                    LogError($"Error in OnTick for {_currentStateType?.Name}: {ex.Message}");
                }
            }
        }

        private void Log(string message)
        {
            if (_enableDebugLogs) ConsoleLogger.Log($"[StateMachine] {message}");
        }

        private void LogWarning(string message)
        {
            if (_enableDebugLogs) ConsoleLogger.LogWarning($"[StateMachine] {message}");
        }

        private void LogError(string message)
        {
            ConsoleLogger.LogError($"[StateMachine] {message}");
        }
    }

    /// <summary>
    /// Interface for states that need periodic updates via Tick()
    /// </summary>
    public interface ITickableState
    {
        void OnTick();
    }

    /// <summary>
    /// Represents a transition between states.
    /// Stores a pre-built delegate to avoid Reflection in the update loop.
    /// </summary>
    internal class StateTransition
    {
        public Type ToStateType { get; }
        public Func<bool> Condition { get; }

        /// <summary>
        /// Pre-compiled delegate — eliminates Reflection overhead in CheckTransitionsAsync
        /// </summary>
        public Func<CancellationToken, UniTask<bool>> Execute { get; }

        public StateTransition(Type toStateType, Func<bool> condition, Func<CancellationToken, UniTask<bool>> execute)
        {
            ToStateType = toStateType;
            Condition = condition;
            Execute = execute;
        }
    }
}
