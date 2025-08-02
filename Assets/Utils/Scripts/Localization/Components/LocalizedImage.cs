using UnityEngine;
using UnityEngine.UI;

namespace Tirex.Game.Utils.Localization.Components
{
    /// <summary>
    /// Component that automatically updates image/sprite based on current language
    /// Useful for localized UI graphics, flags, etc.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class LocalizedImage : MonoBehaviour
    {
        [Header("Localization Settings")]
        [SerializeField] private string localizationKey;
        [SerializeField] private bool updateOnLanguageChange = true;
        [SerializeField] private bool preserveAspectRatio = true;
        [SerializeField] private bool useAutoUpdater = true;

        [Header("Fallback")]
        [SerializeField] private Sprite fallbackSprite;

        // Component reference
        private Image imageComponent;

        // Properties
        public string LocalizationKey
        {
            get => localizationKey;
            set
            {
                localizationKey = value;
                UpdateSprite();
            }
        }

        public Sprite FallbackSprite
        {
            get => fallbackSprite;
            set => fallbackSprite = value;
        }

        private void Awake()
        {
            imageComponent = GetComponent<Image>();
            if (imageComponent == null)
            {
                Debug.LogError($"LocalizedImage on {gameObject.name}: Image component is required!", this);
            }
        }

        private void Start()
        {
            UpdateSprite();
        }

        private void OnEnable()
        {
            // Choose between manual event handling or auto-updater
            if (useAutoUpdater)
            {
                LocalizationAutoUpdater.RegisterLocalizedImage(this);
            }
            else if (updateOnLanguageChange)
            {
                LocalizationManager.OnLanguageChanged += OnLanguageChanged;
            }
            
            // Auto-update when enabled if manager is ready
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                UpdateSprite();
            }
        }

        private void OnDisable()
        {
            // Unregister from appropriate system
            if (useAutoUpdater)
            {
                LocalizationAutoUpdater.UnregisterLocalizedImage(this);
            }
            else if (updateOnLanguageChange)
            {
                LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        /// <summary>
        /// Update the sprite based on current language
        /// </summary>
        public void UpdateSprite()
        {
            if (imageComponent == null || string.IsNullOrEmpty(localizationKey))
                return;

            if (!LocalizationManager.Instance.IsInitialized)
            {
                Debug.LogWarning("LocalizationManager not initialized yet!");
                return;
            }

            Sprite localizedSprite = LocalizationManager.Instance.GetSprite(localizationKey);

            // Use fallback sprite if localized sprite not found
            if (localizedSprite == null && fallbackSprite != null)
            {
                localizedSprite = fallbackSprite;
            }

            if (localizedSprite != null)
            {
                imageComponent.sprite = localizedSprite;
                
                // Preserve aspect ratio if enabled
                if (preserveAspectRatio)
                {
                    imageComponent.preserveAspect = true;
                }
            }
            else
            {
                Debug.LogWarning($"No sprite found for key '{localizationKey}' in current language!", this);
            }
        }

        /// <summary>
        /// Set localization key and update sprite immediately
        /// </summary>
        public void SetLocalizationKey(string key)
        {
            localizationKey = key;
            UpdateSprite();
        }

        /// <summary>
        /// Set fallback sprite
        /// </summary>
        public void SetFallbackSprite(Sprite sprite)
        {
            fallbackSprite = sprite;
        }

        /// <summary>
        /// Called when language changes
        /// </summary>
        private void OnLanguageChanged(LanguageCode newLanguage)
        {
            UpdateSprite();
        }

        /// <summary>
        /// Get current sprite from the image component
        /// </summary>
        public Sprite GetCurrentSprite()
        {
            return imageComponent?.sprite;
        }

        /// <summary>
        /// Validate the component setup
        /// </summary>
        public bool IsValid()
        {
            return imageComponent != null && !string.IsNullOrEmpty(localizationKey);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Update sprite in edit mode for preview
        /// </summary>
        private void OnValidate()
        {
            if (imageComponent == null)
                imageComponent = GetComponent<Image>();
        }

        /// <summary>
        /// Editor-only: Force update sprite
        /// </summary>
        [ContextMenu("Update Sprite")]
        public void EditorUpdateSprite()
        {
            UpdateSprite();
        }
#endif
    }
}
