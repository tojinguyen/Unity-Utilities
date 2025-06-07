using System;
using UnityEngine;
using TirexGame.Utils.Patterns.ServiceLocator;

namespace TirexGame.Utils.Patterns.Examples
{
    // Example service interfaces
    public interface IPlayerService
    {
        void MovePlayer(Vector3 direction);
        Vector3 GetPlayerPosition();
    }
    
    public interface IScoreService
    {
        void AddScore(int points);
        int GetCurrentScore();
    }
    
    public interface IAudioService
    {
        void PlaySound(string soundName);
        void SetVolume(float volume);
    }
    
    // Example service implementations
    public class PlayerService : IPlayerService
    {
        private Vector3 _playerPosition;
          public void MovePlayer(Vector3 direction)
        {
            _playerPosition += direction;
            ConsoleLogger.LogColor($"Player moved to: {_playerPosition}", ColorLog.GREEN);
        }
        
        public Vector3 GetPlayerPosition()
        {
            return _playerPosition;
        }
    }
    
    public class ScoreService : IScoreService
    {
        private int _currentScore;
          public void AddScore(int points)
        {
            _currentScore += points;
            ConsoleLogger.LogColor($"Score updated: {_currentScore}", ColorLog.YELLOW);
        }
        
        public int GetCurrentScore()
        {
            return _currentScore;
        }
    }
    
