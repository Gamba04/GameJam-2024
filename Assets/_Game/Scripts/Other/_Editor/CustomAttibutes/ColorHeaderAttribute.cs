using System;
using UnityEngine;
using UnityEditor;

[AttributeUsage(AttributeTargets.Field)]
public class ColorHeaderAttribute : PropertyAttribute
{
    public readonly string text;
    public readonly Color color;

    public ColorHeaderAttribute(string text, float r, float g, float b, float a = 1)
    {
        this.text = text;
        color = new Color(r, g, b, a);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ColorHeaderAttribute))]
public class ColorHeaderDrawer : DecoratorDrawer
{
    public override float GetHeight() => base.GetHeight() + 9;

    public override void OnGUI(Rect position)
    {
        ColorHeaderAttribute colorHeader = attribute as ColorHeaderAttribute;

        position.x += 1;
        position.y += 11;

        GUIStyle style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = colorHeader.color;

        GUI.Label(position, colorHeader.text, style);
    }
}

#endif