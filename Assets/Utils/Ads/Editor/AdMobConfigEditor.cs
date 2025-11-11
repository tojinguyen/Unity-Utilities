#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TirexGame.Utils.Ads.Editor
{
    [CustomEditor(typeof(AdMobConfig))]
    public class AdMobConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            AdMobConfig config = (AdMobConfig)target;
            
            EditorGUI.BeginChangeCheck();
            
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Ad Unit Management", EditorStyles.boldLabel);
            
            // Android Ad Unit Management
            EditorGUILayout.LabelField("Android Ad Units", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Android IDs", GUILayout.Width(120)))
            {
                if (EditorUtility.DisplayDialog("Clear Android Ad Unit IDs", 
                    "Are you sure you want to clear all Android ad unit IDs?", 
                    "Yes", "Cancel"))
                {
                    var androidIds = GetAndroidAdUnitIds(config);
                    androidIds?.ClearAll();
                    EditorUtility.SetDirty(config);
                }
            }
            
            if (GUILayout.Button("Set Android Test IDs", GUILayout.Width(140)))
            {
                var androidIds = GetAndroidAdUnitIds(config);
                androidIds?.SetAndroidTestIds();
                EditorUtility.SetDirty(config);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // iOS Ad Unit Management
            EditorGUILayout.LabelField("iOS Ad Units", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear iOS IDs", GUILayout.Width(120)))
            {
                if (EditorUtility.DisplayDialog("Clear iOS Ad Unit IDs", 
                    "Are you sure you want to clear all iOS ad unit IDs?", 
                    "Yes", "Cancel"))
                {
                    var iosIds = GetIOSAdUnitIds(config);
                    iosIds?.ClearAll();
                    EditorUtility.SetDirty(config);
                }
            }
            
            if (GUILayout.Button("Set iOS Test IDs", GUILayout.Width(140)))
            {
                var iosIds = GetIOSAdUnitIds(config);
                iosIds?.SetIOSTestIds();
                EditorUtility.SetDirty(config);
            }
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(config);
            }
        }
        
        private AdMobConfig.AdUnitIds GetAndroidAdUnitIds(AdMobConfig config)
        {
            var androidField = typeof(AdMobConfig).GetField("androidAdUnitIds", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return androidField?.GetValue(config) as AdMobConfig.AdUnitIds;
        }
        
        private AdMobConfig.AdUnitIds GetIOSAdUnitIds(AdMobConfig config)
        {
            var iosField = typeof(AdMobConfig).GetField("iosAdUnitIds", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return iosField?.GetValue(config) as AdMobConfig.AdUnitIds;
        }
    }
}
#endif