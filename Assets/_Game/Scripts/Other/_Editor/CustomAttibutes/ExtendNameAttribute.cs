using System;
using UnityEngine;
using UnityEditor;

/// <summary> Add extra information to the field name. </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ExtendNameAttribute : PropertyAttribute
{
    public readonly string text;

    public ExtendNameAttribute(string text) => this.text = text;
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ExtendNameAttribute))]
public class ExtendNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ExtendNameAttribute extendName = attribute as ExtendNameAttribute;

        label.text += extendName.text;

        EditorGUI.PropertyField(position, property, label);
    }
}

#endif