using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SpaceFox
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ObservableValue), true)]
    public class ObservableValuePropertyDrawer : PropertyDrawer
    {
        private const string NoDrawableLabel = "No drawable";
        private const string NoDrawableTip = "Generic type is not serializable, or ObservableValue inheritor hasn't suitable fields";

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var innerProperties = GetInnerProperties(property);
            if (innerProperties.Any())
            {
                EditorGUI.BeginChangeCheck();

                foreach (var p in innerProperties)
                    EditorGUILayout.PropertyField(p, label);

                if (EditorGUI.EndChangeCheck())
                    GetValue(property).InvokeCallbacks();
            }
            else
            {
                EditorGUILayout.LabelField(label, new GUIContent(NoDrawableLabel, NoDrawableTip));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => 0f;

        private static IEnumerable<SerializedProperty> GetInnerProperties(SerializedProperty property)
            => GetFieldNames(property)
            .Select(name => property.FindPropertyRelative(name))
            .Where(p => p != null) ??
            Array.Empty<SerializedProperty>();

        private static IEnumerable<string> GetFieldNames(SerializedProperty property)
            => GetValue(property)?
            .ValueFieldNames?
            .Where(name => !string.IsNullOrEmpty(name)) ??
            Array.Empty<string>();

        private static ObservableValue GetValue(SerializedProperty property)
        {
            var target = property.serializedObject.targetObject;
            var type = target.GetType();
            var field = type.GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field.GetValue(target) as ObservableValue;
        }
    }
#endif
}
