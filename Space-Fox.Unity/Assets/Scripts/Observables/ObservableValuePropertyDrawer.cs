using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SpaceFox
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ObservableValue), true)]
    public class ObservableValuePropertyDrawer : PropertyDrawer
    {
        private const string ValueFieldName = "TValue";
        private const string ValuePropertyName = "Value";
        private const string NoDrawableLabel = "No drawable";
        private const string NoDrawableTip = "Generic type is not serializable, or ObservableValue inheritor hasn't suitable fields";

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative(ValueFieldName);
            if (valueProperty == null)
            {
                EditorGUILayout.LabelField(label, new GUIContent(NoDrawableLabel, NoDrawableTip));
            }
            else
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(valueProperty, label);

                if (EditorGUI.EndChangeCheck())
                    SetValue(property, valueProperty.boxedValue);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => 0f;

        private static void SetValue(SerializedProperty property, object value)
        {
            var monoBehaviour = property.serializedObject.targetObject;
            var monoBehaviourType = monoBehaviour.GetType();
            var observableField = monoBehaviourType.GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var observableValue = observableField.GetValue(monoBehaviour);
            var observableType = observableValue.GetType();
            var observableValueProperty = observableType.GetProperty(ValuePropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
            observableValueProperty.SetValue(observableValue, value);
        }
    }
#endif
}
