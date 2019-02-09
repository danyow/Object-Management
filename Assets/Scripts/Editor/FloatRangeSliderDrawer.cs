using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatRangeSliderAttribute))]
public class FloatRangeSliderDrawer: PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        int originalIndentLevel = EditorGUI.indentLevel;

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        EditorGUI.indentLevel = 0;
        SerializedProperty minProperty = property.FindPropertyRelative("min");
        SerializedProperty maxProperty = property.FindPropertyRelative("max");

        // 防止被更改 因为unity 负责支持撤销和重做
        float minValue = minProperty.floatValue;
        float maxValue = maxProperty.floatValue;

        float fieldWidth = position.width / 4f - 4f;
        float sliderWidth = position.width / 2f;

        position.width = fieldWidth;
        minValue = EditorGUI.FloatField(position, minValue);
        position.x += fieldWidth + 4f;
        position.width = sliderWidth;
        
        FloatRangeSliderAttribute limit = attribute as FloatRangeSliderAttribute;
        // 这里没有必要显示label 故删除
        EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, limit.Min, limit.Max);
        position.x += sliderWidth + 4f;
        position.width = fieldWidth;

        maxValue = EditorGUI.FloatField(position, maxValue);

        if (minValue < limit.Min) {
            minValue = limit.Min;
        }
        if (maxValue < minValue) {
            maxValue = minValue;
        } else if (minValue > limit.Max) {
            maxValue = limit.Max;
        }

        minProperty.floatValue = minValue;
        maxProperty.floatValue = maxValue;
        EditorGUI.EndProperty();
        EditorGUI.indentLevel = originalIndentLevel;
    }
}