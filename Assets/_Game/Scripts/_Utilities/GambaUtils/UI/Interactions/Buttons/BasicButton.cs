using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BasicButton : Button
{
    [Header("Components")]
    [SerializeField]
    private Graphic targetGraphic;
    [SerializeField]
    private SpriteRenderer targetSprite;

    [Header("Settings")]
    [SerializeField]
    private Color color_Disselected = new Color(1, 1, 1, 1);
    [SerializeField]
    private Color color_Highlighted = new Color(1, 1, 1, 1);
    [SerializeField]
    private Color color_Pressed = new Color(1, 1, 1, 1);
    [SerializeField]
    private float duration;
    [SerializeField]
    private AnimationCurve transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private Color lastColor;
    private Color targetColor;
    private float progress;
    private bool unscaled;

    public Transform TargetTransform
    {
        get
        {
            switch (targetSpace)
            {
                case TargetSpace.UI:
                    if (targetGraphic) return targetGraphic.transform;
                    break;
                case TargetSpace.SpriteRenderer:
                    if (targetSprite) return targetSprite.transform;
                    break;
            }

            return null;
        }
    }

    public Color TargetColor
    {
        get
        {
            switch (targetSpace)
            {
                case TargetSpace.UI:
                    if (targetGraphic) return targetGraphic.color;
                    break;
                case TargetSpace.SpriteRenderer:
                    if (targetSprite) return targetSprite.color;
                    break;
            }

            return default;
        }

        set
        {
            switch (targetSpace)
            {
                case TargetSpace.UI:
                    if (targetGraphic) targetGraphic.color = value;
                    break;
                case TargetSpace.SpriteRenderer:
                    if (targetSprite) targetSprite.color = value;
                    break;
            }
        }
    }

    #region ButtonLowerLayout

    [SerializeField]
    private ButtonState state;
    [Space()]
    [SerializeField]
    private UnityEvent onClick;
    [Space()]
    [SerializeField]
    private AdvancedSettings advancedSettings;

    protected override ButtonState NewState { get => state; set => state = value; }
    public override UnityEvent NewOnClick { get => onClick; set => onClick = value; }
    protected override AdvancedSettings NewAdvancedSettings { get => advancedSettings; set => advancedSettings = value; }

    #endregion

    protected override void OnStart()
    {
        base.OnStart();

        if (TargetTransform != null)
        {
            TargetColor = color_Disselected;
        }
    }

    #region Overrides

    protected override void EditorUpdate()
    {
        if (TargetTransform == null)
        {
            targetGraphic = GetComponent<Graphic>();
            targetSprite = GetComponent<SpriteRenderer>();
        }
    }

    protected override void RuntimeUpdate()
    {
        if (TargetTransform != null)
        {
            ColorUpdate();
        }
    }

    protected override void OnDisselect()
    {
        ChangeColor(color_Disselected, true);
    }

    protected override void OnHighlight()
    {
        ChangeColor(color_Highlighted, true);
    }

    protected override void OnPressed()
    {
        ChangeColor(color_Pressed, true);
    }

    protected override void ResetState()
    {
        TargetColor = color_Disselected;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void ColorUpdate()
    {
        if (duration > 0)
        {
            if (progress < 1)
            {
                progress += unscaled ? Time.unscaledDeltaTime / duration : Time.deltaTime / duration;

                if (progress > 1)
                {
                    progress = 1;
                }
            }

            TargetColor = Color.Lerp(lastColor, targetColor, transitionCurve.Evaluate(progress));
        }
    }

    private void ChangeColor(Color newColor, bool unscaled = false)
    {
        this.unscaled = unscaled;

        if (TargetTransform != null)
        {
            lastColor = TargetColor;
            targetColor = newColor;

            if (duration > 0)
            {
                progress = 0;
            }
            else
            {
                TargetColor = newColor;
                progress = 1;
            }
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Editor

    // Show last modified color on scene or direct selection
#if UNITY_EDITOR

    private Color preDiss = new Color(1, 1, 1, 1);
    private Color preHigh = new Color(1, 1, 1, 1);
    private Color prePress = new Color(1, 1, 1, 1);
    private ButtonState preState;

    private void OnValidate()
    {
        if (TargetTransform != null)
        {
            if (color_Disselected != preDiss)
            {
                TargetColor = color_Disselected;
            }
            else if (color_Highlighted != preHigh)
            {
                TargetColor = color_Highlighted;
            }
            else if (color_Pressed != prePress)
            {
                TargetColor = color_Pressed;
            }
            else if (state != preState)
            {
                switch (state)
                {
                    case ButtonState.Disselected:
                        TargetColor = color_Disselected;
                        break;
                    case ButtonState.Highlighted:
                        TargetColor = color_Highlighted;
                        break;
                    case ButtonState.Pressed:
                        TargetColor = color_Pressed;
                        break;
                }
            }

            preDiss = color_Disselected;
            preHigh = color_Highlighted;
            prePress = color_Pressed;
            preState = state;
        }
    }

#endif

    #endregion

}