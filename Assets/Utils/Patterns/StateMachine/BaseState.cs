using System;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// Base abstract class for states
    /// </summary>
    public abstract class BaseState : IState
    {
        public virtual async UniTask OnEnter()
        {
            try
            {
                await UniTask.Yield();
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[{GetType().Name}] Error in OnEnter: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }
        
        public virtual async UniTask OnExit()
        {
            try
            {
                await UniTask.Yield();
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[{GetType().Name}] Error in OnExit: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Base abstract class for states with context
    /// </summary>
    /// <typeparam name="T">The context type</typeparam>
    public abstract class BaseState<T> : BaseState, IState<T>
    {
        protected T Context { get; private set; }
        
        public virtual void Initialize(T context)
        {
            try
            {
                Context = context;
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[{GetType().Name}] Error in Initialize: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Base abstract class for tickable states
    /// </summary>
    public abstract class BaseTickableState : BaseState, ITickableState
    {
        public virtual void OnTick()
        {
            try
            {
                // Override in derived classes
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[{GetType().Name}] Error in OnTick: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Base abstract class for tickable states with context
    /// </summary>
    /// <typeparam name="T">The context type</typeparam>
    public abstract class BaseTickableState<T> : BaseState<T>, ITickableState
    {
        public virtual void OnTick()
        {
            // Override in derived classes
        }
    }
}
