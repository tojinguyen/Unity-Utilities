using System;
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// High-performance priority queue for events
    /// Supports batch processing and priority-based ordering
    /// </summary>
    public class EventQueue
    {
        #region Configuration
        
        private readonly int _initialCapacity;
        private readonly int _maxCapacity;
        private readonly bool _enableLogging;
        
        #endregion
        
        #region Fields
        
        // Priority heap for events (max-heap)
        private readonly List<BaseEvent> _events;
        private readonly Dictionary<BaseEvent, int> _eventIndices;
        
        // Batch processing
        private readonly List<BaseEvent> _batchBuffer;
        private readonly int _maxBatchSize;
        
        // Statistics
        private int _totalProcessed;
        private int _peakSize;
        private float _lastProcessTime;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Current number of events in queue
        /// </summary>
        public int Count => _events.Count;
        
        /// <summary>
        /// Whether the queue is empty
        /// </summary>
        public bool IsEmpty => _events.Count == 0;
        
        /// <summary>
        /// Peak number of events in queue
        /// </summary>
        public int PeakSize => _peakSize;
        
        /// <summary>
        /// Total events processed
        /// </summary>
        public int TotalProcessed => _totalProcessed;
        
        /// <summary>
        /// Last processing time in milliseconds
        /// </summary>
        public float LastProcessTime => _lastProcessTime;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Create a new EventQueue
        /// </summary>
        /// <param name="initialCapacity">Initial capacity of the queue</param>
        /// <param name="maxCapacity">Maximum capacity before warnings</param>
        /// <param name="maxBatchSize">Maximum events to process in one batch</param>
        /// <param name="enableLogging">Enable debug logging</param>
        public EventQueue(int initialCapacity = 256, int maxCapacity = 10000, int maxBatchSize = 1000, bool enableLogging = false)
        {
            _initialCapacity = initialCapacity;
            _maxCapacity = maxCapacity;
            _maxBatchSize = maxBatchSize;
            _enableLogging = enableLogging;
            
            _events = new List<BaseEvent>(initialCapacity);
            _eventIndices = new Dictionary<BaseEvent, int>(initialCapacity);
            _batchBuffer = new List<BaseEvent>(maxBatchSize);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Enqueue an event with priority ordering
        /// </summary>
        /// <param name="eventData">Event to enqueue</param>
        public void Enqueue(BaseEvent eventData)
        {
            if (eventData == null || eventData.IsDisposed)
            {
                Log("Attempted to enqueue null or disposed event");
                return;
            }
            
            // Check capacity
            if (_events.Count >= _maxCapacity)
            {
                Debug.LogWarning($"EventQueue has reached maximum capacity ({_maxCapacity}). Consider increasing capacity or processing events more frequently.");
                return;
            }
            
            // Add to heap and maintain priority order
            _events.Add(eventData);
            var index = _events.Count - 1;
            _eventIndices[eventData] = index;
            
            // Bubble up to maintain max-heap property
            BubbleUp(index);
            
            // Update statistics
            _peakSize = Mathf.Max(_peakSize, _events.Count);
            
            Log($"Enqueued {eventData.GetType().Name} with priority {eventData.Priority}");
        }
        
        /// <summary>
        /// Dequeue the highest priority event
        /// </summary>
        /// <returns>Highest priority event or null if queue is empty</returns>
        public BaseEvent Dequeue()
        {
            if (IsEmpty) return null;
            
            var result = _events[0];
            _eventIndices.Remove(result);
            
            // Move last element to root and bubble down
            var lastIndex = _events.Count - 1;
            if (lastIndex > 0)
            {
                _events[0] = _events[lastIndex];
                _eventIndices[_events[0]] = 0;
                BubbleDown(0);
            }
            
            _events.RemoveAt(lastIndex);
            _totalProcessed++;
            
            Log($"Dequeued {result.GetType().Name}");
            return result;
        }
        
        /// <summary>
        /// Peek at the highest priority event without removing it
        /// </summary>
        /// <returns>Highest priority event or null if queue is empty</returns>
        public BaseEvent Peek()
        {
            return IsEmpty ? null : _events[0];
        }
        
        /// <summary>
        /// Process events in batches for better performance
        /// </summary>
        /// <param name="processor">Function to process each event</param>
        /// <param name="maxEvents">Maximum events to process in this call</param>
        /// <returns>Number of events processed</returns>
        public int ProcessBatch(Action<BaseEvent> processor, int maxEvents = -1)
        {
            if (processor == null || IsEmpty) return 0;
            
            var startTime = Time.realtimeSinceStartup;
            var batchSize = maxEvents > 0 ? Mathf.Min(maxEvents, _maxBatchSize) : _maxBatchSize;
            var processed = 0;
            
            _batchBuffer.Clear();
            
            // Collect events for batch processing
            while (!IsEmpty && _batchBuffer.Count < batchSize)
            {
                var eventData = Dequeue();
                if (eventData != null && eventData.IsValid())
                {
                    _batchBuffer.Add(eventData);
                }
            }
            
            // Process the batch
            foreach (var eventData in _batchBuffer)
            {
                try
                {
                    processor(eventData);
                    processed++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing event {eventData.GetType().Name}: {ex.Message}");
                }
            }
            
            _lastProcessTime = (Time.realtimeSinceStartup - startTime) * 1000f;
            Log($"Processed batch of {processed} events in {_lastProcessTime:F2}ms");
            
            return processed;
        }
        
        /// <summary>
        /// Remove a specific event from the queue
        /// </summary>
        /// <param name="eventData">Event to remove</param>
        /// <returns>True if event was found and removed</returns>
        public bool Remove(BaseEvent eventData)
        {
            if (eventData == null || !_eventIndices.TryGetValue(eventData, out var index))
                return false;
            
            _eventIndices.Remove(eventData);
            
            var lastIndex = _events.Count - 1;
            if (index == lastIndex)
            {
                _events.RemoveAt(lastIndex);
            }
            else
            {
                // Replace with last element and rebalance
                _events[index] = _events[lastIndex];
                _eventIndices[_events[index]] = index;
                _events.RemoveAt(lastIndex);
                
                // Rebalance from this position
                var parentIndex = (index - 1) / 2;
                if (index > 0 && _events[index].Priority > _events[parentIndex].Priority)
                {
                    BubbleUp(index);
                }
                else
                {
                    BubbleDown(index);
                }
            }
            
            Log($"Removed {eventData.GetType().Name} from queue");
            return true;
        }
        
        /// <summary>
        /// Clear all events from the queue
        /// </summary>
        public void Clear()
        {
            _events.Clear();
            _eventIndices.Clear();
            _batchBuffer.Clear();
            Log("Cleared event queue");
        }
        
        /// <summary>
        /// Get queue statistics
        /// </summary>
        /// <returns>Queue statistics</returns>
        public QueueStats GetStats()
        {
            return new QueueStats
            {
                CurrentSize = Count,
                PeakSize = _peakSize,
                TotalProcessed = _totalProcessed,
                LastProcessTime = _lastProcessTime,
                Capacity = _events.Capacity
            };
        }
        
        #endregion
        
        #region Private Methods
        
        private void BubbleUp(int index)
        {
            while (index > 0)
            {
                var parentIndex = (index - 1) / 2;
                if (_events[index].Priority <= _events[parentIndex].Priority)
                    break;
                
                SwapElements(index, parentIndex);
                index = parentIndex;
            }
        }
        
        private void BubbleDown(int index)
        {
            while (true)
            {
                var largest = index;
                var leftChild = 2 * index + 1;
                var rightChild = 2 * index + 2;
                
                if (leftChild < _events.Count && _events[leftChild].Priority > _events[largest].Priority)
                    largest = leftChild;
                
                if (rightChild < _events.Count && _events[rightChild].Priority > _events[largest].Priority)
                    largest = rightChild;
                
                if (largest == index)
                    break;
                
                SwapElements(index, largest);
                index = largest;
            }
        }
        
        private void SwapElements(int i, int j)
        {
            var temp = _events[i];
            _events[i] = _events[j];
            _events[j] = temp;
            
            // Update indices
            _eventIndices[_events[i]] = i;
            _eventIndices[_events[j]] = j;
        }
        
        private void Log(string message)
        {
            if (_enableLogging)
                Debug.Log($"[EventQueue] {message}");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Statistics for event queue performance
    /// </summary>
    [Serializable]
    public struct QueueStats
    {
        /// <summary>
        /// Current number of events in queue
        /// </summary>
        public int CurrentSize;
        
        /// <summary>
        /// Peak number of events reached
        /// </summary>
        public int PeakSize;
        
        /// <summary>
        /// Total events processed
        /// </summary>
        public int TotalProcessed;
        
        /// <summary>
        /// Last batch processing time in milliseconds
        /// </summary>
        public float LastProcessTime;
        
        /// <summary>
        /// Current capacity of underlying storage
        /// </summary>
        public int Capacity;
        
        /// <summary>
        /// Average events per batch
        /// </summary>
        public float AverageEventsPerBatch => TotalProcessed > 0 ? (float)TotalProcessed / Mathf.Max(1, PeakSize) : 0f;
    }
}