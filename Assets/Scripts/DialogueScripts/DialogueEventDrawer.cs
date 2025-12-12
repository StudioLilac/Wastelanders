#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DialogueScripts
{
    /// This drawer tells Unity how to draw any class that inherits from DialogueEvents.
    /// Inside a List of Dialogue Events, this class makes sure elements are labelled by their class name rather than index.
    /// Ex. 'MoveCharacter' instead of 'Element 0'
#nullable enable
    [CustomPropertyDrawer(typeof(DialogueEvents), true)]
    public class DialogueEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (!property.isExpanded) 
                property.isExpanded = true;
            
            object? targetObject = property.managedReferenceValue;
            label.text = targetObject?.GetType().Name ?? "Illegal Event";

            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
}
#endif