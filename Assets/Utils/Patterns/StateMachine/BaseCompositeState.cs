using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// Base class for composite states that can contain sub-states.
    /// Set EnableDebugLogs = true in your derived class constructor to enable logging.
    /// </summary>
    public abstract class BaseCompositeState : BaseState, ICompositeState
    {
        protected readonly Dictionary<Type, IState> _subStates = new();
        private readonly Dictionary<Type, List<SubTransition>> _subTransitions = new();
        protected Type _initialSubStateType;
        protected IState _currentSubState;
        protected bool _isSubTransitioning;

        /// <summary>
        /// Enable or disable debug logging for this composite state.
        /// </summary>
        protected bool EnableDebugLogs { get; set; } = false;

        public IState CurrentSubState => _currentSubState;

        public void AddSubState(IState subState, bool isInitial = false)
        {
            if (subState == null) { LogError("Cannot add null sub-state"); return; }

            var stateType = subState.GetType();

            if (_subStates.ContainsKey(stateType))
                LogWarning($"Sub-state '{stateType.Name}' already exists. Replacing...");

            _subStates[stateType] = subState;
            _subTransitions[stateType] = new List<SubTransition>();

            if (isInitial || _initialSubStateType == null)
                _initialSubStateType = stateType;

            Log($"Added sub-state: {stateType.Name}{(isInitial ? " (initial)" : "")}");
        }

        public void AddSubTransition<TFrom, TTo>(Func<bool> condition = null)
            where TFrom : class, IState
            where TTo : class, IState
        {
            var fromType = typeof(TFrom);
            var toType = typeof(TTo);

            if (!_subStates.ContainsKey(fromType)) { LogError($"From sub-state '{fromType.Name}' not found"); return; }
            if (!_subStates.ContainsKey(toType)) { LogError($"To sub-state '{toType.Name}' not found"); return; }

            _subTransitions[fromType].Add(new SubTransition(toType, condition));
            Log($"Added sub-transition: {fromType.Name} -> {toType.Name}");
        }

        public override async UniTask OnEnter(CancellationToken ct = default)
        {
            Log("Entering composite state");
            await base.OnEnter(ct);

            if (_initialSubStateType != null)
                await TransitionToSubStateInternalAsync(_initialSubStateType, ct);
        }

        public override async UniTask OnExit(CancellationToken ct = default)
        {
            if (_currentSubState != null)
            {
                await _currentSubState.OnExit(ct);
                Log($"Exited sub-state: {_currentSubState.GetType().Name}");
                _currentSubState = null;
            }

            Log("Exiting composite state");
            await base.OnExit(ct);
        }

        public async UniTask CheckSubTransitionsAsync(CancellationToken ct = default)
        {
            if (_currentSubState == null || _isSubTransitioning) return;

            var currentType = _currentSubState.GetType();
            if (!_subTransitions.TryGetValue(currentType, out var transitions)) return;

            foreach (var transition in transitions)
            {
                if (transition.Condition == null || transition.Condition())
                {
                    await TransitionToSubStateInternalAsync(transition.ToStateType, ct);

                    // Propagate to new sub-state if composite
                    if (_currentSubState is ICompositeState composite)
                        await composite.CheckSubTransitionsAsync(ct);

                    break;
                }
            }

            // Also propagate to existing composite sub-state
            if (_currentSubState is ICompositeState nested)
                await nested.CheckSubTransitionsAsync(ct);
        }

        public async UniTask TransitionToSubStateAsync<T>(CancellationToken ct = default) where T : class, IState
        {
            await TransitionToSubStateInternalAsync(typeof(T), ct);
        }

        protected async UniTask TransitionToSubStateInternalAsync(Type stateType, CancellationToken ct = default)
        {
            if (_isSubTransitioning) { LogWarning("Already transitioning sub-states"); return; }
            if (!_subStates.ContainsKey(stateType)) { LogError($"Sub-state '{stateType.Name}' not found"); return; }

            _isSubTransitioning = true;

            try
            {
                if (_currentSubState != null)
                {
                    await _currentSubState.OnExit(ct);
                    Log($"Exited sub-state: {_currentSubState.GetType().Name}");
                }

                _currentSubState = _subStates[stateType];
                await _currentSubState.OnEnter(ct);
                Log($"Entered sub-state: {stateType.Name}");
            }
            catch (OperationCanceledException)
            {
                Log($"Sub-state transition to {stateType.Name} was cancelled");
            }
            catch (Exception ex)
            {
                LogError($"Error during sub-state transition to {stateType.Name}: {ex.Message}");
            }
            finally
            {
                _isSubTransitioning = false;
            }
        }

        private void Log(string message)
        {
            if (EnableDebugLogs) ConsoleLogger.Log($"[{GetType().Name}] {message}");
        }

        private void LogWarning(string message)
        {
            if (EnableDebugLogs) ConsoleLogger.LogWarning($"[{GetType().Name}] {message}");
        }

        private void LogError(string message)
        {
            ConsoleLogger.LogError($"[{GetType().Name}] {message}");
        }

        /// <summary>
        /// Lightweight transition record for sub-state transitions (no delegate needed — execution is internal)
        /// </summary>
        private readonly struct SubTransition
        {
            public readonly Type ToStateType;
            public readonly Func<bool> Condition;

            public SubTransition(Type toStateType, Func<bool> condition)
            {
                ToStateType = toStateType;
                Condition = condition;
            }
        }
    }

    /// <summary>
    /// Base class for tickable composite states.
    /// Automatically ticks the current sub-state if it implements ITickableState.
    /// </summary>
    public abstract class BaseTickableCompositeState : BaseCompositeState, ITickableState
    {
        public virtual void OnTick()
        {
            if (_currentSubState is ITickableState tickable)
                tickable.OnTick();
        }
    }

    /// <summary>
    /// Base class for composite states with context.
    /// Automatically propagates context to sub-states that implement IState&lt;T&gt;.
    /// </summary>
    public abstract class BaseCompositeState<T> : BaseCompositeState, IState<T>
    {
        protected T Context { get; private set; }

        public virtual void Initialize(T context)
        {
            Context = context;

            foreach (var subState in _subStates.Values)
            {
                if (subState is IState<T> contextSubState)
                    contextSubState.Initialize(context);
            }
        }
    }

    /// <summary>
    /// Base class for tickable composite states with context
    /// </summary>
    public abstract class BaseTickableCompositeState<T> : BaseCompositeState<T>, ITickableState
    {
        public virtual void OnTick()
        {
            if (_currentSubState is ITickableState tickable)
                tickable.OnTick();
        }
    }
}