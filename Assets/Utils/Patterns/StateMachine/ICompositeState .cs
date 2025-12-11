namespace TirexGame.Utils.Patterns.StateMachine
{
    /// <summary>
    /// Interface for states that can contain sub-states (Composite State)
    /// </summary>
    public interface ICompositeState : IState
    {
        /// <summary>
        /// Add a sub-state to this composite state
        /// </summary>
        void AddSubState(IState subState, bool isInitial = false);
        
        /// <summary>
        /// Add transition between sub-states
        /// </summary>
        void AddSubTransition<TFrom, TTo>(Func<bool> condition = null) 
            where TFrom : class, IState 
            where TTo : class, IState;
        
        /// <summary>
        /// Check and execute sub-state transitions
        /// </summary>
        UniTask CheckSubTransitionsAsync();
        
        /// <summary>
        /// Manually transition to a sub-state
        /// </summary>
        UniTask TransitionToSubStateAsync<T>() where T : class, IState;
        
        /// <summary>
        /// Get current active sub-state
        /// </summary>
        IState CurrentSubState { get; }
    }
}