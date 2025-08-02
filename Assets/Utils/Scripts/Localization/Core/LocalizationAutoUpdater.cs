using System.Collections.Generic;
using UnityEngine;
using Tirex.Game.Utils.Localization.Components;

namespace Tirex.Game.Utils.Localization
{
    /// <summary>
    /// Auto-update manager that automatically handles all localized components
    /// No need to manually register/unregister events for each component
    /// </summary>
    public class LocalizationAutoUpdater : MonoBehaviour
    {
        private static LocalizationAutoUpdater instance;
        public static LocalizationAutoUpdater Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("LocalizationAutoUpdater");
                    instance = go.AddComponent<LocalizationAutoUpdater>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        // Track all active components
        private HashSet<LocalizedText> activeLocalizedTexts = new HashSet<LocalizedText>();
        private HashSet<LocalizedTextFormatted> activeLocalizedFormattedTexts = new HashSet<LocalizedTextFormatted>();
        private HashSet<LocalizedImage> activeLocalizedImages = new HashSet<LocalizedImage>();

        [Header("Settings")]
        [SerializeField] private bool enableAutoUpdate = true;
        [SerializeField] private bool logUpdates = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Subscribe to language changed event
                LocalizationManager.OnLanguageChanged += OnLanguageChanged;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
                instance = null;
            }
        }

        /// <summary>
        /// Register a LocalizedText component for auto-updates
        /// </summary>
        public static void RegisterLocalizedText(LocalizedText localizedText)
        {
            if (localizedText != null)
            {
                Instance.activeLocalizedTexts.Add(localizedText);
            }
        }

        /// <summary>
        /// Unregister a LocalizedText component
        /// </summary>
        public static void UnregisterLocalizedText(LocalizedText localizedText)
        {
            if (Instance != null && localizedText != null)
            {
                Instance.activeLocalizedTexts.Remove(localizedText);
            }
        }

        /// <summary>
        /// Register a LocalizedTextFormatted component for auto-updates
        /// </summary>
        public static void RegisterLocalizedTextFormatted(LocalizedTextFormatted localizedTextFormatted)
        {
            if (localizedTextFormatted != null)
            {
                Instance.activeLocalizedFormattedTexts.Add(localizedTextFormatted);
            }
        }

        /// <summary>
        /// Unregister a LocalizedTextFormatted component
        /// </summary>
        public static void UnregisterLocalizedTextFormatted(LocalizedTextFormatted localizedTextFormatted)
        {
            if (Instance != null && localizedTextFormatted != null)
            {
                Instance.activeLocalizedFormattedTexts.Remove(localizedTextFormatted);
            }
        }
        public static void RegisterLocalizedImage(LocalizedImage localizedImage)
        {
            if (localizedImage != null)
            {
                Instance.activeLocalizedImages.Add(localizedImage);
            }
        }

        /// <summary>
        /// Unregister a LocalizedImage component
        /// </summary>
        public static void UnregisterLocalizedImage(LocalizedImage localizedImage)
        {
            if (Instance != null && localizedImage != null)
            {
                Instance.activeLocalizedImages.Remove(localizedImage);
            }
        }

        /// <summary>
        /// Called when language changes - updates all registered components
        /// </summary>
        private void OnLanguageChanged(LanguageCode newLanguage)
        {
            if (!enableAutoUpdate) return;

            int updatedTexts = 0;
            int updatedFormattedTexts = 0;
            int updatedImages = 0;

            // Update all LocalizedText components
            foreach (var localizedText in activeLocalizedTexts)
            {
                if (localizedText != null && localizedText.gameObject.activeInHierarchy)
                {
                    localizedText.UpdateText();
                    updatedTexts++;
                }
            }

            // Update all LocalizedTextFormatted components
            foreach (var localizedTextFormatted in activeLocalizedFormattedTexts)
            {
                if (localizedTextFormatted != null && localizedTextFormatted.gameObject.activeInHierarchy)
                {
                    localizedTextFormatted.UpdateText();
                    updatedFormattedTexts++;
                }
            }

            // Update all LocalizedImage components
            foreach (var localizedImage in activeLocalizedImages)
            {
                if (localizedImage != null && localizedImage.gameObject.activeInHierarchy)
                {
                    localizedImage.UpdateSprite();
                    updatedImages++;
                }
            }

            // Clean up null references
            activeLocalizedTexts.RemoveWhere(x => x == null);
            activeLocalizedFormattedTexts.RemoveWhere(x => x == null);
            activeLocalizedImages.RemoveWhere(x => x == null);

            if (logUpdates)
            {
                Debug.Log($"LocalizationAutoUpdater: Updated {updatedTexts} texts, {updatedFormattedTexts} formatted texts, and {updatedImages} images for language: {newLanguage}");
            }
        }

        /// <summary>
        /// Force update all registered components
        /// </summary>
        public void ForceUpdateAll()
        {
            OnLanguageChanged(LocalizationManager.Instance.CurrentLanguage);
        }

        /// <summary>
        /// Get statistics about registered components
        /// </summary>
        public (int textCount, int formattedTextCount, int imageCount) GetComponentCounts()
        {
            // Clean up null references first
            activeLocalizedTexts.RemoveWhere(x => x == null);
            activeLocalizedFormattedTexts.RemoveWhere(x => x == null);
            activeLocalizedImages.RemoveWhere(x => x == null);
            
            return (activeLocalizedTexts.Count, activeLocalizedFormattedTexts.Count, activeLocalizedImages.Count);
        }

        /// <summary>
        /// Enable or disable auto-update functionality
        /// </summary>
        public void SetAutoUpdateEnabled(bool enabled)
        {
            enableAutoUpdate = enabled;
        }

        /// <summary>
        /// Clear all registered components (useful for scene transitions)
        /// </summary>
        public void ClearAllRegistrations()
        {
            activeLocalizedTexts.Clear();
            activeLocalizedFormattedTexts.Clear();
            activeLocalizedImages.Clear();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Get debug information
        /// </summary>
        public void EditorDebugInfo()
        {
            Debug.Log($"LocalizationAutoUpdater Stats:\n" +
                     $"- Registered LocalizedTexts: {activeLocalizedTexts.Count}\n" +
                     $"- Registered LocalizedFormattedTexts: {activeLocalizedFormattedTexts.Count}\n" +
                     $"- Registered LocalizedImages: {activeLocalizedImages.Count}\n" +
                     $"- Auto-update enabled: {enableAutoUpdate}");
        }
#endif
    }
}
