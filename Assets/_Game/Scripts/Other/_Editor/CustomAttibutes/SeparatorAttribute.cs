using System;
using UnityEngine;
using UnityEditor;

/// <summary> Creates a separator line in the inspector. </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SeparatorAttribute : PropertyAttribute
{
    public SeparatorAttribute() => order = int.MinValue + 1;
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(SeparatorAttribute))]
public class SeparatorDrawer : DecoratorDrawer
{
    private const float thickness = 1;

    private readonly Color color = new Color(0.1f, 0.1f, 0.1f);

    public override float GetHeight() => base.GetHeight() * 2;

    public override void OnGUI(Rect position)
    {
        position.y += (GetHeight() - thickness) / 2;
        position.height = thickness;

        EditorGUI.DrawRect(position, color);
    }
}

#endif