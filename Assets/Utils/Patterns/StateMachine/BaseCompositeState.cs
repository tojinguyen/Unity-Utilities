using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// Base class for composite states that can contain sub-states
    /// </summary>
    public abstract class BaseCompositeState : BaseState, ICompositeState
    {
        protected readonly Dictionary<Type, IState> _subStates = new();
        private readonly Dictionary<Type, List<StateTransition>> _subTransitions = new();
        protected Type _initialSubStateType;
        protected IState _currentSubState;
        protected bool _isSubTransitioning;

        public IState CurrentSubState => _currentSubState;

        public void AddSubState(IState subState, bool isInitial = false)
        {
            try
            {
                if (subState == null)
                {
                    ConsoleLogger.LogError($"[{GetType().Name}] Cannot add null sub-state");
                    return;
                }

                var stateType = subState.GetType();

                if (_subStates.ContainsKey(stateType))
                {
                    ConsoleLogger.LogWarning($"[{GetType().Name}] Sub-state '{stateType.Name}' already exists. Replacing...");
                }

                _subStates[stateType] = subState;
                _subTransitions[stateType] = new List<StateTransition>();

                if (isInitial || _initialSubStateType == null)
                {
                    _initialSubStateType = stateType;
                }

                ConsoleLogger.Log($"[{GetType().Name}] Added sub-state: {stateType.Name}" +
                         (isInitial ? " (initial)" : ""));
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[{GetType().Name}] Error in AddSubState: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public void AddSubTransition<TFrom, TTo>(Func<bool> condition = null)
            where TFrom : class, IState
            where TTo : class, IState
        {
            var fromType = typeof(TFrom);
            var toType = typeof(TTo);

            if (!_subStates.ContainsKey(fromType))
            {
                ConsoleLogger.LogError($"[{GetType().Name}] From sub-state '{fromType.Name}' not found");
                return;
            }

            if (!_subStates.ContainsKey(toType))
            {
                ConsoleLogger.LogError($"[{GetType().Name}] To sub-state '{toType.Name}' not found");
                return;
            }

            var transition = new StateTransition(toType, condition);
            _subTransitions[fromType].Add(transition);

            ConsoleLogger.Log($"[{GetType().Name}] Added sub-transition: {fromType.Name} -> {toType.Name}");
        }

        public override async UniTask OnEnter()
        {
            try
            {
                ConsoleLogger.Log($"[{GetType().Name}] Entering composite state");
                await base.OnEnter();

                // Enter initial sub-state
                if (_initialSubStateType != null)
                {
                    try
                    {
                        await TransitionToSubStateInternalAsync(_initialSubStateType);
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogger.LogError($"[{GetType().Name}] Error entering initial sub-state: {ex.Message}\nStackTrace: {ex.StackTrace}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[{GetType().Name}] Error in OnEnter: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public override async UniTask OnExit()
        {
            try
            {
                // Exit current sub-state first
                if (_currentSubState != null)
                {
                    try
                    {
                        await _currentSubState.OnExit();
                        ConsoleLogger.Log($"[{GetType().Name}] Exited sub-state: {_currentSubState.GetType().Name}");
                        _currentSubState = null;
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogger.LogError($"[{GetType().Name}] Error exiting sub-state: {ex.Message}\nStackTrace: {ex.StackTrace}");
                        throw;
                    }
                }

                ConsoleLogger.Log($"[{GetType().Name}] Exiting composite state");
                await base.OnExit();
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[{GetType().Name}] Error in OnExit: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async UniTask CheckSubTransitionsAsync()
        {
            if (_currentSubState == null || _isSubTransitioning)
                return;

            var currentType = _currentSubState.GetType();
            if (!_subTransitions.ContainsKey(currentType))
                return;

            foreach (var transition in _subTransitions[currentType])
            {
                if (transition.Condition == null || transition.Condition.Invoke())
                {
                    await TransitionToSubStateInternalAsync(transition.ToStateType);

                    // Also check if sub-state is composite and needs to check its own transitions
                    if (_currentSubState is ICompositeState compositeSubState)
                    {
                        await compositeSubState.CheckSubTransitionsAsync();
                    }

                    break; // Only execute first valid transition
                }
            }

            // Check nested composite state transitions
            if (_currentSubState is ICompositeState nestedComposite)
            {
                await nestedComposite.CheckSubTransitionsAsync();
            }
        }

        public async UniTask TransitionToSubStateAsync<T>() where T : class, IState
        {
            await TransitionToSubStateInternalAsync(typeof(T));
        }

        protected async UniTask TransitionToSubStateInternalAsync(Type stateType)
        {
            if (_isSubTransitioning)
            {
                ConsoleLogger.LogWarning($"[{GetType().Name}] Already transitioning sub-states");
                return;
            }

            if (!_subStates.ContainsKey(stateType))
            {
                ConsoleLogger.LogError($"[{GetType().Name}] Sub-state '{stateType.Name}' not found");
                return;
            }

            _isSubTransitioning = true;

            try
            {
                // Exit current sub-state
                if (_currentSubState != null)
                {
                    try
                    {
                        await _currentSubState.OnExit();
                        ConsoleLogger.Log($"[{GetType().Name}] Exited sub-state: {_currentSubState.GetType().Name}");
                    }
                    catch (Exception ex)
                    {
                        ConsoleLogger.LogError($"[{GetType().Name}] Error exiting sub-state {_currentSubState.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                        throw;
                    }
                }

                // Enter new sub-state
                try
                {
                    _currentSubState = _subStates[stateType];
                    await _currentSubState.OnEnter();
                    ConsoleLogger.Log($"[{GetType().Name}] Entered sub-state: {stateType.Name}");
                }
                catch (Exception ex)
                {
                    ConsoleLogger.LogError($"[{GetType().Name}] Error entering sub-state {stateType.Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    throw;
                }
            }
            catch (Exception e)
            {
                ConsoleLogger.LogError($"[{GetType().Name}] Error during sub-state transition to {stateType.Name}: {e.Message}\nStackTrace: {e.StackTrace}");
            }
            finally
            {
                _isSubTransitioning = false;
            }
        }
    }

    /// <summary>
    /// Base class for tickable composite states
    /// </summary>
    public abstract class BaseTickableCompositeState : BaseCompositeState, ITickableState
    {
        public virtual void OnTick()
        {
            // Tick current sub-state if it's tickable
            if (_currentSubState != null && _currentSubState is ITickableState tickableSubState)
            {
                tickableSubState.OnTick();
            }
        }
    }

    /// <summary>
    /// Base class for composite states with context
    /// </summary>
    public abstract class BaseCompositeState<T> : BaseCompositeState, IState<T>
    {
        protected T Context { get; private set; }
        
        public virtual void Initialize(T context)
        {
            Context = context;
            
            // Initialize sub-states with context if they support it
            foreach (var subState in _subStates.Values)
            {
                if (subState is IState<T> contextSubState)
                {
                    contextSubState.Initialize(context);
                }
            }
        }
    }

    /// Base class for tickable composite states with context
    /// </summary>
    public abstract class BaseTickableCompositeState<T> : BaseCompositeState<T>, ITickableState
    {
        public virtual void OnTick()
        {
            // Tick current sub-state if it's tickable
            if (_currentSubState != null && _currentSubState is ITickableState tickableSubState)
            {
                tickableSubState.OnTick();
            }
        }
    }
}