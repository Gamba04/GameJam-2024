using UnityEngine;
using UnityEngine.Events;

public class Checkbox : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private AdvancedButton button;

    [Header("Settings")]
    public bool value;
    [SerializeField]
    private Color disselectedColor = Color.white;
    [SerializeField]
    private Color selectedColor = Color.white;

    [Space]
    [SerializeField]
    private UnityEvent<bool> onToggleValue;

    public void ToggleValue()
    {
        SetValue(!value);

        onToggleValue?.Invoke(value);
    }

    public void SetValue(bool value)
    {
        this.value = value;

        AdvancedButton.TargetGraphic target = button.GetTargetGraphic(0);
        AdvancedButton.GraphicOptions option = new AdvancedButton.GraphicOptions()
        {
            color = value ? selectedColor : disselectedColor,
            localScale = Vector2.one,
        };

        target.disselected = option;
        target.highlighted = option;
        target.pressed     = option;

        button.RefreshState();
    }
}