
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Tirex.Game.Utils.Notifications.Editor
{
    public class NotificationSetupWindow : EditorWindow
    {
        private string configPath = "Assets/Utils/Scripts/Notifications/Resources";
        private string configName = "NotificationConfig";

        [MenuItem("TirexGame/Notifications/Setup Notifications")]
        public static void ShowWindow()
        {
            GetWindow<NotificationSetupWindow>("Notification Setup");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Notification Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This window helps you set up the notification system for your project.\n\n" +
                "Steps:\n" +
                "1. Install Mobile Notifications package\n" +
                "2. Create Notification Configuration\n" +
                "3. Configure your notification channels",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            if (IsMobileNotificationsInstalled())
            {
                EditorGUILayout.HelpBox("✓ Mobile Notifications package is installed", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠ Mobile Notifications package is not installed", MessageType.Warning);

                if (GUILayout.Button("Install Mobile Notifications Package"))
                {
                    InstallMobileNotificationsPackage();
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Configuration Setup", EditorStyles.boldLabel);

            configPath = EditorGUILayout.TextField("Config Path:", configPath);
            configName = EditorGUILayout.TextField("Config Name:", configName);

            if (GUILayout.Button("Create Notification Configuration"))
            {
                CreateNotificationConfig();
            }
        }

        private bool IsMobileNotificationsInstalled()
        {
            return File.Exists("Packages/manifest.json") && File.ReadAllText("Packages/manifest.json").Contains("com.unity.mobile.notifications");
        }

        private void InstallMobileNotificationsPackage()
        {
            UnityEditor.PackageManager.Client.Add("com.unity.mobile.notifications");
            EditorUtility.DisplayDialog("Installing Package",
                "Installing Mobile Notifications package via Package Manager. Please wait for the installation to complete.",
                "OK");
        }

        private void CreateNotificationConfig()
        {
            string fullPath = Path.Combine(Application.dataPath, configPath.Replace("Assets/", ""));
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            var config = ScriptableObject.CreateInstance<NotificationConfig>();

            string assetPath = Path.Combine(configPath, configName + ".asset");

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;

            EditorUtility.DisplayDialog("Success",
                $"NotificationConfig created at {assetPath}\n\nPlease configure your notification channels in the inspector.",
                "OK");
        }
    }
}
#endif
