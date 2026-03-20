using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Tirex.Utils.ObjectPooling.Editor
{
    public class ObjectPoolingMonitorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private string stringSearch = "";
        private bool autoRefresh = true;
        private double lastRefreshTime;
        private const float RefreshRate = 0.5f; // 0.5 seconds
        
        [MenuItem("TirexGame/Object Pooling Monitor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ObjectPoolingMonitorWindow>("Pool Monitor");
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > RefreshRate)
            {
                lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            
            selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Standard Pools", "Addressable Pools" });
            
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            if (selectedTab == 0)
                DrawStandardPools();
            else
                DrawAddressablePools();
                
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            autoRefresh = GUILayout.Toggle(autoRefresh, "Auto Refresh", EditorStyles.toolbarButton, GUILayout.Width(100));
            if (GUILayout.Button("Refresh Now", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                Repaint();
            }
            
            GUILayout.FlexibleSpace();
            
            var searchStyle = GUI.skin.FindStyle("ToolbarSearchTextField") ?? GUI.skin.FindStyle("ToolbarSeachTextField") ?? GUI.skin.textField;
            var cancelStyle = GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? GUI.skin.FindStyle("ToolbarSeachCancelButton") ?? GUI.skin.button;
            
            stringSearch = EditorGUILayout.TextField(stringSearch, searchStyle, GUILayout.Width(250));
            if (GUILayout.Button("", cancelStyle))
            {
                stringSearch = "";
                GUI.FocusControl(null);
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawStandardPools()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view Standard Object Pools.", MessageType.Info);
                return;
            }

            var keys = ObjectPooling.GetAllPoolKeys().ToList();
            if (keys.Count == 0)
            {
                EditorGUILayout.HelpBox("No active standard pools.", MessageType.Info);
                return;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All Standard Pools", GUILayout.Height(30)))
            {
                ObjectPooling.ClearAllPools();
                GUIUtility.ExitGUI();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            DrawHeader("Prefab Name", "Size", "Active", "Created", "Peak", "Actions");

            foreach (var prefab in keys)
            {
                if (prefab == null) continue;
                
                if (!string.IsNullOrEmpty(stringSearch) && !prefab.name.ToLower().Contains(stringSearch.ToLower()))
                    continue;

                int poolSize = ObjectPooling.GetPoolSize(prefab);
                int activeCount = ObjectPooling.GetActiveCount(prefab);
                int created = ObjectPooling.GetTotalCreated(prefab);
                int peak = ObjectPooling.GetPeakActiveCount(prefab);

                GUILayout.BeginHorizontal("box");
                
                GUILayout.Label(prefab.name, GUILayout.Width(200));
                GUILayout.Label(poolSize.ToString(), GUILayout.Width(60));
                GUILayout.Label(activeCount.ToString(), GUILayout.Width(60));
                GUILayout.Label(created >= 0 ? created.ToString() : "-", GUILayout.Width(60));
                GUILayout.Label(peak >= 0 ? peak.ToString() : "-", GUILayout.Width(60));
                
                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    EditorGUIUtility.PingObject(prefab);
                }
                
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                {
                    ObjectPooling.ClearPool(prefab);
                    GUIUtility.ExitGUI();
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawAddressablePools()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view Addressable Pools.", MessageType.Info);
                return;
            }

            var keys = AddressableObjectPooling.GetAllPoolKeys().ToList();
            if (keys.Count == 0)
            {
                EditorGUILayout.HelpBox("No active addressable pools.", MessageType.Info);
                return;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Unload All Addressable Pools", GUILayout.Height(30)))
            {
                AddressableObjectPooling.UnloadAllPools();
                GUIUtility.ExitGUI();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            DrawHeader("Asset GUID", "Size", "-", "-", "-", "Actions");

            foreach (var assetRef in keys)
            {
                if (assetRef == null) continue;
                
                string guid = assetRef.AssetGUID;
                if (!string.IsNullOrEmpty(stringSearch) && !guid.ToLower().Contains(stringSearch.ToLower()))
                    continue;

                int poolSize = AddressableObjectPooling.GetPoolSize(assetRef);

                GUILayout.BeginHorizontal("box");
                
                GUILayout.Label(guid, GUILayout.Width(200));
                GUILayout.Label(poolSize.ToString(), GUILayout.Width(60));
                GUILayout.Label("-", GUILayout.Width(60));
                GUILayout.Label("-", GUILayout.Width(60));
                GUILayout.Label("-", GUILayout.Width(60));
                
                if (GUILayout.Button("Copy ID", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    GUIUtility.systemCopyBuffer = guid;
                    Debug.Log($"[PoolMonitor] Copied GUID: {guid}");
                }
                
                if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    AddressableObjectPooling.ClearPool(assetRef);
                    GUIUtility.ExitGUI();
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawHeader(string c1, string c2, string c3, string c4, string c5, string actions)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(c1, EditorStyles.miniBoldLabel, GUILayout.Width(200));
            GUILayout.Label(c2, EditorStyles.miniBoldLabel, GUILayout.Width(60));
            GUILayout.Label(c3, EditorStyles.miniBoldLabel, GUILayout.Width(60));
            GUILayout.Label(c4, EditorStyles.miniBoldLabel, GUILayout.Width(60));
            GUILayout.Label(c5, EditorStyles.miniBoldLabel, GUILayout.Width(60));
            GUILayout.Label(actions, EditorStyles.miniBoldLabel);
            GUILayout.EndHorizontal();
        }
    }
}