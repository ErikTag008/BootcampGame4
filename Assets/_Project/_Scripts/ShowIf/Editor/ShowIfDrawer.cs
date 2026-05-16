using UnityEditor;
using UnityEngine;
namespace Project.Assets._Project._Scripts.ShowIf.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (ShouldShow(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return 0f;
        }

        private bool ShouldShow(SerializedProperty property)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;

            SerializedProperty sourceProperty =
                property.serializedObject.FindProperty(showIf.ConditionalSourceField);

            if (sourceProperty == null)
            {
                Debug.LogWarning($"ShowIf: Could not find field {showIf.ConditionalSourceField}");
                return true;
            }

            return sourceProperty.propertyType switch
            {
                SerializedPropertyType.Enum => sourceProperty.enumValueIndex.Equals((int)showIf.CompareValue),
                SerializedPropertyType.Boolean => sourceProperty.boolValue.Equals(showIf.CompareValue),
                SerializedPropertyType.Integer => sourceProperty.intValue.Equals(showIf.CompareValue),
                SerializedPropertyType.Float => sourceProperty.floatValue.Equals(showIf.CompareValue),
                _ => true,
            };
        }
    }
}
