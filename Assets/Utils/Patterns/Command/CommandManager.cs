using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.Command
{
    /// <summary>
    /// Command manager that handles execution, queuing, and undo/redo functionality
    /// </summary>
    public class CommandManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private int maxUndoHistory = 50;
        [SerializeField] private bool enableQueue = true;
        
        private readonly Queue<ICommand> _commandQueue = new();
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        
        private bool _isProcessing;
        
        public event Action<ICommand> OnCommandExecuted;
        public event Action<ICommand> OnCommandUndone;
        public event Action<ICommand> OnCommandRedone;
        
        /// <summary>
        /// Number of commands that can be undone
        /// </summary>
        public int UndoCount => _undoStack.Count;
        
        /// <summary>
        /// Number of commands that can be redone
        /// </summary>
        public int RedoCount => _redoStack.Count;
        
        /// <summary>
        /// Whether the manager is currently processing commands
        /// </summary>
        public bool IsProcessing => _isProcessing;
        
        /// <summary>
        /// Execute a command immediately
        /// </summary>
        public async UniTask<bool> ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
                LogError("Cannot execute null command");
                return false;
            }
            
            try
            {
                Log($"Executing command: {command.Description}");
                await command.Execute();
                
                // Add to undo stack if it can be undone
                if (command.CanUndo)
                {
                    _undoStack.Push(command);
                    _redoStack.Clear(); // Clear redo stack when new command is executed
                    
                    // Maintain max history
                    while (_undoStack.Count > maxUndoHistory)
                    {
                        var oldestCommand = new ICommand[_undoStack.Count];
                        _undoStack.CopyTo(oldestCommand, 0);
                        _undoStack.Clear();
                        
                        for (int i = 1; i < oldestCommand.Length; i++)
                        {
                            _undoStack.Push(oldestCommand[i]);
                        }
                    }
                }
                
                OnCommandExecuted?.Invoke(command);
                return true;
            }
            catch (Exception e)
            {
                LogError($"Error executing command {command.Description}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Queue a command for execution
        /// </summary>
        public void QueueCommand(ICommand command)
        {
            if (!enableQueue)
            {
                ExecuteCommand(command).Forget();
                return;
            }
            
            if (command == null)
            {
                LogError("Cannot queue null command");
                return;
            }
            
            _commandQueue.Enqueue(command);
            Log($"Queued command: {command.Description}");
            
            // Start processing if not already doing so
            if (!_isProcessing)
            {
                ProcessQueue().Forget();
            }
        }
        
        /// <summary>
        /// Process all queued commands
        /// </summary>
        private async UniTask ProcessQueue()
        {
            _isProcessing = true;
            
            while (_commandQueue.Count > 0)
            {
                var command = _commandQueue.Dequeue();
                await ExecuteCommand(command);
                
                // Yield control to avoid blocking
                await UniTask.Yield();
            }
            
            _isProcessing = false;
        }
        
        /// <summary>
        /// Undo the last executed command
        /// </summary>
        public async UniTask<bool> Undo()
        {
            if (_undoStack.Count == 0)
            {
                Log("No commands to undo");
                return false;
            }
            
            var command = _undoStack.Pop();
            
            try
            {
                Log($"Undoing command: {command.Description}");
                await command.Undo();
                
                _redoStack.Push(command);
                OnCommandUndone?.Invoke(command);
                return true;
            }
            catch (Exception e)
            {
                LogError($"Error undoing command {command.Description}: {e.Message}");
                // Put command back on undo stack
                _undoStack.Push(command);
                return false;
            }
        }
        
        /// <summary>
        /// Redo the last undone command
        /// </summary>
        public async UniTask<bool> Redo()
        {
            if (_redoStack.Count == 0)
            {
                Log("No commands to redo");
                return false;
            }
            
            var command = _redoStack.Pop();
            
            try
            {
                Log($"Redoing command: {command.Description}");
                await command.Execute();
                
                _undoStack.Push(command);
                OnCommandRedone?.Invoke(command);
                return true;
            }
            catch (Exception e)
            {
                LogError($"Error redoing command {command.Description}: {e.Message}");
                // Put command back on redo stack
                _redoStack.Push(command);
                return false;
            }
        }
        
        /// <summary>
        /// Clear all command history
        /// </summary>
        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            Log("Command history cleared");
        }
        
        /// <summary>
        /// Clear the command queue
        /// </summary>
        public void ClearQueue()
        {
            _commandQueue.Clear();
            Log("Command queue cleared");
        }
        
        /// <summary>
        /// Get command history for debugging
        /// </summary>
        public string GetHistoryDebugString()
        {
            var undoCommands = new List<string>();
            foreach (var command in _undoStack)
            {
                undoCommands.Add(command.Description);
            }
            
            var redoCommands = new List<string>();
            foreach (var command in _redoStack)
            {
                redoCommands.Add(command.Description);
            }
            
            return $"Undo Stack ({undoCommands.Count}): [{string.Join(", ", undoCommands)}]\n" +
                   $"Redo Stack ({redoCommands.Count}): [{string.Join(", ", redoCommands)}]";
        }
        
        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[CommandManager] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[CommandManager] {message}");
        }
        
        private void OnDestroy()
        {
            ClearHistory();
            ClearQueue();
        }
    }
}
