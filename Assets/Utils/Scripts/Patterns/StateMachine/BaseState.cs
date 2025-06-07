using System;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// Base abstract class for states
    /// </summary>
    public abstract class BaseState : IState
    {
        public abstract string StateName { get; }
        
        public virtual async UniTask OnEnter()
        {
            await UniTask.Yield();
        }
        
        public virtual void OnUpdate()
        {
            // Override in derived classes
        }
        
        public virtual void OnFixedUpdate()
        {
            // Override in derived classes
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
}
