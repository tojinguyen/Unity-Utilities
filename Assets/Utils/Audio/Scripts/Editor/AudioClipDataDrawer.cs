using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AudioClipData))]
public class AudioClipDataDrawer : PropertyDrawer
{
    private const float HEADER_HEIGHT = 20f;
    private const float SPACING = 2f;
    private const float INDENT = 15f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Calculate foldout rectangle
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        
        // Get the id property to use as the main label
        SerializedProperty idProperty = property.FindPropertyRelative("id");
        string displayName = string.IsNullOrEmpty(idProperty.stringValue) ? "Audio Clip Data" : idProperty.stringValue;
        
        // Draw foldout
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, displayName, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float yPos = position.y + EditorGUIUtility.singleLineHeight + SPACING;

            // Audio Settings Header
            DrawHeaderLabel(new Rect(position.x, yPos, position.width, HEADER_HEIGHT), "Audio Settings");
            yPos += HEADER_HEIGHT + SPACING;

            // ID Field
            SerializedProperty audioTypeProperty = property.FindPropertyRelative("audioType");
            SerializedProperty playModeProperty = property.FindPropertyRelative("playMode");

            yPos = DrawPropertyField(position, idProperty, "ID", yPos);
            yPos = DrawPropertyField(position, audioTypeProperty, "Audio Type", yPos);
            yPos = DrawPropertyField(position, playModeProperty, "Play Mode", yPos);

            // Clip Reference Header
            yPos += SPACING;
            DrawHeaderLabel(new Rect(position.x, yPos, position.width, HEADER_HEIGHT), "Clip Reference");
            yPos += HEADER_HEIGHT + SPACING;

            SerializedProperty audioClipProperty = property.FindPropertyRelative("audioClip");
            SerializedProperty audioClipReferenceProperty = property.FindPropertyRelative("audioClipReference");

            yPos = DrawPropertyField(position, audioClipProperty, "Audio Clip", yPos);
            yPos = DrawPropertyField(position, audioClipReferenceProperty, "Audio Clip Reference", yPos);

            // Volume & Pitch Header
            yPos += SPACING;
            DrawHeaderLabel(new Rect(position.x, yPos, position.width, HEADER_HEIGHT), "Volume & Pitch");
            yPos += HEADER_HEIGHT + SPACING;

            SerializedProperty volumeProperty = property.FindPropertyRelative("volume");
            SerializedProperty pitchProperty = property.FindPropertyRelative("pitch");
            SerializedProperty spatialBlendProperty = property.FindPropertyRelative("spatialBlend");

            yPos = DrawPropertyField(position, volumeProperty, "Volume", yPos);
            yPos = DrawPropertyField(position, pitchProperty, "Pitch", yPos);
            yPos = DrawPropertyField(position, spatialBlendProperty, "Spatial Blend", yPos);

            // Loop Settings Header
            yPos += SPACING;
            DrawHeaderLabel(new Rect(position.x, yPos, position.width, HEADER_HEIGHT), "Loop Settings");
            yPos += HEADER_HEIGHT + SPACING;

            SerializedProperty minRandomDelayProperty = property.FindPropertyRelative("minRandomDelay");
            SerializedProperty maxRandomDelayProperty = property.FindPropertyRelative("maxRandomDelay");

            yPos = DrawPropertyField(position, minRandomDelayProperty, "Min Random Delay", yPos);
            yPos = DrawPropertyField(position, maxRandomDelayProperty, "Max Random Delay", yPos);

            // Fade Settings Header
            yPos += SPACING;
            DrawHeaderLabel(new Rect(position.x, yPos, position.width, HEADER_HEIGHT), "Fade Settings");
            yPos += HEADER_HEIGHT + SPACING;

            SerializedProperty useFadeProperty = property.FindPropertyRelative("useFade");
            SerializedProperty fadeInDurationProperty = property.FindPropertyRelative("fadeInDuration");
            SerializedProperty fadeOutDurationProperty = property.FindPropertyRelative("fadeOutDuration");

            yPos = DrawPropertyField(position, useFadeProperty, "Use Fade", yPos);
            
            if (useFadeProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                yPos = DrawPropertyField(position, fadeInDurationProperty, "Fade In Duration", yPos);
                yPos = DrawPropertyField(position, fadeOutDurationProperty, "Fade Out Duration", yPos);
                EditorGUI.indentLevel--;
            }

            // 3D Audio Settings Header
            yPos += SPACING;
            DrawHeaderLabel(new Rect(position.x, yPos, position.width, HEADER_HEIGHT), "3D Audio Settings");
            yPos += HEADER_HEIGHT + SPACING;

            SerializedProperty minDistanceProperty = property.FindPropertyRelative("minDistance");
            SerializedProperty maxDistanceProperty = property.FindPropertyRelative("maxDistance");
            SerializedProperty rolloffModeProperty = property.FindPropertyRelative("rolloffMode");

            yPos = DrawPropertyField(position, minDistanceProperty, "Min Distance", yPos);
            yPos = DrawPropertyField(position, maxDistanceProperty, "Max Distance", yPos);
            yPos = DrawPropertyField(position, rolloffModeProperty, "Rolloff Mode", yPos);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        float height = EditorGUIUtility.singleLineHeight + SPACING; // Foldout line

        // Audio Settings
        height += HEADER_HEIGHT + SPACING;
        height += (EditorGUIUtility.singleLineHeight + SPACING) * 3; // id, audioType, playMode

        // Clip Reference
        height += SPACING + HEADER_HEIGHT + SPACING;
        height += (EditorGUIUtility.singleLineHeight + SPACING) * 2; // audioClip, audioClipReference

        // Volume & Pitch
        height += SPACING + HEADER_HEIGHT + SPACING;
        height += (EditorGUIUtility.singleLineHeight + SPACING) * 3; // volume, pitch, spatialBlend

        // Loop Settings
        height += SPACING + HEADER_HEIGHT + SPACING;
        height += (EditorGUIUtility.singleLineHeight + SPACING) * 2; // minRandomDelay, maxRandomDelay

        // Fade Settings
        height += SPACING + HEADER_HEIGHT + SPACING;
        height += EditorGUIUtility.singleLineHeight + SPACING; // useFade

        SerializedProperty useFadeProperty = property.FindPropertyRelative("useFade");
        if (useFadeProperty != null && useFadeProperty.boolValue)
        {
            height += (EditorGUIUtility.singleLineHeight + SPACING) * 2; // fadeIn, fadeOut
        }

        // 3D Audio Settings
        height += SPACING + HEADER_HEIGHT + SPACING;
        height += (EditorGUIUtility.singleLineHeight + SPACING) * 3; // minDistance, maxDistance, rolloffMode

        return height;
    }

    private float DrawPropertyField(Rect containerRect, SerializedProperty property, string label, float yPos)
    {
        Rect fieldRect = new Rect(containerRect.x, yPos, containerRect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(fieldRect, property, new GUIContent(label));
        return yPos + EditorGUIUtility.singleLineHeight + SPACING;
    }

    private void DrawHeaderLabel(Rect rect, string text)
    {
        EditorGUI.LabelField(rect, text, EditorStyles.boldLabel);
    }
}