    public class AudioService : IAudioService
    {
        private float _volume = 1.0f;
          public void PlaySound(string soundName)
        {
            ConsoleLogger.LogColor($"Playing sound: {soundName} at volume {_volume}", ColorLog.BLUE);
        }
        
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            ConsoleLogger.LogColor($"Audio volume set to: {_volume}", ColorLog.BLUE);
        }
    }
    
    /// <summary>
    /// Example demonstrating ServiceLocator usage with manual registration and resolution
    /// </summary>
    public class ServiceLocatorExample : MonoBehaviour
    {
        [Header("Manual Service Usage")]
        [SerializeField] private bool registerServicesOnStart = true;
        [SerializeField] private bool testServicesOnStart = true;
        
        private void Start()
        {
            if (registerServicesOnStart)
            {
                RegisterServices();
            }
            
            if (testServicesOnStart)
            {
                TestServices();
            }
        }
          private void RegisterServices()
        {
            ConsoleLogger.LogColor("=== Registering Services ===", ColorLog.BLUE);
            
            // Register service instances
            ServiceLocatorManager.Register<IPlayerService>(new PlayerService());
            ServiceLocatorManager.Register<IScoreService>(new ScoreService());
            
            // Register with keys
            ServiceLocatorManager.Register<IAudioService>(new AudioService(), "main");
            ServiceLocatorManager.Register<IAudioService>(new AudioService(), "music");
            
            // Register lazy services
            ServiceLocatorManager.RegisterLazy<IPlayerService>(() => 
            {
                ConsoleLogger.LogColor("Creating lazy PlayerService!", ColorLog.ORANGE);
                return new PlayerService();
            }, "lazy");
            
            ConsoleLogger.LogColor($"Total services registered: {ServiceLocatorManager.ServiceCount}", ColorLog.GREEN);
        }
          private void TestServices()
        {
            ConsoleLogger.LogColor("=== Testing Service Resolution ===", ColorLog.BLUE);
            
            // Resolve and use services
            var playerService = ServiceLocatorManager.Resolve<IPlayerService>();
            playerService.MovePlayer(Vector3.forward);
            
            var scoreService = ServiceLocatorManager.Resolve<IScoreService>();
            scoreService.AddScore(100);
            
            // Resolve keyed services
            var mainAudio = ServiceLocatorManager.Resolve<IAudioService>("main");
            mainAudio.PlaySound("jump");
            
            var musicAudio = ServiceLocatorManager.Resolve<IAudioService>("music");
            musicAudio.SetVolume(0.5f);
            musicAudio.PlaySound("background_music");
            
            // Test TryResolve
            if (ServiceLocatorManager.TryResolve<IPlayerService>("lazy", out var lazyPlayer))
            {
                lazyPlayer.MovePlayer(Vector3.up);
            }
            
            // Test service existence
            ConsoleLogger.LogColor($"Has PlayerService: {ServiceLocatorManager.IsRegistered<IPlayerService>()}", ColorLog.GREEN);
            ConsoleLogger.LogColor($"Has lazy PlayerService: {ServiceLocatorManager.IsRegistered<IPlayerService>("lazy")}", ColorLog.GREEN);
        }
          [ContextMenu("Clear All Services")]
        private void ClearServices()
        {
            ServiceLocatorManager.Clear();
            ConsoleLogger.LogColor("All services cleared!", ColorLog.RED);
        }
    }
      /// <summary>
    /// Example demonstrating automatic service injection without reflection
    /// </summary>
    public class ServiceInjectionExample : InjectableMonoBehaviour
    {
        [Header("Injected Services")]
        [SerializeField] private bool showInjectionDetails = true;
        
        // Services to be injected
        private IPlayerService _playerService;
        private IScoreService _scoreService;
        private IAudioService _mainAudio;
        private IAudioService _musicAudio;
        private IAudioService _optionalAudio;
          public override void InjectServices(IServiceLocator serviceLocator)
        {
            // Inject required services
            if (ServiceInjectionHelper.TryInject(serviceLocator, out _playerService, required: true))
            {
                if (showInjectionDetails)
                    ConsoleLogger.LogColor("[ServiceInjectionExample] Player service injected", ColorLog.GREEN);
            }
            
            if (ServiceInjectionHelper.TryInject(serviceLocator, out _scoreService, required: true))
            {
                if (showInjectionDetails)
                    ConsoleLogger.LogColor("[ServiceInjectionExample] Score service injected", ColorLog.GREEN);
            }
            
            // Inject keyed services
            if (ServiceInjectionHelper.TryInject(serviceLocator, out _mainAudio, key: "main", required: true))
            {
                if (showInjectionDetails)
                    ConsoleLogger.LogColor("[ServiceInjectionExample] Main audio service injected", ColorLog.BLUE);
            }
            
            // Inject optional services
            if (ServiceInjectionHelper.TryInject(serviceLocator, out _musicAudio, key: "music", required: false))
            {
                if (showInjectionDetails)
                    ConsoleLogger.LogColor("[ServiceInjectionExample] Music audio service injected", ColorLog.YELLOW);
            }
            
            if (ServiceInjectionHelper.TryInject(serviceLocator, out _optionalAudio, key: "nonexistent", required: false))
            {
                if (showInjectionDetails)
                    ConsoleLogger.LogColor("[ServiceInjectionExample] Optional audio service injected", ColorLog.ORANGE);
            }
        }
          protected override void Awake()
        {
            base.Awake();
            
            // Services are automatically injected here
            ConsoleLogger.LogColor("=== Service Injection Example (No Reflection) ===", ColorLog.BLUE);
        }
        
        private void Start()
        {
            TestInjectedServices();
        }
          private void TestInjectedServices()
        {
            // Use injected services
            if (_playerService != null)
            {
                _playerService.MovePlayer(Vector3.right);
                ConsoleLogger.LogColor($"Player position: {_playerService.GetPlayerPosition()}", ColorLog.GREEN);
            }
            
            if (_scoreService != null)
            {
                _scoreService.AddScore(50);
                ConsoleLogger.LogColor($"Current score: {_scoreService.GetCurrentScore()}", ColorLog.GREEN);
            }
            
            if (_mainAudio != null)
            {
                _mainAudio.PlaySound("coin_pickup");
            }
            
            if (_musicAudio != null)
            {
                _musicAudio.PlaySound("level_complete");
            }
            else
            {
                ConsoleLogger.LogWarning("Music audio service not available");
            }
            
            if (_optionalAudio == null)
            {
                ConsoleLogger.Log("Optional audio service not available (as expected)");
            }
        }
          [ContextMenu("Test Manual Injection")]
        private void TestManualInjection()
        {
            InjectServices();
            ConsoleLogger.LogColor("Manual service injection completed!", ColorLog.BLUE);
        }
    }
    
    /// <summary>
    /// Example of a service that depends on other services
    /// </summary>
    public interface IGameService
    {
        void StartGame();
        void EndGame();
    }
    
    public class GameService : IGameService
    {
        private readonly IPlayerService _playerService;
        private readonly IScoreService _scoreService;
        private readonly IAudioService _audioService;
        
        public GameService(IPlayerService playerService, IScoreService scoreService, IAudioService audioService)
        {
            _playerService = playerService;
            _scoreService = scoreService;
            _audioService = audioService;
        }
          public void StartGame()
        {
            ConsoleLogger.LogColor("Game Started!", ColorLog.GREEN);
            _audioService.PlaySound("game_start");
            _scoreService.AddScore(0); // Reset score
        }
        
        public void EndGame()
        {
            ConsoleLogger.LogColor($"Game Ended! Final Score: {_scoreService.GetCurrentScore()}", ColorLog.RED);
            _audioService.PlaySound("game_over");
        }
    }
    
    /// <summary>
    /// Example showing advanced service composition
    /// </summary>
    public class AdvancedServiceExample : MonoBehaviour
    {
        private void Start()
        {
            RegisterCompositeService();
            TestCompositeService();
        }
        
        private void RegisterCompositeService()
        {
            // Register a service that depends on other services
            ServiceLocatorManager.RegisterLazy<IGameService>(() =>
            {
                var playerService = ServiceLocatorManager.Resolve<IPlayerService>();
                var scoreService = ServiceLocatorManager.Resolve<IScoreService>();
                var audioService = ServiceLocatorManager.Resolve<IAudioService>("main");
                
                return new GameService(playerService, scoreService, audioService);
            });
        }
        
        private void TestCompositeService()
        {
            if (ServiceLocatorManager.TryResolve<IGameService>(out var gameService))
            {
                gameService.StartGame();
                
                // Simulate some gameplay
                var playerService = ServiceLocatorManager.Resolve<IPlayerService>();
                playerService.MovePlayer(Vector3.forward * 5);
                
                var scoreService = ServiceLocatorManager.Resolve<IScoreService>();
                scoreService.AddScore(1000);
                
                gameService.EndGame();
            }            else
            {
                ConsoleLogger.LogError("GameService not available. Make sure other services are registered first.");
            }
        }
    }
    
    /// <summary>
    /// Simple example showing direct service injection pattern (fastest performance)
    /// </summary>
    public class SimpleServiceUser : MonoBehaviour
    {
        [Header("Simple Service Usage")]
        [SerializeField] private bool resolveOnStart = true;
        
        private IPlayerService _playerService;
        private IScoreService _scoreService;
        
        private void Start()
        {
            if (resolveOnStart)
            {
                ResolveServices();
                TestServices();
            }
        }
        
        /// <summary>
        /// Manually resolve services - fastest approach, no reflection, no attributes
        /// </summary>        [ContextMenu("Resolve Services")]
        public void ResolveServices()
        {
            ConsoleLogger.LogColor("=== Manual Service Resolution (No Reflection) ===", ColorLog.BLUE);
            
            try
            {
                // Direct service resolution - best performance
                _playerService = ServiceLocatorManager.Resolve<IPlayerService>();
                _scoreService = ServiceLocatorManager.Resolve<IScoreService>();
                
                ConsoleLogger.LogColor("Services resolved successfully!", ColorLog.GREEN);
            }
            catch (Exception e)
            {
                ConsoleLogger.LogError($"Failed to resolve services: {e.Message}");
            }
        }
          private void TestServices()
        {
            if (_playerService != null)
            {
                _playerService.MovePlayer(Vector3.left);
                ConsoleLogger.LogColor($"Player moved to: {_playerService.GetPlayerPosition()}", ColorLog.GREEN);
            }
            
            if (_scoreService != null)
            {
                _scoreService.AddScore(25);
                ConsoleLogger.LogColor($"Score updated to: {_scoreService.GetCurrentScore()}", ColorLog.YELLOW);
            }
        }
          [ContextMenu("Test Safe Resolution")]
        public void TestSafeResolution()
        {
            ConsoleLogger.LogColor("=== Safe Service Resolution ===", ColorLog.BLUE);
            
            // Safe resolution with TryResolve
            if (ServiceLocatorManager.TryResolve<IPlayerService>(out var playerService))
            {
                playerService.MovePlayer(Vector3.up);
                ConsoleLogger.LogColor("Player service resolved safely", ColorLog.GREEN);
            }
            else
            {
                ConsoleLogger.LogWarning("Player service not available");
            }
            
            // Check service existence before resolving
            if (ServiceLocatorManager.IsRegistered<IScoreService>())
            {
                var scoreService = ServiceLocatorManager.Resolve<IScoreService>();
                scoreService.AddScore(10);
                ConsoleLogger.LogColor("Score service used safely", ColorLog.GREEN);
            }
            else
            {
                ConsoleLogger.LogWarning("Score service not registered");
            }
        }
    }
}
