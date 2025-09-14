using System;

namespace TirexGame.Utilities.Patterns
{
    /// <summary>
    /// Generic singleton pattern for non-MonoBehaviour classes.
    /// This provides a thread-safe, lazy-initialized singleton implementation.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class</typeparam>
    public abstract class Singleton<T> where T : class
    {
        private static readonly Lazy<T> _instance = new Lazy<T>(() => 
        {
            var constructor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (constructor == null || constructor.IsPrivate)
            {
                throw new Exception($"[Singleton] Type {typeof(T).Name} must have a public or protected parameterless constructor to use the Singleton pattern.");
            }
            
            return (T)Activator.CreateInstance(typeof(T));
        });

        /// <summary>
        /// Gets the singleton instance of the class.
        /// </summary>
        public static T Instance => _instance.Value;

        /// <summary>
        /// Protected constructor to prevent external instantiation.
        /// </summary>
        protected Singleton() 
        {
            Initialize();
        }

        /// <summary>
        /// Called when the singleton instance is first created.
        /// Override this method to perform initialization logic.
        /// </summary>
        protected virtual void Initialize() { }
    }
}
