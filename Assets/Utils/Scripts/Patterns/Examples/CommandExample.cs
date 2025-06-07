using UnityEngine;
using Cysharp.Threading.Tasks;
using TirexGame.Utils.Patterns.Command;

namespace TirexGame.Utils.Patterns.Examples
{
    /// <summary>
    /// Example commands for game actions
    /// </summary>
    public class GameCommands
    {
        /// <summary>
        /// Command to move a player
        /// </summary>
        public class MovePlayerCommand : UndoableCommand
        {
            private readonly Transform _player;
            private readonly Vector3 _direction;
            private readonly float _distance;
            private Vector3 _previousPosition;
            
            public override string Description => $"Move Player {_direction * _distance}";
            
            public MovePlayerCommand(Transform player, Vector3 direction, float distance)
            {
                _player = player;
                _direction = direction.normalized;
                _distance = distance;
            }
            
            public override async UniTask Execute()
            {
                if (_player == null) return;
                
                _previousPosition = _player.position;
                _player.position += _direction * _distance;
                
                // Simulate movement animation
                await UniTask.Delay(100);
            }
            
            public override async UniTask Undo()
            {
                if (_player == null) return;
                
                _player.position = _previousPosition;
                await UniTask.Delay(100);
            }
        }
        
        /// <summary>
        /// Command to spawn an enemy
        /// </summary>
        public class SpawnEnemyCommand : UndoableCommand
        {
            private readonly Vector3 _spawnPosition;
            private readonly GameObject _enemyPrefab;
            private GameObject _spawnedEnemy;
            
            public override string Description => $"Spawn Enemy at {_spawnPosition}";
            
            public SpawnEnemyCommand(GameObject enemyPrefab, Vector3 spawnPosition)
            {
                _enemyPrefab = enemyPrefab;
                _spawnPosition = spawnPosition;
            }
            
            public override async UniTask Execute()
            {
                if (_enemyPrefab == null) return;
                
                _spawnedEnemy = Object.Instantiate(_enemyPrefab, _spawnPosition, Quaternion.identity);
                await UniTask.Delay(50);
            }
            
            public override async UniTask Undo()
            {
                if (_spawnedEnemy != null)
                {
                    Object.Destroy(_spawnedEnemy);
                    _spawnedEnemy = null;
                }
                await UniTask.Delay(50);
            }
        }
        
        /// <summary>
        /// Command to play an audio clip
        /// </summary>
        public class PlayAudioCommand : BaseCommand
        {
            private readonly string _audioId;
            
            public override string Description => $"Play Audio: {_audioId}";
            
            public PlayAudioCommand(string audioId)
            {
                _audioId = audioId;
            }
            
            public override async UniTask Execute()
            {
                // Use your audio system
                // AudioService.PlaySFX(_audioId);
                Debug.Log($"Playing audio: {_audioId}");
                await UniTask.Delay(10);
            }
        }
        
        /// <summary>
        /// Composite command for complex actions
        /// </summary>
        public class AttackComboCommand : UndoableCommand
        {
            private readonly ICommand[] _commands;
            private int _executedCommands = 0;
            
            public override string Description => "Attack Combo";
            
            public AttackComboCommand(params ICommand[] commands)
            {
                _commands = commands;
            }
            
            public override async UniTask Execute()
            {
                _executedCommands = 0;
                
                foreach (var command in _commands)
                {
                    await command.Execute();
                    _executedCommands++;
                }
            }
            
            public override async UniTask Undo()
            {
                // Undo in reverse order
                for (int i = _executedCommands - 1; i >= 0; i--)
                {
                    if (_commands[i].CanUndo)
                    {
                        await _commands[i].Undo();
                    }
                }
                
                _executedCommands = 0;
            }
        }
    }
    
    /// <summary>
    /// Example controller demonstrating command pattern usage
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CommandManager commandManager;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameObject enemyPrefab;
        
        [Header("Settings")]
        [SerializeField] private float moveDistance = 1f;
        [SerializeField] private float moveSpeed = 5f;
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            // Movement commands
            if (Input.GetKeyDown(KeyCode.W))
            {
                var command = new GameCommands.MovePlayerCommand(playerTransform, Vector3.forward, moveDistance);
                commandManager.QueueCommand(command);
            }
            
            if (Input.GetKeyDown(KeyCode.S))
            {
                var command = new GameCommands.MovePlayerCommand(playerTransform, Vector3.back, moveDistance);
                commandManager.QueueCommand(command);
            }
            
            if (Input.GetKeyDown(KeyCode.A))
            {
                var command = new GameCommands.MovePlayerCommand(playerTransform, Vector3.left, moveDistance);
                commandManager.QueueCommand(command);
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                var command = new GameCommands.MovePlayerCommand(playerTransform, Vector3.right, moveDistance);
                commandManager.QueueCommand(command);
            }
            
            // Spawn enemy command
            if (Input.GetKeyDown(KeyCode.E))
            {
                var spawnPos = playerTransform.position + Vector3.forward * 2f;
                var command = new GameCommands.SpawnEnemyCommand(enemyPrefab, spawnPos);
                commandManager.QueueCommand(command);
            }
            
            // Audio command
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var command = new GameCommands.PlayAudioCommand("jump_sound");
                commandManager.QueueCommand(command);
            }
            
            // Combo command
            if (Input.GetKeyDown(KeyCode.C))
            {
                var comboCommand = new GameCommands.AttackComboCommand(
                    new GameCommands.PlayAudioCommand("attack_sound_1"),
                    new GameCommands.MovePlayerCommand(playerTransform, Vector3.forward, 0.5f),
                    new GameCommands.PlayAudioCommand("attack_sound_2"),
                    new GameCommands.SpawnEnemyCommand(enemyPrefab, playerTransform.position + Vector3.right)
                );
                commandManager.QueueCommand(comboCommand);
            }
            
            // Undo/Redo
            if (Input.GetKeyDown(KeyCode.Z))
            {
                commandManager.Undo().Forget();
            }
            
            if (Input.GetKeyDown(KeyCode.Y))
            {
                commandManager.Redo().Forget();
            }
            
            // Debug command history
            if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log(commandManager.GetHistoryDebugString());
            }
        }
    }
}
