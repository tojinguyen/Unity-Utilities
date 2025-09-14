using System;
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.Patterns.Strategy
{
    /// <summary>
    /// Context class that uses strategies
    /// </summary>
    /// <typeparam name="TInput">Input parameter type</typeparam>
    /// <typeparam name="TOutput">Output result type</typeparam>
    public class StrategyContext<TInput, TOutput>
    {
        private readonly Dictionary<string, IStrategy<TInput, TOutput>> _strategies = new();
        private IStrategy<TInput, TOutput> _currentStrategy;
        
        public event Action<string> OnStrategyChanged;
        
        /// <summary>
        /// Current active strategy name
        /// </summary>
        public string CurrentStrategyName => _currentStrategy?.StrategyName ?? "None";
        
        /// <summary>
        /// Register a strategy with the context
        /// </summary>
        public void RegisterStrategy(IStrategy<TInput, TOutput> strategy)
        {
            if (strategy == null)
            {
                Debug.LogError("[StrategyContext] Cannot register null strategy");
                return;
            }
            
            _strategies[strategy.StrategyName] = strategy;
            Debug.Log($"[StrategyContext] Registered strategy: {strategy.StrategyName}");
        }
        
        /// <summary>
        /// Unregister a strategy
        /// </summary>
        public void UnregisterStrategy(string strategyName)
        {
            if (_strategies.Remove(strategyName))
            {
                if (_currentStrategy?.StrategyName == strategyName)
                {
                    _currentStrategy = null;
                }
                Debug.Log($"[StrategyContext] Unregistered strategy: {strategyName}");
            }
        }
        
        /// <summary>
        /// Set the active strategy
        /// </summary>
        public bool SetStrategy(string strategyName)
        {
            if (_strategies.TryGetValue(strategyName, out var strategy))
            {
                _currentStrategy = strategy;
                OnStrategyChanged?.Invoke(strategyName);
                Debug.Log($"[StrategyContext] Switched to strategy: {strategyName}");
                return true;
            }
            
            Debug.LogError($"[StrategyContext] Strategy not found: {strategyName}");
            return false;
        }
        
        /// <summary>
        /// Execute the current strategy
        /// </summary>
        public TOutput Execute(TInput input)
        {
            if (_currentStrategy == null)
            {
                Debug.LogError("[StrategyContext] No strategy set");
                return default(TOutput);
            }
            
            return _currentStrategy.Execute(input);
        }
        
        /// <summary>
        /// Get all registered strategy names
        /// </summary>
        public IEnumerable<string> GetRegisteredStrategies()
        {
            return _strategies.Keys;
        }
        
        /// <summary>
        /// Check if a strategy is registered
        /// </summary>
        public bool HasStrategy(string strategyName)
        {
            return _strategies.ContainsKey(strategyName);
        }
    }
    
    /// <summary>
    /// MonoBehaviour-based strategy context for Unity integration
    /// </summary>
    /// <typeparam name="TInput">Input parameter type</typeparam>
    /// <typeparam name="TOutput">Output result type</typeparam>
    public class StrategyContextMono<TInput, TOutput> : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private string currentStrategyName = "None";
        
        private readonly StrategyContext<TInput, TOutput> _context = new();
        
        public event Action<string> OnStrategyChanged;
        
        /// <summary>
        /// Current active strategy name
        /// </summary>
        public string CurrentStrategyName => _context.CurrentStrategyName;
        
        private void Awake()
        {
            _context.OnStrategyChanged += (name) =>
            {
                currentStrategyName = name;
                OnStrategyChanged?.Invoke(name);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[{gameObject.name}] Strategy changed to: {name}");
                }
            };
        }
        
        /// <summary>
        /// Register a strategy with the context
        /// </summary>
        public void RegisterStrategy(IStrategy<TInput, TOutput> strategy)
        {
            _context.RegisterStrategy(strategy);
        }
        
        /// <summary>
        /// Unregister a strategy
        /// </summary>
        public void UnregisterStrategy(string strategyName)
        {
            _context.UnregisterStrategy(strategyName);
        }
        
        /// <summary>
        /// Set the active strategy
        /// </summary>
        public bool SetStrategy(string strategyName)
        {
            return _context.SetStrategy(strategyName);
        }
        
        /// <summary>
        /// Execute the current strategy
        /// </summary>
        public TOutput Execute(TInput input)
        {
            return _context.Execute(input);
        }
        
        /// <summary>
        /// Get all registered strategy names
        /// </summary>
        public IEnumerable<string> GetRegisteredStrategies()
        {
            return _context.GetRegisteredStrategies();
        }
        
        /// <summary>
        /// Check if a strategy is registered
        /// </summary>
        public bool HasStrategy(string strategyName)
        {
            return _context.HasStrategy(strategyName);
        }
    }
}
