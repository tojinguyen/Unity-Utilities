#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using TirexGame.Utils.UI;

namespace TirexGame.Utils.UI.Editor
{
    /// <summary>
    /// Custom editor for FloatingTextManager with preview and testing tools
    /// </summary>
    [CustomEditor(typeof(FloatingTextManager))]
    public class FloatingTextManagerEditor : UnityEditor.Editor
    {
        private string previewText = "100";
        private FloatingTextData previewData;
        private bool preview3D = false;
        private Vector3 previewPosition = Vector3.zero;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Preview & Testing", EditorStyles.boldLabel);

            previewText = EditorGUILayout.TextField("Text", previewText);
            previewData = (FloatingTextData)EditorGUILayout.ObjectField("Data", previewData, typeof(FloatingTextData), false);
            preview3D = EditorGUILayout.Toggle("Use 3D Mode", preview3D);
            previewPosition = EditorGUILayout.Vector3Field("Position", previewPosition);

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Preview Floating Text", GUILayout.Height(30)))
            {
                if (Application.isPlaying)
                {
                    if (previewData != null)
                    {
                        FloatingTextFactory.Create(previewText, previewPosition, previewData, preview3D);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Preview", "Please assign a FloatingTextData asset to preview.", "OK");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Preview", "Please enter Play Mode to preview floating text.", "OK");
                }
            }

            if (Application.isPlaying && previewData != null)
            {
                EditorGUILayout.Space(5);

                if (GUILayout.Button("Test with Random Numbers"))
                {
                    FloatingTextFactory.Create(Random.Range(10, 100).ToString("0"), previewPosition, previewData, preview3D);
                }
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Clear All", GUILayout.Height(25)))
                {
                    FloatingTextManager.Instance.ClearAll();
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "💡 Tips:\n" +
                "• Enter Play Mode to test floating text\n" +
                "• Use FloatingTextFactory for quick creation\n" +
                "• Create FloatingTextData assets for custom presets\n" +
                "• Object pooling is automatic",
                MessageType.Info
            );
        }
    }

    /// <summary>
    /// Custom editor for FloatingTextData with preview curves
    /// </summary>
    [CustomEditor(typeof(FloatingTextData))]
    public class FloatingTextDataEditor : UnityEditor.Editor
    {
        private SerializedProperty moveDirection;
        private SerializedProperty moveSpeed;
        private SerializedProperty lifetime;
        private SerializedProperty randomizeDirection;
        private SerializedProperty randomAngle;
        private SerializedProperty scaleCurve;
        private SerializedProperty alphaCurve;
        private SerializedProperty fontSize;
        private SerializedProperty textColor;
        private SerializedProperty fontStyle;
        private SerializedProperty useWorldSpace;

        private void OnEnable()
        {
            moveDirection = serializedObject.FindProperty("MoveDirection");
            moveSpeed = serializedObject.FindProperty("MoveSpeed");
            lifetime = serializedObject.FindProperty("Lifetime");
            randomizeDirection = serializedObject.FindProperty("RandomizeDirection");
            randomAngle = serializedObject.FindProperty("RandomAngle");
            scaleCurve = serializedObject.FindProperty("ScaleCurve");
            alphaCurve = serializedObject.FindProperty("AlphaCurve");
            fontSize = serializedObject.FindProperty("FontSize");
            textColor = serializedObject.FindProperty("TextColor");
            fontStyle = serializedObject.FindProperty("FontStyle");
            useWorldSpace = serializedObject.FindProperty("UseWorldSpace");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(moveDirection);
            EditorGUILayout.PropertyField(moveSpeed);
            EditorGUILayout.PropertyField(lifetime);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Randomization", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomizeDirection);
            if (randomizeDirection.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(randomAngle);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Animation Curves", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(scaleCurve);
            EditorGUILayout.PropertyField(alphaCurve);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fontSize);
            EditorGUILayout.PropertyField(textColor);
            EditorGUILayout.PropertyField(fontStyle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(useWorldSpace);

            serializedObject.ApplyModifiedProperties();

            // Quick preset buttons
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Damage"))
            {
                ApplyPreset(FloatingTextData.CreateDamageDefault());
            }
            if (GUILayout.Button("Healing"))
            {
                ApplyPreset(FloatingTextData.CreateHealingDefault());
            }
            if (GUILayout.Button("Critical"))
            {
                ApplyPreset(FloatingTextData.CreateCriticalDefault());
            }

            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void ApplyPreset(FloatingTextData preset)
        {
            Undo.RecordObject(target, "Apply Preset");

            FloatingTextData data = (FloatingTextData)target;
            data.MoveDirection = preset.MoveDirection;
            data.MoveSpeed = preset.MoveSpeed;
            data.Lifetime = preset.Lifetime;
            data.RandomizeDirection = preset.RandomizeDirection;
            data.RandomAngle = preset.RandomAngle;
            data.ScaleCurve = new AnimationCurve(preset.ScaleCurve.keys);
            data.AlphaCurve = new AnimationCurve(preset.AlphaCurve.keys);
            data.FontSize = preset.FontSize;
            data.TextColor = preset.TextColor;
            data.FontStyle = preset.FontStyle;

            EditorUtility.SetDirty(target);
            serializedObject.Update();
        }
    }

    /// <summary>
    /// Custom editor for FloatingTextBase component
    /// </summary>
    [CustomEditor(typeof(FloatingTextBase), true)]
    public class FloatingTextEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FloatingTextBase floatingText = (FloatingTextBase)target;

            EditorGUILayout.Space(5);
            string mode = floatingText is FloatingText2D ? "UI (2D)" : "World Space (3D)";
            EditorGUILayout.HelpBox(
                $"Mode: {mode}\n" +
                "The mode is automatically detected based on the text component attached.",
                MessageType.Info
            );

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Test Animation", GUILayout.Height(25)))
                {
                    floatingText.Show("Test", floatingText.transform.position);
                }
            }
        }
    }
}
#endif
