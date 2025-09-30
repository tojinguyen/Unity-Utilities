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
            await UniTask.Yield();
        }
        
        public virtual async UniTask OnExit()
        {
            await UniTask.Yield();
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
            Context = context;
        }
    }
    
    /// <summary>
    /// Base abstract class for tickable states
    /// </summary>
    public abstract class BaseTickableState : BaseState, ITickableState
    {
        public virtual void OnTick()
        {
            // Override in derived classes
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
