using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.Command
{
    /// <summary>
    /// Base interface for all commands
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Execute the command
        /// </summary>
        UniTask Execute();
        
        /// <summary>
        /// Undo the command (if supported)
        /// </summary>
        UniTask Undo();
        
        /// <summary>
        /// Whether this command can be undone
        /// </summary>
        bool CanUndo { get; }
        
        /// <summary>
        /// Command description for debugging
        /// </summary>
        string Description { get; }
    }
    
    /// <summary>
    /// Interface for commands that can be executed with parameters
    /// </summary>
    /// <typeparam name="T">Parameter type</typeparam>
    public interface IParameterizedCommand<in T> : ICommand
    {
        /// <summary>
        /// Execute the command with parameters
        /// </summary>
        UniTask Execute(T parameter);
    }
}
