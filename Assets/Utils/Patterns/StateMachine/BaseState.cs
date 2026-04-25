using System.Threading;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// Base abstract class for states.
    /// Override OnEnter/OnExit as needed — both return UniTask.CompletedTask by default (zero overhead).
    /// </summary>
    public abstract class BaseState : IState
    {
        public virtual UniTask OnEnter(CancellationToken ct = default) => UniTask.CompletedTask;
        public virtual UniTask OnExit(CancellationToken ct = default) => UniTask.CompletedTask;
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
        public virtual void OnTick() { }
    }

    /// <summary>
    /// Base abstract class for tickable states with context
    /// </summary>
    /// <typeparam name="T">The context type</typeparam>
    public abstract class BaseTickableState<T> : BaseState<T>, ITickableState
    {
        public virtual void OnTick() { }
    }
}
