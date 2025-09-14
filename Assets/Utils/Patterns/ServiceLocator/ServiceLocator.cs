using System;
using System.Collections.Generic;

namespace TirexGame.Utils.Patterns.ServiceLocator
{
    /// <summary>
    /// Exception thrown when a service is not found in the service locator
    /// </summary>
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException(Type serviceType) 
            : base($"Service of type {serviceType.Name} is not registered")
        {
        }
        
        public ServiceNotFoundException(Type serviceType, string key) 
            : base($"Service of type {serviceType.Name} with key '{key}' is not registered")
        {
        }
    }
    
    /// <summary>
    /// Exception thrown when a service is already registered
    /// </summary>
    public class ServiceAlreadyRegisteredException : Exception
    {
        public ServiceAlreadyRegisteredException(Type serviceType) 
            : base($"Service of type {serviceType.Name} is already registered")
        {
        }
        
        public ServiceAlreadyRegisteredException(Type serviceType, string key) 
            : base($"Service of type {serviceType.Name} with key '{key}' is already registered")
        {
        }
    }
    
    /// <summary>
    /// Standard implementation of the Service Locator pattern.
    /// Provides dependency injection and service resolution capabilities.
    /// </summary>
    public class ServiceLocator : IServiceLocator
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<string, object> _keyedServices = new();
        private readonly Dictionary<Type, Func<object>> _lazyServices = new();
        private readonly Dictionary<string, Func<object>> _keyedLazyServices = new();
        private readonly bool _enableLogging;
        
        public ServiceLocator(bool enableLogging = false)
        {
            _enableLogging = enableLogging;
            Log("ServiceLocator initialized");
        }
        
        public int ServiceCount => _services.Count + _keyedServices.Count + _lazyServices.Count + _keyedLazyServices.Count;
        
        public void Register<T>(T instance)
        {
            if (instance == null)
            {
                LogError($"Cannot register null instance for type {typeof(T).Name}");
                return;
            }
            
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                throw new ServiceAlreadyRegisteredException(type);
            }
            
            _services[type] = instance;
            Log($"Registered service: {type.Name}");
        }
        
        public void Register<T>(T instance, string key)
        {
            if (instance == null)
            {
                LogError($"Cannot register null instance for type {typeof(T).Name} with key '{key}'");
                return;
            }
            
            if (string.IsNullOrEmpty(key))
            {
                LogError("Cannot register service with null or empty key");
                return;
            }
            
            var serviceKey = GetServiceKey<T>(key);
            
            if (_keyedServices.ContainsKey(serviceKey))
            {
                throw new ServiceAlreadyRegisteredException(typeof(T), key);
            }
            
            _keyedServices[serviceKey] = instance;
            Log($"Registered keyed service: {typeof(T).Name} with key '{key}'");
        }
        
        public void RegisterLazy<T>(Func<T> factory)
        {
            if (factory == null)
            {
                LogError($"Cannot register null factory for type {typeof(T).Name}");
                return;
            }
            
            var type = typeof(T);
            
            if (_lazyServices.ContainsKey(type))
            {
                throw new ServiceAlreadyRegisteredException(type);
            }
            
            _lazyServices[type] = () => factory();
            Log($"Registered lazy service: {type.Name}");
        }
        
        public void RegisterLazy<T>(Func<T> factory, string key)
        {
            if (factory == null)
            {
                LogError($"Cannot register null factory for type {typeof(T).Name} with key '{key}'");
                return;
            }
            
            if (string.IsNullOrEmpty(key))
            {
                LogError("Cannot register lazy service with null or empty key");
                return;
            }
            
            var serviceKey = GetServiceKey<T>(key);
            
            if (_keyedLazyServices.ContainsKey(serviceKey))
            {
                throw new ServiceAlreadyRegisteredException(typeof(T), key);
            }
            
            _keyedLazyServices[serviceKey] = () => factory();
            Log($"Registered keyed lazy service: {typeof(T).Name} with key '{key}'");
        }
        
        public T Resolve<T>()
        {
            var type = typeof(T);
            
            // Try direct service first
            if (_services.TryGetValue(type, out var service))
            {
                Log($"Resolved service: {type.Name}");
                return (T)service;
            }
            
            // Try lazy service
            if (_lazyServices.TryGetValue(type, out var factory))
            {
                var instance = (T)factory();
                Log($"Resolved lazy service: {type.Name}");
                
                // Cache the instance for future use
                _services[type] = instance;
                _lazyServices.Remove(type);
                
                return instance;
            }
            
            throw new ServiceNotFoundException(type);
        }
        
        public T Resolve<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                LogError("Cannot resolve service with null or empty key");
                return default(T);
            }
            
            var serviceKey = GetServiceKey<T>(key);
            
            // Try direct keyed service first
            if (_keyedServices.TryGetValue(serviceKey, out var service))
            {
                Log($"Resolved keyed service: {typeof(T).Name} with key '{key}'");
                return (T)service;
            }
            
            // Try lazy keyed service
            if (_keyedLazyServices.TryGetValue(serviceKey, out var factory))
            {
                var instance = (T)factory();
                Log($"Resolved keyed lazy service: {typeof(T).Name} with key '{key}'");
                
                // Cache the instance for future use
                _keyedServices[serviceKey] = instance;
                _keyedLazyServices.Remove(serviceKey);
                
                return instance;
            }
            
            throw new ServiceNotFoundException(typeof(T), key);
        }
        
        public bool TryResolve<T>(out T service)
        {
            try
            {
                service = Resolve<T>();
                return true;
            }
            catch (ServiceNotFoundException)
            {
                service = default(T);
                return false;
            }
        }
        
        public bool TryResolve<T>(string key, out T service)
        {
            try
            {
                service = Resolve<T>(key);
                return true;
            }
            catch (ServiceNotFoundException)
            {
                service = default(T);
                return false;
            }
        }
        
        public bool IsRegistered<T>()
        {
            var type = typeof(T);
            return _services.ContainsKey(type) || _lazyServices.ContainsKey(type);
        }
        
        public bool IsRegistered<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
                
            var serviceKey = GetServiceKey<T>(key);
            return _keyedServices.ContainsKey(serviceKey) || _keyedLazyServices.ContainsKey(serviceKey);
        }
        
        public void Unregister<T>()
        {
            var type = typeof(T);
            var removed = false;
            
            if (_services.Remove(type))
            {
                removed = true;
                Log($"Unregistered service: {type.Name}");
            }
            
            if (_lazyServices.Remove(type))
            {
                removed = true;
                Log($"Unregistered lazy service: {type.Name}");
            }
            
            if (!removed)
            {
                LogWarning($"Service {type.Name} was not registered");
            }
        }
        
        public void Unregister<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                LogError("Cannot unregister service with null or empty key");
                return;
            }
            
            var serviceKey = GetServiceKey<T>(key);
            var removed = false;
            
            if (_keyedServices.Remove(serviceKey))
            {
                removed = true;
                Log($"Unregistered keyed service: {typeof(T).Name} with key '{key}'");
            }
            
            if (_keyedLazyServices.Remove(serviceKey))
            {
                removed = true;
                Log($"Unregistered keyed lazy service: {typeof(T).Name} with key '{key}'");
            }
            
            if (!removed)
            {
                LogWarning($"Keyed service {typeof(T).Name} with key '{key}' was not registered");
            }
        }
        
        public void Clear()
        {
            var totalCount = ServiceCount;
            
            _services.Clear();
            _keyedServices.Clear();
            _lazyServices.Clear();
            _keyedLazyServices.Clear();
            
            Log($"Cleared all services ({totalCount} services removed)");
        }
        
        private string GetServiceKey<T>(string key)
        {
            return $"{typeof(T).FullName}_{key}";
        }
          private void Log(string message)
        {
            if (_enableLogging)
            {
                ConsoleLogger.LogColor($"[ServiceLocator] {message}", ColorLog.BLUE);
            }
        }
        
        private void LogWarning(string message)
        {
            if (_enableLogging)
            {
                ConsoleLogger.LogWarning($"[ServiceLocator] {message}");
            }
        }
        
        private void LogError(string message)
        {
            ConsoleLogger.LogError($"[ServiceLocator] {message}");
        }
    }
}
