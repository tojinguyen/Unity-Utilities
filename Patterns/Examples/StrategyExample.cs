using UnityEngine;
using TirexGame.Utils.Patterns.Strategy;

namespace TirexGame.Utils.Patterns.Examples
{
    /// <summary>
    /// Example AI strategies for different enemy behaviors
    /// </summary>
    public class AIStrategies
    {
        /// <summary>
        /// Context data for AI strategies
        /// </summary>
        public class AIContext
        {
            public Transform enemyTransform;
            public Transform playerTransform;
            public float detectionRange;
            public float attackRange;
            public float moveSpeed;
            public float deltaTime;
        }
        
        /// <summary>
        /// Aggressive AI that always moves toward player
        /// </summary>
        public class AggressiveStrategy : IStrategy<AIContext, Vector3>
        {
            public string StrategyName => "Aggressive";
            
            public Vector3 Execute(AIContext context)
            {
                if (context?.playerTransform == null || context?.enemyTransform == null)
                    return Vector3.zero;
                
                var direction = (context.playerTransform.position - context.enemyTransform.position).normalized;
                return direction * context.moveSpeed * context.deltaTime;
            }
        }
        
        /// <summary>
        /// Defensive AI that keeps distance from player
        /// </summary>
        public class DefensiveStrategy : IStrategy<AIContext, Vector3>
        {
            public string StrategyName => "Defensive";
            
            public Vector3 Execute(AIContext context)
            {
                if (context?.playerTransform == null || context?.enemyTransform == null)
                    return Vector3.zero;
                
                var distance = Vector3.Distance(context.playerTransform.position, context.enemyTransform.position);
                var direction = (context.enemyTransform.position - context.playerTransform.position).normalized;
                
                // Move away if too close, move closer if too far
                if (distance < context.detectionRange * 0.5f)
                {
                    return direction * context.moveSpeed * context.deltaTime; // Move away
                }
                else if (distance > context.detectionRange)
                {
                    return -direction * context.moveSpeed * 0.5f * context.deltaTime; // Move closer slowly
                }
                
                return Vector3.zero; // Stay in position
            }
        }
        
        /// <summary>
        /// Patrol AI that moves in a pattern
        /// </summary>
        public class PatrolStrategy : IStrategy<AIContext, Vector3>
        {
            private Vector3[] _patrolPoints;
            private int _currentPatrolIndex = 0;
            private bool _isReturning = false;
            
            public string StrategyName => "Patrol";
            
            public PatrolStrategy(Vector3[] patrolPoints)
            {
                _patrolPoints = patrolPoints ?? new Vector3[0];
            }
            
            public Vector3 Execute(AIContext context)
            {
                if (context?.enemyTransform == null || _patrolPoints.Length == 0)
                    return Vector3.zero;
                
                var targetPoint = _patrolPoints[_currentPatrolIndex];
                var distance = Vector3.Distance(context.enemyTransform.position, targetPoint);
                
                // If close enough to target, move to next patrol point
                if (distance < 0.1f)
                {
                    if (!_isReturning)
                    {
                        _currentPatrolIndex++;
                        if (_currentPatrolIndex >= _patrolPoints.Length)
                        {
                            _currentPatrolIndex = _patrolPoints.Length - 2;
                            _isReturning = true;
                        }
                    }
                    else
                    {
                        _currentPatrolIndex--;
                        if (_currentPatrolIndex < 0)
                        {
                            _currentPatrolIndex = 1;
                            _isReturning = false;
                        }
                    }
                    
                    if (_currentPatrolIndex >= 0 && _currentPatrolIndex < _patrolPoints.Length)
                    {
                        targetPoint = _patrolPoints[_currentPatrolIndex];
                    }
                }
                
                var direction = (targetPoint - context.enemyTransform.position).normalized;
                return direction * context.moveSpeed * 0.5f * context.deltaTime;
            }
        }
        
