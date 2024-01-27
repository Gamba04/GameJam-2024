using System;
using UnityEngine;
using UnityEditor;

/// <summary> Disable or enable editing of the field from the inspector. </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ReadOnlyAttribute : PropertyAttribute
{
    private bool enabled;
    private readonly bool canEdit;

    public ReadOnlyAttribute(bool canEdit = false)
    {
        this.canEdit = canEdit;

        order = int.MaxValue;
    }

    public bool Enabled => enabled;

    public bool CanEdit => canEdit;

    public void ToggleEnabled() => enabled = !enabled;
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : DecoratorDrawer
{
    public override float GetHeight() => 0;

    public override void OnGUI(Rect position)
    {
        ReadOnlyAttribute readOnly = attribute as ReadOnlyAttribute;

        if (readOnly.CanEdit)
        {
            Rect buttonRect = position;
            buttonRect.x -= 16;
            buttonRect.y += 1;
            buttonRect.width = 16;
            buttonRect.height = 17;

            if (GUI.Button(buttonRect, "", GUIStyle.none)) readOnly.ToggleEnabled();

            bool enabled = readOnly.Enabled;

            GUI.Label(buttonRect, new GUIContent(enabled ? "✎" : "✐", "ReadOnly made by Gamba"));

            GUI.enabled = enabled;
        }
        else
        {
            GUI.enabled = false;
        }
    }
}

#endif