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
        /// Called every frame while in this state
        /// </summary>
        void OnUpdate();
        
        /// <summary>
        /// Called every fixed frame while in this state
        /// </summary>
        void OnFixedUpdate();
        
        /// <summary>
        /// Called when exiting this state
        /// </summary>
        UniTask OnExit();
        
        /// <summary>
        /// State identifier
        /// </summary>
        string StateName { get; }
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
