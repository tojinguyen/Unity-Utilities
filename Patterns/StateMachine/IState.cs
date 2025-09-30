using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// Interface for state machine states
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when entering this state
        /// </summary>
        UniTask OnEnter();
        
        /// <summary>
        /// Called when exiting this state
        /// </summary>
        UniTask OnExit();
    }
    
    /// <summary>
    /// Generic interface for states with context
    /// </summary>
    /// <typeparam name="T">The context type</typeparam>
    public interface IState<T> : IState
    {
        /// <summary>
        /// Initialize the state with context
        /// </summary>
        void Initialize(T context);
    }
}
