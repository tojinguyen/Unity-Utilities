using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// A flexible state machine implementation for game logic without MonoBehaviour dependency
    /// </summary>
    public class StateMachine
    {
        private readonly bool _enableDebugLogs;
        private Type _currentStateType;
        
        private readonly Dictionary<Type, IState> _states = new();
        private readonly Dictionary<Type, List<StateTransition>> _transitions = new();
        
        private IState _currentState;
        private bool _isTransitioning;
        
        public event Action<IState, IState> OnStateChanged;
        public event Action<Type> OnTransitionFailed;
        
        /// <summary>
        /// Create a new StateMachine instance
        /// </summary>
        /// <param name="enableDebugLogs">Enable debug logging</param>
        public StateMachine(bool enableDebugLogs = true)
        {
            _enableDebugLogs = enableDebugLogs;
        }
        
        /// <summary>
        /// Current active state
        /// </summary>
        public IState CurrentState => _currentState;
        
        /// <summary>
        /// Current active state type
        /// </summary>
        public Type CurrentStateType => _currentStateType;
        
        /// <summary>
        /// Is the state machine currently transitioning
        /// </summary>
        public bool IsTransitioning => _isTransitioning;
        
        /// <summary>
        /// Add a state to the state machine
        /// </summary>
        public void AddState<T>(T state) where T : class, IState
        {
            if (state == null)
            {
                LogError("Cannot add null state");
                return;
            }
            
            var stateType = typeof(T);
            if (_states.ContainsKey(stateType))
            {
                LogWarning($"State '{stateType.Name}' already exists. Replacing...");
            }
            
            _states[stateType] = state;
            _transitions[stateType] = new List<StateTransition>();
            
            Log($"Added state: {stateType.Name}");
        }
        
        /// <summary>
        /// Remove a state from the state machine
        /// </summary>
        public void RemoveState<T>() where T : class, IState
        {
            var stateType = typeof(T);
            if (!_states.ContainsKey(stateType))
            {
                LogWarning($"State '{stateType.Name}' not found");
                return;
            }
            
            if (_currentStateType == stateType)
            {
                LogError($"Cannot remove current state '{stateType.Name}'");
                return;
            }
            
            _states.Remove(stateType);
            _transitions.Remove(stateType);
            
            // Remove transitions TO this state
            foreach (var transitionList in _transitions.Values)
            {
                transitionList.RemoveAll(t => t.ToStateType == stateType);
            }
            
            Log($"Removed state: {stateType.Name}");
        }
        
        /// <summary>
        /// Add a transition between states
        /// </summary>
        public void AddTransition<TFrom, TTo>(Func<bool> condition = null) 
            where TFrom : class, IState 
            where TTo : class, IState
        {
            var fromStateType = typeof(TFrom);
            var toStateType = typeof(TTo);
            
            if (!_states.ContainsKey(fromStateType))
            {
                LogError($"From state '{fromStateType.Name}' not found");
                return;
            }
            
            if (!_states.ContainsKey(toStateType))
            {
                LogError($"To state '{toStateType.Name}' not found");
                return;
            }
            
            var transition = new StateTransition(toStateType, condition);
            _transitions[fromStateType].Add(transition);
            
            Log($"Added transition: {fromStateType.Name} -> {toStateType.Name}");
        }
        
        /// <summary>
        /// Start the state machine with an initial state
        /// </summary>
        public async UniTask StartAsync<T>() where T : class, IState
        {
            var stateType = typeof(T);
            if (!_states.ContainsKey(stateType))
            {
                LogError($"Initial state '{stateType.Name}' not found");
                return;
            }
            
            await TransitionToAsync<T>();
            Log($"Started state machine with initial state: {stateType.Name}");
        }
        
        /// <summary>
        /// Manually trigger a transition to a specific state
        /// </summary>
        public async UniTask<bool> TransitionToAsync<T>() where T : class, IState
        {
            var stateType = typeof(T);
            
            if (_isTransitioning)
            {
                LogWarning("Already transitioning, ignoring request");
                return false;
            }
            
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
                // Exit current state
                if (_currentState != null)
                {
                    await _currentState.OnExit();
                    Log($"Exited state: {_currentStateType?.Name}");
                }
                
                // Enter new state
                _currentState = newState;
                _currentStateType = stateType;
                await _currentState.OnEnter();
                
                Log($"Entered state: {_currentStateType.Name}");
                OnStateChanged?.Invoke(previousState, _currentState);
                
                return true;
            }
            catch (Exception e)
            {
                LogError($"Error during state transition: {e.Message}");
                OnTransitionFailed?.Invoke(stateType);
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
            if (_currentState == null || _isTransitioning || _currentStateType == null)
                return;
            
            if (!_transitions.ContainsKey(_currentStateType))
                return;
            
            foreach (var transition in _transitions[_currentStateType])
            {
                if (transition.Condition == null || transition.Condition.Invoke())
                {
                    // Use reflection to call the generic TransitionToAsync method
                    var method = GetType().GetMethod(nameof(TransitionToAsync));
                    var genericMethod = method.MakeGenericMethod(transition.ToStateType);
                    await (UniTask<bool>)genericMethod.Invoke(this, null);
                    break; // Only execute first valid transition
                }
            }
        }
        
        /// <summary>
        /// Stop the state machine
        /// </summary>
        public async UniTask StopAsync()
        {
            if (_currentState != null)
            {
                await _currentState.OnExit();
                Log($"Exited final state: {_currentStateType?.Name}");
            }
            
            _currentState = null;
            _currentStateType = null;
            Log("State machine stopped");
        }
        
        /// <summary>
        /// Manual tick method for states that need periodic updates
        /// Call this method manually when you need to update states
        /// </summary>
        public void Tick()
        {
            if (_currentState != null && !_isTransitioning && _currentState is ITickableState tickableState)
            {
                tickableState.OnTick();
            }
        }
        
        private void Log(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[StateMachine] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.LogWarning($"[StateMachine] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[StateMachine] {message}");
        }
    }
    
    /// <summary>
    /// Interface for states that need manual ticking
    /// </summary>
    public interface ITickableState
    {
        void OnTick();
    }
    
    /// <summary>
    /// Represents a transition between states
    /// </summary>
    internal class StateTransition
    {
        public Type ToStateType { get; }
        public Func<bool> Condition { get; }
        
        public StateTransition(Type toStateType, Func<bool> condition = null)
        {
            ToStateType = toStateType;
            Condition = condition;
        }
    }
}
