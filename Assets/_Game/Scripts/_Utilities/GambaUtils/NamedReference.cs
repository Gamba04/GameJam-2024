using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public class NamedReference<T>
{
    [SerializeField, HideInInspector] private string name;

    public T reference;

    public void SetName(object name) => this.name = name.ToString();

    public void SetName(Func<T, object> nameFunction)
    {
        if (nameFunction == null) return;

        name = nameFunction(reference).ToString();
    }
}

#if UNITY_EDITOR

namespace Editor
{
    [CustomPropertyDrawer(typeof(NamedReference<>))]
    public class NamedReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(position, property.FindPropertyRelative("reference"), label);

            EditorGUI.EndProperty();
        }
    }
}

#endif