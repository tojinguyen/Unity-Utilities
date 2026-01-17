using UnityEngine;
using ServiceLocatorImpl = TirexGame.Utils.Patterns.ServiceLocator.ServiceLocator;

namespace TirexGame.Utils.Patterns.ServiceLocator
{
    /// <summary>
    /// MonoBehaviour-based Service Locator for Unity integration.
    /// Provides a global access point for service resolution with Unity lifecycle support.
    /// This implementation follows the Singleton pattern and integrates with Unity's component system.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ServiceLocatorManager : MonoSingleton<ServiceLocatorManager>
    {
        [Header("Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        
        private IServiceLocator _serviceLocator;
        
        /// <summary>
        /// Access to the underlying service locator
        /// </summary>
        public static IServiceLocator Services => Instance._serviceLocator;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            _serviceLocator = new ServiceLocatorImpl(enableLogging);
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            Log("ServiceLocatorManager initialized");
        }
        
        /// <summary>
        /// Register a service instance
        /// </summary>
        public static void Register<T>(T instance)
        {
            Services.Register(instance);
        }
        
        /// <summary>
        /// Register a service instance with a key
        /// </summary>
        public static void Register<T>(T instance, string key)
        {
            Services.Register(instance, key);
        }
        
        /// <summary>
        /// Register a lazy service factory
        /// </summary>
        public static void RegisterLazy<T>(System.Func<T> factory)
        {
            Services.RegisterLazy(factory);
        }
        
        /// <summary>
        /// Register a lazy service factory with a key
        /// </summary>
        public static void RegisterLazy<T>(System.Func<T> factory, string key)
        {
            Services.RegisterLazy(factory, key);
        }
        
        /// <summary>
        /// Resolve a service instance
        /// </summary>
        public static T Resolve<T>()
        {
            return Services.Resolve<T>();
        }
        
        /// <summary>
        /// Resolve a service instance with a key
        /// </summary>
        public static T Resolve<T>(string key)
        {
            return Services.Resolve<T>(key);
        }
        
        /// <summary>
        /// Try to resolve a service instance
        /// </summary>
        public static bool TryResolve<T>(out T service)
        {
            return Services.TryResolve(out service);
        }
        
        /// <summary>
        /// Try to resolve a service instance with a key
        /// </summary>
        public static bool TryResolve<T>(string key, out T service)
        {
            return Services.TryResolve(key, out service);
        }
        
        /// <summary>
        /// Check if a service is registered
        /// </summary>
        public static bool IsRegistered<T>()
        {
            return Services.IsRegistered<T>();
        }
        
        /// <summary>
        /// Check if a service is registered with a key
        /// </summary>
        public static bool IsRegistered<T>(string key)
        {
            return Services.IsRegistered<T>(key);
        }
        
        /// <summary>
        /// Unregister a service
        /// </summary>
        public static void Unregister<T>()
        {
            Services.Unregister<T>();
        }
        
        /// <summary>
        /// Unregister a service with a key
        /// </summary>
        public static void Unregister<T>(string key)
        {
            Services.Unregister<T>(key);
        }
        
        /// <summary>
        /// Clear all registered services
        /// </summary>
        public static void Clear()
        {
            Services.Clear();
        }
        
        /// <summary>
        /// Get the number of registered services
        /// </summary>
        public static int ServiceCount => Services.ServiceCount;
        
        protected override void OnDestroy()
        {
            if (_serviceLocator != null)
            {
                _serviceLocator.Clear();
                Log("ServiceLocatorManager destroyed and services cleared");
            }
            
            base.OnDestroy();
        }
          private void Log(string message)
        {
            if (enableLogging)
            {
                ConsoleLogger.LogColor($"[ServiceLocatorManager] {message}", ColorLog.BLUE);
            }
        }
          #region Unity Editor Support
        
#if UNITY_EDITOR
        [ContextMenu("Log Service Count")]
        private void LogServiceCount()
        {
            if (_serviceLocator != null)
            {
                ConsoleLogger.LogColor($"[ServiceLocatorManager] Total registered services: {_serviceLocator.ServiceCount}", ColorLog.YELLOW);
            }
            else
            {
                ConsoleLogger.LogWarning("[ServiceLocatorManager] Service locator not initialized");
            }
        }
          [ContextMenu("Clear All Services")]
        private void ClearAllServices()
        {
            if (_serviceLocator != null)
            {
                _serviceLocator.Clear();
                ConsoleLogger.LogColor("[ServiceLocatorManager] All services cleared via context menu", ColorLog.RED);
            }
        }
#endif
        
        #endregion
    }
}
