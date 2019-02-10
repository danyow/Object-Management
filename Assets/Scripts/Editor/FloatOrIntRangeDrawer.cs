using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatRange)), CustomPropertyDrawer(typeof(IntRange))]
public class FloatOrIntRangeDrawer: PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        // 保存两个值 最后面用来恢复
        int   originalIndentLevel = EditorGUI.indentLevel;
        float originalLabelWidth  = EditorGUIUtility.labelWidth;

        // 开始
        EditorGUI.BeginProperty(position, label, property);

        // 先把label放好 得到剩余的空间 GetControlID加入了之后 label不会变的蓝色了
        // position = EditorGUI.PrefixLabel(position, label);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        // 因为要放两组 所以要除以2
        position.width = position.width / 2f;
        // 覆盖原先的宽度 不然这个宽度会包含label的宽度
        EditorGUIUtility.labelWidth = position.width / 2f;
        // 蓝色突出显示？
        EditorGUI.indentLevel = 1;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("min"));
        position.x += position.width;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("max"));
        EditorGUI.EndProperty();

        // 恢复
        EditorGUI.indentLevel       = originalIndentLevel;
        EditorGUIUtility.labelWidth = originalLabelWidth;
    }
}