        /// <summary>
        /// Smart AI that switches between strategies based on conditions
        /// </summary>
        public class AdaptiveStrategy : IStrategy<AIContext, Vector3>
        {
            private readonly IStrategy<AIContext, Vector3> _aggressiveStrategy = new AggressiveStrategy();
            private readonly IStrategy<AIContext, Vector3> _defensiveStrategy = new DefensiveStrategy();
            private float _health = 100f;
            
            public string StrategyName => "Adaptive";
            
            public Vector3 Execute(AIContext context)
            {
                if (context?.playerTransform == null || context?.enemyTransform == null)
                    return Vector3.zero;
                
                var distance = Vector3.Distance(context.playerTransform.position, context.enemyTransform.position);
                
                // Use aggressive strategy when healthy and player is close
                if (_health > 50f && distance <= context.attackRange * 1.5f)
                {
                    return _aggressiveStrategy.Execute(context);
                }
                // Use defensive strategy when hurt or player is far
                else
                {
                    return _defensiveStrategy.Execute(context);
                }
            }
            
            public void TakeDamage(float damage)
            {
                _health -= damage;
                _health = Mathf.Max(0f, _health);
            }
        }
    }
    
    /// <summary>
    /// Example enemy AI controller using strategy pattern
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private Transform player;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float moveSpeed = 3f;
        
        [Header("Strategy")]
        [SerializeField] private string currentStrategy = "Aggressive";
        [SerializeField] private Vector3[] patrolPoints;
        
        private StrategyContextMono<AIStrategies.AIContext, Vector3> _strategyContext;
        private AIStrategies.AIContext _aiContext;
        
        private void Awake()
        {
            // Add strategy context component
            _strategyContext = gameObject.AddComponent<StrategyContextMono<AIStrategies.AIContext, Vector3>>();
            
            // Initialize AI context
            _aiContext = new AIStrategies.AIContext
            {
                enemyTransform = transform,
                playerTransform = player,
                detectionRange = detectionRange,
                attackRange = attackRange,
                moveSpeed = moveSpeed
            };
        }
        
        private void Start()
        {
            // Register strategies
            _strategyContext.RegisterStrategy(new AIStrategies.AggressiveStrategy());
            _strategyContext.RegisterStrategy(new AIStrategies.DefensiveStrategy());
            _strategyContext.RegisterStrategy(new AIStrategies.PatrolStrategy(patrolPoints));
            _strategyContext.RegisterStrategy(new AIStrategies.AdaptiveStrategy());
            
            // Set initial strategy
            _strategyContext.SetStrategy(currentStrategy);
        }
        
        private void Update()
        {
            // Update context
            _aiContext.deltaTime = Time.deltaTime;
            
            // Execute current strategy
            var movement = _strategyContext.Execute(_aiContext);
            
            // Apply movement
            if (movement != Vector3.zero)
            {
                transform.position += movement;
                transform.LookAt(transform.position + movement);
            }
            
            // Switch strategies based on input (for testing)
            HandleStrategySwitch();
        }
        
        private void HandleStrategySwitch()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _strategyContext.SetStrategy("Aggressive");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _strategyContext.SetStrategy("Defensive");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _strategyContext.SetStrategy("Patrol");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _strategyContext.SetStrategy("Adaptive");
            }
        }
        
        /// <summary>
        /// Method to switch strategy programmatically
        /// </summary>
        public void ChangeStrategy(string strategyName)
        {
            if (_strategyContext.HasStrategy(strategyName))
            {
                _strategyContext.SetStrategy(strategyName);
                Debug.Log($"Enemy AI switched to: {strategyName}");
            }
        }
        
        /// <summary>
        /// Get current strategy name
        /// </summary>
        public string GetCurrentStrategy()
        {
            return _strategyContext.CurrentStrategyName;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw patrol points
            if (patrolPoints != null && patrolPoints.Length > 1)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i], 0.5f);
                    if (i < patrolPoints.Length - 1)
                    {
                        Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
                    }
                }
            }
        }
    }
}
