using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Setup component for EventCenter system — ensures proper initialization.
    /// Add this to a persistent GameObject in your first scene.
    /// </summary>
    public class EventCenterSetup : MonoBehaviour
    {
        [Header("EventCenter Configuration")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool enableLogging = false;

        private EventCenter _eventCenter;

        private void Awake()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initialize the EventCenter system.
        /// </summary>
        public void Initialize()
        {
            if (_eventCenter == null)
            {
                _eventCenter = GetComponent<EventCenter>() ?? gameObject.AddComponent<EventCenter>();
            }

            EventCenterService.SetCurrent(_eventCenter);

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(EventCenterService.Current, _eventCenter))
            {
                EventCenterService.ClearCurrent();
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Get the EventCenter instance managed by this setup.</summary>
        public EventCenter GetEventCenter() => _eventCenter;

        /// <summary>Check if the system is properly initialized.</summary>
        public bool IsInitialized() =>
            _eventCenter != null && ReferenceEquals(EventCenterService.Current, _eventCenter);
    }
}