using System;
using UnityEngine;
using UnityEditor;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class RequireInterfaceAttribute : PropertyAttribute
{
	public readonly Type targetType;

	public RequireInterfaceAttribute(Type type)
	{
		targetType = type;
	}
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
public class RequireInterfaceDrawer : PropertyDrawer
{
	private readonly Color errorColor = new Color(1, 0.3f, 0.3f);

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		RequireInterfaceAttribute requireInterface = attribute as RequireInterfaceAttribute;
		Type targetType = requireInterface.targetType;

		if (property.propertyType == SerializedPropertyType.ObjectReference && property.type == $"PPtr<${nameof(Component)}>")
		{
			UpdateLabel(label, targetType);
			UpdateDrag(position, targetType);
			UpdateValue(property, targetType);

			EditorGUI.PropertyField(position, property, label);
		}
		else
		{
			DisplayError(position, $"{property.displayName} must be of type Component");
		}
	}

    #region Process

    private void UpdateLabel(GUIContent label, Type targetType)
	{
		label.text += $" ({targetType.Name})";
	}

	private void UpdateDrag(Rect position, Type targetType)
	{
		if (!position.Contains(Event.current.mousePosition) || DragAndDrop.objectReferences.Length != 1) return;

		UnityEngine.Object value = DragAndDrop.objectReferences[0];

		if (!ValidateValue(ref value, targetType))
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
		}
	}

	private void UpdateValue(SerializedProperty property, Type targetType)
	{
		UnityEngine.Object value = property.objectReferenceValue;

		if (!ValidateValue(ref value, targetType))
		{
			property.objectReferenceValue = null;
		}
		else
		{
			property.objectReferenceValue = value;
		}
	}

	#endregion

	// ----------------------------------------------------------------------------------------------------------------------------

	#region Other

	private bool ValidateValue(ref UnityEngine.Object value, Type targetType)
	{
		if (value == null) return false;

		if (value is Component component)
		{
			if (!targetType.IsAssignableFrom(component.GetType()))
			{
				if (value is Transform transform)
				{
					Component targetValue = transform.GetComponent(targetType);

					if (!targetValue)
					{
						return false;
					}
					else
					{
						value = targetValue;
					}
				}
				else return false;
			}
		}

		return true;
	}

	private void DisplayError(Rect position, string error)
	{
		Color color = GUI.color;
		GUI.color = errorColor;

		EditorGUI.LabelField(position, error);

		GUI.color = color;
	}

    #endregion

}

#endif