using System;
using UnityEngine;

namespace TirexGame.Utils.Patterns.ServiceLocator
{
    /// <summary>
    /// Attribute to mark fields for automatic service injection.
    /// Use this with ServiceInjector to automatically resolve dependencies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class InjectServiceAttribute : Attribute
    {
        /// <summary>
        /// Optional service key for keyed service resolution
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// Whether the injection is optional (won't throw if service not found)
        /// </summary>
        public bool Optional { get; }
        
        public InjectServiceAttribute(string key = null, bool optional = false)
        {
            Key = key;
            Optional = optional;
        }
    }
      /// <summary>
    /// Interface for objects that can receive dependency injection.
    /// Implement this interface to get automatic service injection without reflection.
    /// </summary>
    public interface IServiceInjectable
    {
        /// <summary>
        /// Called to inject services into this object
        /// </summary>
        /// <param name="serviceLocator">Service locator to resolve services from</param>
        void InjectServices(IServiceLocator serviceLocator);
    }
    
    /// <summary>
    /// Utility class for automatic service injection without reflection.
    /// Objects must implement IServiceInjectable interface for injection.
    /// </summary>
    public static class ServiceInjector
    {
        /// <summary>
        /// Inject services into the target object if it implements IServiceInjectable
        /// </summary>
        /// <param name="target">Target object to inject services into</param>
        /// <param name="serviceLocator">Service locator to resolve services from (optional, uses global if null)</param>
        public static void Inject(object target, IServiceLocator serviceLocator = null)
        {
            if (target == null)
            {
                ConsoleLogger.LogError("[ServiceInjector] Cannot inject into null target");
                return;
            }
            
            var locator = serviceLocator ?? ServiceLocatorManager.Services;
            if (locator == null)
            {
                ConsoleLogger.LogError("[ServiceInjector] No service locator available for injection");
                return;
            }

            // Check if target implements IServiceInjectable
            if (target is IServiceInjectable injectable)
            {
                try
                {
                    injectable.InjectServices(locator);
                    ConsoleLogger.LogColor($"[ServiceInjector] Injected services into {target.GetType().Name}", ColorLog.GREEN);
                }
                catch (Exception e)
                {
                    ConsoleLogger.LogError($"[ServiceInjector] Failed to inject services into {target.GetType().Name}: {e.Message}");
                }
            }
            else
            {
                ConsoleLogger.LogWarning($"[ServiceInjector] {target.GetType().Name} does not implement IServiceInjectable interface");
            }
        }
          /// <summary>
        /// Inject services into all components in a GameObject hierarchy that implement IServiceInjectable
        /// </summary>
        /// <param name="gameObject">Root GameObject to scan</param>
        /// <param name="includeInactive">Whether to include inactive GameObjects</param>
        /// <param name="serviceLocator">Service locator to resolve services from (optional, uses global if null)</param>
        public static void InjectInHierarchy(GameObject gameObject, bool includeInactive = false, IServiceLocator serviceLocator = null)
        {
            if (gameObject == null)
            {
                ConsoleLogger.LogError("[ServiceInjector] Cannot inject into null GameObject");
                return;
            }
            
            var components = gameObject.GetComponentsInChildren<Component>(includeInactive);
            
            foreach (var component in components)
            {
                if (component != null && component is IServiceInjectable)
                {
                    Inject(component, serviceLocator);
                }
            }
        }
    }
    
    /// <summary>
    /// Base MonoBehaviour class that automatically injects services on Awake.
    /// Inherit from this class and implement InjectServices method to get automatic dependency injection.
    /// </summary>
    public abstract class InjectableMonoBehaviour : MonoBehaviour, IServiceInjectable
    {
        [Header("Service Injection")]
        [SerializeField] private bool autoInjectOnAwake = true;
        [SerializeField] private bool logInjection = false;
        
        protected virtual void Awake()
        {
            if (autoInjectOnAwake)
            {
                InjectServices();
            }
        }
        
        /// <summary>
        /// Manually trigger service injection
        /// </summary>
        protected void InjectServices()
        {
            if (logInjection)
            {
                ConsoleLogger.LogColor($"[{GetType().Name}] Injecting services...", ColorLog.BLUE);
            }
            
            ServiceInjector.Inject(this);
        }
        
        /// <summary>
        /// Override this method to define which services to inject
        /// </summary>
        /// <param name="serviceLocator">Service locator to resolve services from</param>
        public abstract void InjectServices(IServiceLocator serviceLocator);
    }
    
    /// <summary>
    /// Helper class to make service injection easier and more performant.
    /// Use this to resolve services safely without reflection.
    /// </summary>
    public static class ServiceInjectionHelper
    {
        /// <summary>
        /// Safely resolve a service with error handling
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceLocator">Service locator</param>
        /// <param name="service">Output resolved service</param>
        /// <param name="key">Optional service key</param>
        /// <param name="required">Whether service is required (logs error if not found)</param>
        /// <returns>True if service was resolved successfully</returns>
        public static bool TryInject<T>(IServiceLocator serviceLocator, out T service, string key = null, bool required = true) where T : class
        {
            service = null;
            
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    if (serviceLocator.TryResolve<T>(out service))
                    {
                        return true;
                    }
                }
                else
                {
                    if (serviceLocator.TryResolve<T>(key, out service))
                    {
                        return true;
                    }
                }
                
                if (required)
                {
                    var serviceType = typeof(T).Name;
                    var keyInfo = string.IsNullOrEmpty(key) ? "" : $" with key '{key}'";
                    ConsoleLogger.LogError($"[ServiceInjection] Required service {serviceType}{keyInfo} not found");
                }
                
                return false;
            }
            catch (Exception e)
            {
                if (required)
                {
                    var serviceType = typeof(T).Name;
                    var keyInfo = string.IsNullOrEmpty(key) ? "" : $" with key '{key}'";
                    ConsoleLogger.LogError($"[ServiceInjection] Failed to resolve service {serviceType}{keyInfo}: {e.Message}");
                }
                
                return false;
            }
        }
        
        /// <summary>
        /// Resolve a service directly (throws if not found)
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="serviceLocator">Service locator</param>
        /// <param name="key">Optional service key</param>
        /// <returns>Resolved service</returns>
        public static T Inject<T>(IServiceLocator serviceLocator, string key = null) where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                return serviceLocator.Resolve<T>();
            }
            else
            {
                return serviceLocator.Resolve<T>(key);
            }
        }
    }
}
