namespace TirexGame.Utils.Patterns.Strategy
{
    /// <summary>
    /// Base interface for strategy pattern
    /// </summary>
    /// <typeparam name="TInput">Input parameter type</typeparam>
    /// <typeparam name="TOutput">Output result type</typeparam>
    public interface IStrategy<in TInput, out TOutput>
    {
        /// <summary>
        /// Execute the strategy
        /// </summary>
        TOutput Execute(TInput input);
        
        /// <summary>
        /// Strategy name for identification
        /// </summary>
        string StrategyName { get; }
    }
    
    /// <summary>
    /// Strategy interface without return value
    /// </summary>
    /// <typeparam name="TInput">Input parameter type</typeparam>
    public interface IStrategy<in TInput> : IStrategy<TInput, object>
    {
        /// <summary>
        /// Execute the strategy without return value
        /// </summary>
        void Execute(TInput input);
    }
    
    /// <summary>
    /// Strategy interface without parameters or return value
    /// </summary>
    public interface IStrategy : IStrategy<object, object>
    {
        /// <summary>
        /// Execute the strategy
        /// </summary>
        void Execute();
    }
}
