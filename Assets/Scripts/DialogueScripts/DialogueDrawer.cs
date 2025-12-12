#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace DialogueScripts
{
#nullable enable
    /// <summary>
    /// Allows you to add any event that extends DialogueEvents to the DialogueEntryInUnityEditor.events list in the unity editor.
    /// </summary>
    [CustomPropertyDrawer(typeof(DialogueEntryInUnityEditor))]
    public class DialogueTextDrawer : PropertyDrawer
    {
        private readonly Type[] eventTypes;
        private readonly string[] eventTypeNames;
        private int selectedIndex = 0;
        private const float FACTORY_SPACING = 10f;

        public DialogueTextDrawer()
        {
            eventTypes = GetImplementations(typeof(DialogueEvents));
            eventTypeNames = eventTypes.Select(type => type.Name).ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty iterator = property.Copy();
                SerializedProperty endProperty = property.GetEndProperty();
                SerializedProperty? eventsListProperty = null;
                SerializedProperty? actorProfileProperty = null;

                if (iterator.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(iterator, endProperty))
                            break;

                        float propHeight = EditorGUI.GetPropertyHeight(iterator, true);
                        Rect propRect = new Rect(position.x, y, position.width, propHeight);
                        EditorGUI.PropertyField(propRect, iterator, true);

                        if (iterator.name == nameof(DialogueEntryInUnityEditor.events))
                        {
                            eventsListProperty = iterator.Copy();
                        } else if (iterator.name == nameof(DialogueEntryInUnityEditor.speaker))
                        {
                            actorProfileProperty = iterator.Copy();
                        }
                        
                        y += propHeight + EditorGUIUtility.standardVerticalSpacing;

                    }
                    while (iterator.NextVisible(false));
                }

                if (eventsListProperty != null)
                {
                    y += FACTORY_SPACING;

                    // Factory Dropdown
                    Rect dropdownRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                    selectedIndex = EditorGUI.Popup(dropdownRect, "Add Event", selectedIndex, eventTypeNames);
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    // Add Button
                    Rect buttonRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(buttonRect, "Add Selected Event"))
                    {
                        AddEvent(eventsListProperty, actorProfileProperty, selectedIndex);
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float totalHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();

            if (iterator.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(iterator, endProperty))
                        break;

                    totalHeight += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                }
                while (iterator.NextVisible(false));
            }

            totalHeight += FACTORY_SPACING;
            totalHeight += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;

            return totalHeight;
        }

        private void AddEvent(SerializedProperty eventsList, SerializedProperty? profileProp, int typeIndex)
        {
            Type selectedType = eventTypes[typeIndex];

            object newEventInstance = Activator.CreateInstance(selectedType);

            // Automatically populates the ActorProfile field of the event if the event needs one.
            if (profileProp != null && profileProp.objectReferenceValue != null)
            {
                selectedType
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(f => f.FieldType == typeof(ActorProfile))
                    ?.SetValue(newEventInstance, profileProp.objectReferenceValue);
            }

            int newIndex = eventsList.arraySize;
            eventsList.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newElementProperty = eventsList.GetArrayElementAtIndex(newIndex);

            newElementProperty.managedReferenceValue = newEventInstance;
        }

        private void AutoAssignActorProfile(object instance, SerializedProperty? profileProp){
        }

        private Type[] GetImplementations(Type baseType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = new List<Type>();
            foreach (var assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(t =>
                        baseType.IsAssignableFrom(t) &&
                        !t.IsInterface &&
                        !t.IsAbstract
                    ));
                }
                catch
                {
                    Debug.LogWarning("There was some error in grabbing event implementations in Dialogue Drawer.");
                }
            }
            return types.ToArray();
        }
    }
}
#endif