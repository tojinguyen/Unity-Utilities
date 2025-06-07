using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.Command
{
    /// <summary>
    /// Base implementation of ICommand
    /// </summary>
    public abstract class BaseCommand : ICommand
    {
        public abstract UniTask Execute();
        public virtual UniTask Undo() => UniTask.CompletedTask;
        public virtual bool CanUndo => false;
        public abstract string Description { get; }
    }
    
    /// <summary>
    /// Base implementation for undoable commands
    /// </summary>
    public abstract class UndoableCommand : BaseCommand
    {
        public override bool CanUndo => true;
        public abstract override UniTask Undo();
    }
    
    /// <summary>
    /// Base implementation for parameterized commands
    /// </summary>
    /// <typeparam name="T">Parameter type</typeparam>
    public abstract class ParameterizedCommand<T> : BaseCommand, IParameterizedCommand<T>
    {
        protected T Parameter { get; private set; }
        
        public override async UniTask Execute()
        {
            await Execute(Parameter);
        }
        
        public virtual async UniTask Execute(T parameter)
        {
            Parameter = parameter;
            await ExecuteWithParameter();
        }
        
        protected abstract UniTask ExecuteWithParameter();
    }
}
