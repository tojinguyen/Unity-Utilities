using UnityEditor;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene.Editor
{
    [CustomEditor(typeof(LoadSceneManager))]
    public class LoadSceneManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _ui;
        private SerializedProperty _completionDelay;
        private float _previewProgress;

        private void OnEnable()
        {
            _ui             = serializedObject.FindProperty("_ui");
            _completionDelay = serializedObject.FindProperty("_completionDelay");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Fields
            EditorGUILayout.PropertyField(_ui, new GUIContent("Loading UI Controller"));
            EditorGUILayout.PropertyField(_completionDelay, new GUIContent("Completion Delay",
                "Thời gian dừng tại 100% (giây) trước khi ẩn màn hình loading."));

            serializedObject.ApplyModifiedProperties();

            // Warning nếu chưa assign UI
            if (_ui.objectReferenceValue == null)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox("Loading UI Controller chưa được assign!", MessageType.Warning);
            }

            // Tools
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Auto-Find UI in Scene"))
                AutoFindUI();

            if (GUILayout.Button("Open Creator Window"))
                LoadingManagerCreatorWindow.Open();

            // Play mode preview
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Preview (Play Mode)", EditorStyles.boldLabel);

                var ui = _ui.objectReferenceValue as DefaultLoadingUIController;

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = ui != null;
                if (GUILayout.Button("Show UI")) ui?.ShowUI();
                if (GUILayout.Button("Hide UI")) ui?.HideUI();
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                if (ui != null)
                {
                    EditorGUILayout.Space(2);
                    EditorGUI.BeginChangeCheck();
                    _previewProgress = EditorGUILayout.Slider("Preview Progress", _previewProgress, 0f, 1f);
                    if (EditorGUI.EndChangeCheck())
                        ui.SetProgress(_previewProgress);
                }
            }
        }

        private void AutoFindUI()
        {
            var ui = FindFirstObjectByType<DefaultLoadingUIController>();
            if (ui != null)
            {
                serializedObject.Update();
                _ui.objectReferenceValue = ui;
                serializedObject.ApplyModifiedProperties();
                Debug.Log("[LoadSceneManager] DefaultLoadingUIController assigned automatically.");
            }
            else
            {
                EditorUtility.DisplayDialog("Not Found",
                    "Không tìm thấy DefaultLoadingUIController trong scene.\n\n" +
                    "Dùng 'Open Creator Window' để tạo setup hoàn chỉnh.",
                    "OK");
            }
        }
    }
}
