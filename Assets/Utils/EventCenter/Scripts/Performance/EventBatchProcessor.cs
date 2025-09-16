using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// High-performance batch processor for events
    /// Optimizes processing of large numbers of similar events
    /// </summary>
    public class EventBatchProcessor
    {
        #region Configuration
        
        private readonly int _maxBatchSize;
        private readonly float _maxBatchTime;
        private readonly bool _enableLogging;
        
        #endregion
        
        #region Fields
        
        // Batches organized by event type
        private readonly Dictionary<Type, EventBatch> _batches = new Dictionary<Type, EventBatch>();
        
        // Processing statistics
        private int _totalBatchesProcessed;
        private int _totalEventsProcessed;
        private float _lastProcessTime;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Total number of batches processed
        /// </summary>
        public int TotalBatchesProcessed => _totalBatchesProcessed;
        
        /// <summary>
        /// Total number of events processed
        /// </summary>
        public int TotalEventsProcessed => _totalEventsProcessed;
        
        /// <summary>
        /// Last processing time in milliseconds
        /// </summary>
        public float LastProcessTime => _lastProcessTime;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Create a new EventBatchProcessor
        /// </summary>
        /// <param name="maxBatchSize">Maximum events per batch</param>
        /// <param name="maxBatchTime">Maximum time to wait before processing batch (seconds)</param>
        /// <param name="enableLogging">Enable debug logging</param>
        public EventBatchProcessor(int maxBatchSize = 100, float maxBatchTime = 0.016f, bool enableLogging = false)
        {
            _maxBatchSize = maxBatchSize;
            _maxBatchTime = maxBatchTime;
            _enableLogging = enableLogging;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Add an event to a batch for processing
        /// </summary>
        /// <param name="eventData">Event to add to batch</param>
        /// <param name="processor">Processor function for this event type</param>
        public void AddToBatch<T>(T eventData, Action<List<T>> processor) where T : BaseEvent
        {
            if (eventData == null || processor == null) return;
            
            var eventType = typeof(T);
            if (!_batches.TryGetValue(eventType, out var batch))
            {
                batch = new EventBatch<T>(_maxBatchSize, _maxBatchTime, processor);
                _batches[eventType] = batch;
            }
            
            if (batch is EventBatch<T> typedBatch)
            {
                typedBatch.AddEvent(eventData);
                Log($"Added {eventType.Name} to batch, current size: {typedBatch.Count}");
            }
        }
        
        /// <summary>
        /// Process all ready batches
        /// </summary>
        /// <returns>Number of batches processed</returns>
        public int ProcessReadyBatches()
        {
            var startTime = Time.realtimeSinceStartup;
            var batchesProcessed = 0;
            var eventsProcessed = 0;
            
            var batchesToRemove = new List<Type>();
            
            foreach (var kvp in _batches)
            {
                var batch = kvp.Value;
                if (batch.IsReadyForProcessing())
                {
                    try
                    {
                        var batchSize = batch.Count;
                        batch.Process();
                        
                        batchesProcessed++;
                        eventsProcessed += batchSize;
                        
                        Log($"Processed batch of {batchSize} {kvp.Key.Name} events");
                        
                        // Remove empty batches
                        if (batch.Count == 0)
                        {
                            batchesToRemove.Add(kvp.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing batch for {kvp.Key.Name}: {ex.Message}");
                    }
                }
            }
            
            // Clean up empty batches
            foreach (var type in batchesToRemove)
            {
                _batches.Remove(type);
            }
            
            _totalBatchesProcessed += batchesProcessed;
            _totalEventsProcessed += eventsProcessed;
            _lastProcessTime = (Time.realtimeSinceStartup - startTime) * 1000f;
            
            if (batchesProcessed > 0)
            {
                Log($"Processed {batchesProcessed} batches with {eventsProcessed} events in {_lastProcessTime:F2}ms");
            }
            
            return batchesProcessed;
        }
        
        /// <summary>
        /// Force process all batches regardless of readiness
        /// </summary>
        /// <returns>Number of batches processed</returns>
        public int ProcessAllBatches()
        {
            var batchesProcessed = 0;
            var eventsProcessed = 0;
            
            foreach (var kvp in _batches.ToArray())
            {
                var batch = kvp.Value;
                if (batch.Count > 0)
                {
                    try
                    {
                        var batchSize = batch.Count;
                        batch.Process();
                        
                        batchesProcessed++;
                        eventsProcessed += batchSize;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error force processing batch for {kvp.Key.Name}: {ex.Message}");
                    }
                }
            }
            
            _batches.Clear();
            _totalBatchesProcessed += batchesProcessed;
            _totalEventsProcessed += eventsProcessed;
            
            Log($"Force processed {batchesProcessed} batches with {eventsProcessed} events");
            return batchesProcessed;
        }
        
        /// <summary>
        /// Get statistics for all batches
        /// </summary>
        /// <returns>Dictionary of batch statistics by type</returns>
        public Dictionary<Type, BatchStats> GetBatchStats()
        {
            var stats = new Dictionary<Type, BatchStats>();
            
            foreach (var kvp in _batches)
            {
                stats[kvp.Key] = kvp.Value.GetStats();
            }
            
            return stats;
        }
        
        /// <summary>
        /// Clear all batches
        /// </summary>
        public void Clear()
        {
            _batches.Clear();
            Log("Cleared all event batches");
        }
        
        #endregion
        
        #region Private Methods
        
        private void Log(string message)
        {
            if (_enableLogging)
                Debug.Log($"[EventBatchProcessor] {message}");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Base class for event batches
    /// </summary>
    public abstract class EventBatch
    {
        protected readonly int _maxBatchSize;
        protected readonly float _maxBatchTime;
        protected float _firstEventTime;
        
        public abstract int Count { get; }
        
        protected EventBatch(int maxBatchSize, float maxBatchTime)
        {
            _maxBatchSize = maxBatchSize;
            _maxBatchTime = maxBatchTime;
            _firstEventTime = Time.unscaledTime;
        }
        
        public virtual bool IsReadyForProcessing()
        {
            return Count >= _maxBatchSize || 
                   (Count > 0 && Time.unscaledTime - _firstEventTime >= _maxBatchTime);
        }
        
        public abstract void Process();
        public abstract BatchStats GetStats();
    }
    
    /// <summary>
    /// Typed event batch implementation
    /// </summary>
    public class EventBatch<T> : EventBatch where T : BaseEvent
    {
        #region Fields
        
        private readonly List<T> _events;
        private readonly Action<List<T>> _processor;
        
        // Statistics
        private int _totalProcessed;
        private int _batchesProcessed;
        
        #endregion
        
        #region Properties
        
        public override int Count => _events.Count;
        
        #endregion
        
        #region Constructor
        
        public EventBatch(int maxBatchSize, float maxBatchTime, Action<List<T>> processor) 
            : base(maxBatchSize, maxBatchTime)
        {
            _events = new List<T>(maxBatchSize);
            _processor = processor;
        }
        
        #endregion
        
        #region Public Methods
        
        public void AddEvent(T eventData)
        {
            if (eventData == null) return;
            
            if (_events.Count == 0)
            {
                _firstEventTime = Time.unscaledTime;
            }
            
            _events.Add(eventData);
        }
        
        public override void Process()
        {
            if (_events.Count == 0 || _processor == null) return;
            
            try
            {
                _processor(_events);
                _totalProcessed += _events.Count;
                _batchesProcessed++;
            }
            finally
            {
                // Dispose poolable events
                foreach (var eventData in _events)
                {
                    if (eventData.IsPoolable)
                    {
                        eventData.Dispose();
                    }
                }
                
                _events.Clear();
            }
        }
        
        public override BatchStats GetStats()
        {
            return new BatchStats
            {
                CurrentBatchSize = Count,
                TotalEventsProcessed = _totalProcessed,
                TotalBatchesProcessed = _batchesProcessed,
                TimeSinceFirstEvent = Time.unscaledTime - _firstEventTime,
                AverageEventsPerBatch = _batchesProcessed > 0 ? (float)_totalProcessed / _batchesProcessed : 0f
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Statistics for event batch performance
    /// </summary>
    [Serializable]
    public struct BatchStats
    {
        /// <summary>
        /// Current number of events in batch
        /// </summary>
        public int CurrentBatchSize;
        
        /// <summary>
        /// Total events processed in all batches
        /// </summary>
        public int TotalEventsProcessed;
        
        /// <summary>
        /// Total number of batches processed
        /// </summary>
        public int TotalBatchesProcessed;
        
        /// <summary>
        /// Time since first event was added to current batch
        /// </summary>
        public float TimeSinceFirstEvent;
        
        /// <summary>
        /// Average number of events per batch
        /// </summary>
        public float AverageEventsPerBatch;
    }
}