using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// Property drawer for ConditionalField attribute
/// </summary>
[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute condAttribute = (ConditionalFieldAttribute)attribute;
        bool enabled = GetConditionResult(condAttribute, property);

        bool wasEnabled = GUI.enabled;
        GUI.enabled = enabled;
        
        if (enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
        
        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute condAttribute = (ConditionalFieldAttribute)attribute;
        bool enabled = GetConditionResult(condAttribute, property);
        
        if (enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool GetConditionResult(ConditionalFieldAttribute condAttribute, SerializedProperty property)
    {
        bool enabled = true;
        string propertyPath = property.propertyPath;
        string conditionPath = propertyPath.Replace(property.name, condAttribute.FieldToCheck);
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            enabled = CheckPropertyType(sourcePropertyValue, condAttribute.CompareValue);
            if (condAttribute.Inverse) enabled = !enabled;
        }

        return enabled;
    }

    private bool CheckPropertyType(SerializedProperty sourcePropertyValue, object compareValue)
    {
        switch (sourcePropertyValue.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return sourcePropertyValue.boolValue.Equals(compareValue);
            case SerializedPropertyType.Enum:
                return sourcePropertyValue.enumValueIndex.Equals((int)compareValue);
            case SerializedPropertyType.Integer:
                return sourcePropertyValue.intValue.Equals(compareValue);
            case SerializedPropertyType.Float:
                return Mathf.Approximately(sourcePropertyValue.floatValue, (float)compareValue);
            case SerializedPropertyType.String:
                return sourcePropertyValue.stringValue.Equals(compareValue);
            default:
                return true;
        }
    }
}

/// <summary>
/// Property drawer for ConditionalHeader attribute
/// </summary>
[CustomPropertyDrawer(typeof(ConditionalHeaderAttribute))]
public class ConditionalHeaderPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalHeaderAttribute condAttribute = (ConditionalHeaderAttribute)attribute;
        bool enabled = GetConditionResult(condAttribute, property);

        if (enabled)
        {
            // Draw header
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, condAttribute.Text, EditorStyles.boldLabel);
            
            // Draw property below header
            Rect propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, 
                                       position.width, EditorGUI.GetPropertyHeight(property, label));
            EditorGUI.PropertyField(propertyRect, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalHeaderAttribute condAttribute = (ConditionalHeaderAttribute)attribute;
        bool enabled = GetConditionResult(condAttribute, property);
        
        if (enabled)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool GetConditionResult(ConditionalHeaderAttribute condAttribute, SerializedProperty property)
    {
        bool enabled = true;
        string propertyPath = property.propertyPath;
        string conditionPath = propertyPath.Replace(property.name, condAttribute.FieldToCheck);
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            enabled = CheckPropertyType(sourcePropertyValue, condAttribute.CompareValue);
            if (condAttribute.Inverse) enabled = !enabled;
        }

        return enabled;
    }

    private bool CheckPropertyType(SerializedProperty sourcePropertyValue, object compareValue)
    {
        switch (sourcePropertyValue.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return sourcePropertyValue.boolValue.Equals(compareValue);
            case SerializedPropertyType.Enum:
                return sourcePropertyValue.enumValueIndex.Equals((int)compareValue);
            case SerializedPropertyType.Integer:
                return sourcePropertyValue.intValue.Equals(compareValue);
            case SerializedPropertyType.Float:
                return Mathf.Approximately(sourcePropertyValue.floatValue, (float)compareValue);
            case SerializedPropertyType.String:
                return sourcePropertyValue.stringValue.Equals(compareValue);
            default:
                return true;
        }
    }
}
#endif