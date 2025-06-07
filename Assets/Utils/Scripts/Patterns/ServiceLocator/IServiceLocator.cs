using System;

namespace TirexGame.Utils.Patterns.ServiceLocator
{
    /// <summary>
    /// Interface for the Service Locator pattern.
    /// Provides a way to register and resolve services without tight coupling.
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// Register a service instance with the locator
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="instance">Service instance</param>
        void Register<T>(T instance);
        
        /// <summary>
        /// Register a service instance with a specific key
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="instance">Service instance</param>
        /// <param name="key">Service key</param>
        void Register<T>(T instance, string key);
        
        /// <summary>
        /// Register a lazy service factory
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="factory">Factory function to create the service</param>
        void RegisterLazy<T>(Func<T> factory);
        
        /// <summary>
        /// Register a lazy service factory with a specific key
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="factory">Factory function to create the service</param>
        /// <param name="key">Service key</param>
        void RegisterLazy<T>(Func<T> factory, string key);
        
        /// <summary>
        /// Resolve a service instance
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <returns>Service instance</returns>
        T Resolve<T>();
        
        /// <summary>
        /// Resolve a service instance with a specific key
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="key">Service key</param>
        /// <returns>Service instance</returns>
        T Resolve<T>(string key);
        
        /// <summary>
        /// Try to resolve a service instance
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="service">Resolved service instance</param>
        /// <returns>True if service was resolved successfully</returns>
        bool TryResolve<T>(out T service);
        
        /// <summary>
        /// Try to resolve a service instance with a specific key
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="key">Service key</param>
        /// <param name="service">Resolved service instance</param>
        /// <returns>True if service was resolved successfully</returns>
        bool TryResolve<T>(string key, out T service);
        
        /// <summary>
        /// Check if a service is registered
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <returns>True if service is registered</returns>
        bool IsRegistered<T>();
        
        /// <summary>
        /// Check if a service is registered with a specific key
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="key">Service key</param>
        /// <returns>True if service is registered</returns>
        bool IsRegistered<T>(string key);
        
        /// <summary>
        /// Unregister a service
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        void Unregister<T>();
        
        /// <summary>
        /// Unregister a service with a specific key
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="key">Service key</param>
        void Unregister<T>(string key);
        
        /// <summary>
        /// Clear all registered services
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Get the number of registered services
        /// </summary>
        int ServiceCount { get; }
    }
}
