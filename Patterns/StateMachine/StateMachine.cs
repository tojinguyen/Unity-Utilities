using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// A flexible state machine implementation for game logic
    /// </summary>
    public class StateMachine : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private string currentStateName = "None";
        
        private readonly Dictionary<string, IState> _states = new();
        private readonly Dictionary<string, List<StateTransition>> _transitions = new();
        
        private IState _currentState;
        private bool _isTransitioning;
        
        public event Action<IState, IState> OnStateChanged;
        public event Action<string> OnTransitionFailed;
        
        /// <summary>
        /// Current active state
        /// </summary>
        public IState CurrentState => _currentState;
        
        /// <summary>
        /// Is the state machine currently transitioning
        /// </summary>
        public bool IsTransitioning => _isTransitioning;
        
        /// <summary>
        /// Add a state to the state machine
        /// </summary>
        public void AddState(IState state)
        {
            if (state == null)
            {
                LogError("Cannot add null state");
                return;
            }
            
            var stateName = state.StateName;
            if (_states.ContainsKey(stateName))
            {
                LogWarning($"State '{stateName}' already exists. Replacing...");
            }
            
            _states[stateName] = state;
            _transitions[stateName] = new List<StateTransition>();
            
            Log($"Added state: {stateName}");
        }
        
        /// <summary>
        /// Remove a state from the state machine
        /// </summary>
        public void RemoveState(string stateName)
        {
            if (!_states.ContainsKey(stateName))
            {
                LogWarning($"State '{stateName}' not found");
                return;
            }
            
            if (_currentState?.StateName == stateName)
            {
                LogError($"Cannot remove current state '{stateName}'");
                return;
            }
            
            _states.Remove(stateName);
            _transitions.Remove(stateName);
            
            // Remove transitions TO this state
            foreach (var transitionList in _transitions.Values)
            {
                transitionList.RemoveAll(t => t.ToState == stateName);
            }
            
            Log($"Removed state: {stateName}");
        }
        
        /// <summary>
        /// Add a transition between states
        /// </summary>
        public void AddTransition(string fromState, string toState, Func<bool> condition = null)
        {
            if (!_states.ContainsKey(fromState))
            {
                LogError($"From state '{fromState}' not found");
                return;
            }
            
            if (!_states.ContainsKey(toState))
            {
                LogError($"To state '{toState}' not found");
                return;
            }
            
            var transition = new StateTransition(toState, condition);
            _transitions[fromState].Add(transition);
            
            Log($"Added transition: {fromState} -> {toState}");
        }
        
        /// <summary>
        /// Start the state machine with an initial state
        /// </summary>
        public async UniTask StartAsync(string initialStateName)
        {
            if (!_states.ContainsKey(initialStateName))
            {
                LogError($"Initial state '{initialStateName}' not found");
                return;
            }
            
            await TransitionToAsync(initialStateName);
            Log($"Started state machine with initial state: {initialStateName}");
        }
        
        /// <summary>
        /// Manually trigger a transition to a specific state
        /// </summary>
        public async UniTask<bool> TransitionToAsync(string stateName)
        {
            if (_isTransitioning)
            {
                LogWarning("Already transitioning, ignoring request");
                return false;
            }
            
            if (!_states.ContainsKey(stateName))
            {
                LogError($"State '{stateName}' not found");
                OnTransitionFailed?.Invoke(stateName);
                return false;
            }
            
            var previousState = _currentState;
            var newState = _states[stateName];
            
            _isTransitioning = true;
            
            try
            {
                // Exit current state
                if (_currentState != null)
                {
                    await _currentState.OnExit();
                    Log($"Exited state: {_currentState.StateName}");
                }
                
                // Enter new state
                _currentState = newState;
                currentStateName = _currentState.StateName;
                await _currentState.OnEnter();
                
                Log($"Entered state: {_currentState.StateName}");
                OnStateChanged?.Invoke(previousState, _currentState);
                
                return true;
            }
            catch (Exception e)
            {
                LogError($"Error during state transition: {e.Message}");
                OnTransitionFailed?.Invoke(stateName);
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
            if (_currentState == null || _isTransitioning)
                return;
            
            var currentStateName = _currentState.StateName;
            if (!_transitions.ContainsKey(currentStateName))
                return;
            
            foreach (var transition in _transitions[currentStateName])
            {
                if (transition.Condition == null || transition.Condition.Invoke())
                {
                    await TransitionToAsync(transition.ToState);
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
                Log($"Exited final state: {_currentState.StateName}");
            }
            
            _currentState = null;
            currentStateName = "None";
            Log("State machine stopped");
        }
        
        private void Update()
        {
            if (_currentState != null && !_isTransitioning)
            {
                _currentState.OnUpdate();
            }
        }
        
        private void FixedUpdate()
        {
            if (_currentState != null && !_isTransitioning)
            {
                _currentState.OnFixedUpdate();
            }
        }
        
        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[StateMachine] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            if (enableDebugLogs)
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
    /// Represents a transition between states
    /// </summary>
    internal class StateTransition
    {
        public string ToState { get; }
        public Func<bool> Condition { get; }
        
        public StateTransition(string toState, Func<bool> condition = null)
        {
            ToState = toState;
            Condition = condition;
        }
    }
}
