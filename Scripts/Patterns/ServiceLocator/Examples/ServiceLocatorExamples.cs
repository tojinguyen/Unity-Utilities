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
            Debug.Log($"Player moved to: {_playerPosition}");
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
            Debug.Log($"Score updated: {_currentScore}");
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
            Debug.Log($"Playing sound: {soundName} at volume {_volume}");
        }
        
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            Debug.Log($"Audio volume set to: {_volume}");
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
            Debug.Log("=== Registering Services ===");
            
            // Register service instances
            ServiceLocatorManager.Register<IPlayerService>(new PlayerService());
            ServiceLocatorManager.Register<IScoreService>(new ScoreService());
            
            // Register with keys
            ServiceLocatorManager.Register<IAudioService>(new AudioService(), "main");
            ServiceLocatorManager.Register<IAudioService>(new AudioService(), "music");
            
            // Register lazy services
            ServiceLocatorManager.RegisterLazy<IPlayerService>(() => 
            {
                Debug.Log("Creating lazy PlayerService!");
                return new PlayerService();
            }, "lazy");
            
            Debug.Log($"Total services registered: {ServiceLocatorManager.ServiceCount}");
        }
        
        private void TestServices()
        {
            Debug.Log("=== Testing Service Resolution ===");
            
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
            Debug.Log($"Has PlayerService: {ServiceLocatorManager.IsRegistered<IPlayerService>()}");
            Debug.Log($"Has lazy PlayerService: {ServiceLocatorManager.IsRegistered<IPlayerService>("lazy")}");
        }
        
        [ContextMenu("Clear All Services")]
        private void ClearServices()
        {
            ServiceLocatorManager.Clear();
            Debug.Log("All services cleared!");
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
                    Debug.Log("[ServiceInjectionExample] Player service injected");
            }
            
            if (ServiceInjectionHelper.TryInject(serviceLocator, out _scoreService, required: true))
            {
                if (showInjectionDetails)
                    Debug.Log("[ServiceInjectionExample] Score service injected");
            }
            
            // Inject keyed services
            if (ServiceInjectionHelper.TryInject(serviceLocator, out _mainAudio, key: "main", required: true))
            {
                if (showInjectionDetails)
                    Debug.Log("[ServiceInjectionExample] Main audio service injected");
            }
            
            // Inject optional services
            if (ServiceInjectionHelper.TryInject(serviceLocator, out _musicAudio, key: "music", required: false))
            {
                if (showInjectionDetails)
                    Debug.Log("[ServiceInjectionExample] Music audio service injected");
            }
            
            if (ServiceInjectionHelper.TryInject(serviceLocator, out _optionalAudio, key: "nonexistent", required: false))
            {
                if (showInjectionDetails)
                    Debug.Log("[ServiceInjectionExample] Optional audio service injected");
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            // Services are automatically injected here
            Debug.Log("=== Service Injection Example (No Reflection) ===");
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
                Debug.Log($"Player position: {_playerService.GetPlayerPosition()}");
            }
            
            if (_scoreService != null)
            {
                _scoreService.AddScore(50);
                Debug.Log($"Current score: {_scoreService.GetCurrentScore()}");
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
                Debug.Log("Music audio service not available");
            }
            
            if (_optionalAudio == null)
            {
                Debug.Log("Optional audio service not available (as expected)");
            }
        }
        
        [ContextMenu("Test Manual Injection")]
        private void TestManualInjection()
        {
            InjectServices();
            Debug.Log("Manual service injection completed!");
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
            Debug.Log("Game Started!");
            _audioService.PlaySound("game_start");
            _scoreService.AddScore(0); // Reset score
        }
        
        public void EndGame()
        {
            Debug.Log($"Game Ended! Final Score: {_scoreService.GetCurrentScore()}");
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
            }
            else
            {
                Debug.LogError("GameService not available. Make sure other services are registered first.");
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
        /// </summary>
        [ContextMenu("Resolve Services")]
        public void ResolveServices()
        {
            Debug.Log("=== Manual Service Resolution (No Reflection) ===");
            
            try
            {
                // Direct service resolution - best performance
                _playerService = ServiceLocatorManager.Resolve<IPlayerService>();
                _scoreService = ServiceLocatorManager.Resolve<IScoreService>();
                
                Debug.Log("Services resolved successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to resolve services: {e.Message}");
            }
        }
        
        private void TestServices()
        {
            if (_playerService != null)
            {
                _playerService.MovePlayer(Vector3.left);
                Debug.Log($"Player moved to: {_playerService.GetPlayerPosition()}");
            }
            
            if (_scoreService != null)
            {
                _scoreService.AddScore(25);
                Debug.Log($"Score updated to: {_scoreService.GetCurrentScore()}");
            }
        }
        
        [ContextMenu("Test Safe Resolution")]
        public void TestSafeResolution()
        {
            Debug.Log("=== Safe Service Resolution ===");
            
            // Safe resolution with TryResolve
            if (ServiceLocatorManager.TryResolve<IPlayerService>(out var playerService))
            {
                playerService.MovePlayer(Vector3.up);
                Debug.Log("Player service resolved safely");
            }
            else
            {
                Debug.Log("Player service not available");
            }
            
            // Check service existence before resolving
            if (ServiceLocatorManager.IsRegistered<IScoreService>())
            {
                var scoreService = ServiceLocatorManager.Resolve<IScoreService>();
                scoreService.AddScore(10);
                Debug.Log("Score service used safely");
            }
            else
            {
                Debug.Log("Score service not registered");
            }
        }
    }
}
