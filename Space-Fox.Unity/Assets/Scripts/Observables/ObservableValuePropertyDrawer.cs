using System.Reflection;
using ModestTree;
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

                if (TryGetSlider(property, out var slider))
                {
                    if (valueProperty.propertyType == SerializedPropertyType.Integer)
                    {
                        EditorGUILayout.IntSlider(valueProperty, slider.MinInt, slider.MaxInt, label);
                    }
                    else if (valueProperty.propertyType == SerializedPropertyType.Float)
                    {
                        EditorGUILayout.Slider(valueProperty, slider.Min, slider.Max, label);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(valueProperty, label);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(valueProperty, label);
                }

                if (EditorGUI.EndChangeCheck())
                    SetValue(property, valueProperty.boxedValue);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => 0f;

        private static bool TryGetSlider(SerializedProperty property, out SliderAttribute slider)
        {
            var monoBehaviour = property.serializedObject.targetObject;
            var observableField = GetObservableField(property, monoBehaviour);

            var hasAttribute = observableField.HasAttribute<SliderAttribute>();
            slider = hasAttribute ? observableField.GetAttribute<SliderAttribute>() : default;
            return hasAttribute;
        }

        private static void SetValue(SerializedProperty property, object value)
        {
            var monoBehaviour = property.serializedObject.targetObject;
            var observableField = GetObservableField(property, monoBehaviour);

            var observableValue = observableField.GetValue(monoBehaviour);
            var observableType = observableValue.GetType();
            var observableValueProperty = observableType.GetProperty(ValuePropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
            observableValueProperty.SetValue(observableValue, value);
        }

        private static FieldInfo GetObservableField(SerializedProperty property, Object monoBehaviour)
        {
            var monoBehaviourType = monoBehaviour.GetType();
            var observableField = monoBehaviourType.GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return observableField;
        }
    }
#endif
}
