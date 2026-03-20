#if UNITY_EDITOR && AUTO_APPLY_SPRITE_SETTINGS
using UnityEditor;
using UnityEngine;

public class SpriteImportSettings
{
    private const string SPRITE_PIXELS_PER_UNIT_KEY = "sprite_pixels_per_unit";
    private const string FILTER_MODE_KEY = "sprite_filter_mode";
    private const string TEXTURE_COMPRESSION_KEY = "sprite_texture_compression";
    private const string TEXTURE_TYPE_KEY = "sprite_texture_type";
    private const string MAX_SIZE_KEY = "sprite_max_size";
    private const string FORMAT_KEY = "sprite_format";
    private const string AUTO_APPLY_KEY = "auto_apply_sprite_settings";

    // Method to open settings window for setting default import settings
    [MenuItem("Tools/Asset/Import/Set Default Sprite Import Settings")]
    public static void SetSpriteImportSettings()
    {
        // Show a custom window for adjusting sprite import settings
        SpriteImportSettingsWindow.ShowWindow();
    }

    // Custom editor window for setting the import settings
    public class SpriteImportSettingsWindow : EditorWindow
    {
        private int selectedPixelsPerUnit = 100;
        private FilterMode selectedFilterMode = FilterMode.Point;
        private TextureImporterCompression selectedCompression = TextureImporterCompression.Uncompressed;
        private TextureImporterType selectedTextureType = TextureImporterType.Sprite;
        private int selectedMaxSize = 1024;
        private TextureImporterFormat selectedFormat = TextureImporterFormat.Automatic;
        private bool autoApplySettings = false;  // Toggle for auto apply

        public static void ShowWindow()
        {
            var window = GetWindow<SpriteImportSettingsWindow>();
            window.titleContent = new GUIContent("Sprite Import Settings");
            window.Show();
        }

        private void OnEnable()
        {
            // Load saved settings from EditorPrefs
            selectedPixelsPerUnit = EditorPrefs.GetInt(SPRITE_PIXELS_PER_UNIT_KEY, 100);
            selectedFilterMode = (FilterMode)EditorPrefs.GetInt(FILTER_MODE_KEY, (int)FilterMode.Point);
            selectedCompression = (TextureImporterCompression)EditorPrefs.GetInt(TEXTURE_COMPRESSION_KEY, (int)TextureImporterCompression.Uncompressed);
            selectedTextureType = (TextureImporterType)EditorPrefs.GetInt(TEXTURE_TYPE_KEY, (int)TextureImporterType.Sprite);
            selectedMaxSize = EditorPrefs.GetInt(MAX_SIZE_KEY, 1024);
            selectedFormat = (TextureImporterFormat)EditorPrefs.GetInt(FORMAT_KEY, (int)TextureImporterFormat.Automatic);
            autoApplySettings = EditorPrefs.GetBool(AUTO_APPLY_KEY, false);  // Load toggle value
        }

        private void OnGUI()
        {
            GUILayout.Label("Select Import Settings for Sprites", EditorStyles.boldLabel);

            // Sprite Pixels Per Unit
            selectedPixelsPerUnit = EditorGUILayout.IntField("Sprite Pixels Per Unit", selectedPixelsPerUnit);

            // Filter Mode
            selectedFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", selectedFilterMode);

            // Texture Compression
            selectedCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Texture Compression", selectedCompression);

            // Texture Type
            selectedTextureType = (TextureImporterType)EditorGUILayout.EnumPopup("Texture Type", selectedTextureType);

            // Max Size
            selectedMaxSize = EditorGUILayout.IntField("Max Size", selectedMaxSize);

            // Format
            selectedFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Format", selectedFormat);

            // Toggle for auto apply settings
            autoApplySettings = EditorGUILayout.Toggle("Auto Apply Settings", autoApplySettings);

            // Save button
            if (GUILayout.Button("Save Settings"))
            {
                // Save settings to EditorPrefs
                EditorPrefs.SetInt(SPRITE_PIXELS_PER_UNIT_KEY, selectedPixelsPerUnit);
                EditorPrefs.SetInt(FILTER_MODE_KEY, (int)selectedFilterMode);
                EditorPrefs.SetInt(TEXTURE_COMPRESSION_KEY, (int)selectedCompression);
                EditorPrefs.SetInt(TEXTURE_TYPE_KEY, (int)selectedTextureType);
                EditorPrefs.SetInt(MAX_SIZE_KEY, selectedMaxSize);
                EditorPrefs.SetInt(FORMAT_KEY, (int)selectedFormat);
                EditorPrefs.SetBool(AUTO_APPLY_KEY, autoApplySettings);  // Save the toggle state

                Debug.Log("Sprite import settings saved.");
            }
        }
    }

    // Asset Postprocessor class to apply the settings automatically on asset import
    public class SpriteAssetPostprocessor : AssetPostprocessor
    {
        // Called when a sprite asset is imported
        private void OnPreprocessTexture()
        {
            var isApplySettings = EditorPrefs.GetBool(AUTO_APPLY_KEY, true);
            if (!isApplySettings)
                return;
            if (assetImporter is not TextureImporter importer) 
                return;
            // Only apply the settings if the auto-apply feature is enabled
            if (!EditorPrefs.GetBool(AUTO_APPLY_KEY, true))
                return;
            // Check if the asset is a sprite
            if (!importer.assetPath.EndsWith(".png") && !importer.assetPath.EndsWith(".jpg") &&
                !importer.assetPath.EndsWith(".tga"))
                return;
            // Load saved settings
            var pixelsPerUnit = EditorPrefs.GetInt(SPRITE_PIXELS_PER_UNIT_KEY, 100);
            var filterMode = (FilterMode)EditorPrefs.GetInt(FILTER_MODE_KEY, (int)FilterMode.Point);
            var compression = (TextureImporterCompression)EditorPrefs.GetInt(TEXTURE_COMPRESSION_KEY, (int)TextureImporterCompression.Uncompressed);
            var textureType = (TextureImporterType)EditorPrefs.GetInt(TEXTURE_TYPE_KEY, (int)TextureImporterType.Sprite);
            var maxSize = EditorPrefs.GetInt(MAX_SIZE_KEY, 1024);
            var format = (TextureImporterFormat)EditorPrefs.GetInt(FORMAT_KEY, (int)TextureImporterFormat.Automatic);

            // Apply settings
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = filterMode;
            importer.textureCompression = compression;
            importer.textureType = textureType;
            importer.maxTextureSize = maxSize;
#pragma warning disable CS0618 // Type or member is obsolete
            importer.textureFormat = format;
#pragma warning restore CS0618 // Type or member is obsolete

            // Log the applied settings
            Debug.Log($"Applied Sprite Import Settings: {importer.assetPath}");
        }
    }
}
#endif
