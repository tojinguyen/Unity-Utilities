using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Patterns.Factory
{
    /// <summary>
    /// Interface for object factories
    /// </summary>
    /// <typeparam name="T">Type of objects this factory creates</typeparam>
    public interface IFactory<T>
    {
        /// <summary>
        /// Create an object of type T
        /// </summary>
        T Create();
        
        /// <summary>
        /// Create an object with a specific identifier
        /// </summary>
        T Create(string id);
    }
    
    /// <summary>
    /// Interface for async object factories
    /// </summary>
    /// <typeparam name="T">Type of objects this factory creates</typeparam>
    public interface IAsyncFactory<T>
    {
        /// <summary>
        /// Asynchronously create an object of type T
        /// </summary>
        UniTask<T> CreateAsync();
        
        /// <summary>
        /// Asynchronously create an object with a specific identifier
        /// </summary>
        UniTask<T> CreateAsync(string id);
    }
    
    /// <summary>
    /// Interface for factories that can also destroy objects
    /// </summary>
    /// <typeparam name="T">Type of objects this factory manages</typeparam>
    public interface IManagedFactory<T> : IFactory<T>
    {
        /// <summary>
        /// Destroy an object created by this factory
        /// </summary>
        void Destroy(T obj);
        
        /// <summary>
        /// Get the number of active objects
        /// </summary>
        int ActiveCount { get; }
    }
}
