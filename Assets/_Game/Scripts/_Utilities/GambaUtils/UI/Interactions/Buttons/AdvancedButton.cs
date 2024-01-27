using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdvancedButton : Button
{
    [Serializable]
    public class TargetGraphic
    {
        [SerializeField, HideInInspector] private string name;

        [Header("Components")]
        public Graphic targetGraphic;
        public SpriteRenderer targetSprite;

        [Header("Settings")]
        public GraphicOptions disselected;
        public GraphicOptions highlighted;
        public GraphicOptions pressed;

        [SerializeField]
        private float duration = 0.1f;
        [SerializeField]
        private AnimationCurve transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        private TargetSpace targetSpace;

        private Color lastColor;
        private Color targetColor;

        private Vector2 lastPosition;
        private Vector2 targetPosition;

        private Vector2 lastScale;
        private Vector2 targetScale;

        private float progress = 1;
        private bool unscaled;

        public Transform targetTransform;

        public bool IsInTransition => progress < 1;

        private Transform GetTargetTransform
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

        public void Initialize()
        {
            targetTransform = GetTargetTransform;

            targetGraphic = null;
            targetSprite = null;

            transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

            disselected.Initialize();
            highlighted.Initialize();
            pressed.Initialize();
        }

        #region States

        public void Reset()
        {
            if (targetTransform != null)
            {
                TargetColor = disselected.color;
                targetTransform.localPosition = disselected.localPosition;
                targetTransform.localScale = disselected.localScale;
            }
        }

        public void StateUpdate()
        {
            if (duration > 0)
            {
                if (IsInTransition)
                {
                    progress += unscaled ? Time.unscaledDeltaTime / duration : Time.deltaTime / duration;

                    if (progress > 1)
                    {
                        progress = 1;
                    }

                    float curveValue = transitionCurve.Evaluate(progress);

                    TargetColor = Color.Lerp(lastColor, targetColor, curveValue);
                    targetTransform.localPosition = Vector2.Lerp(lastPosition, targetPosition, curveValue);
                    targetTransform.localScale = Vector2.Lerp(lastScale, targetScale, curveValue);
                }
            }
        }

        public void ChangeState(ButtonState state)
        {
            if (targetTransform != null)
            {
                GraphicOptions options = disselected;
                switch (state)
                {
                    case ButtonState.Disselected:
                        options = disselected;
                        break;
                    case ButtonState.Highlighted:
                        options = highlighted;
                        break;
                    case ButtonState.Pressed:
                        options = pressed;
                        break;
                }

                unscaled = false;

                lastColor = TargetColor;
                targetColor = options.color;

                lastPosition = targetTransform.localPosition;
                targetPosition = options.localPosition;

                lastScale = targetTransform.localScale;
                targetScale = options.localScale;

                if (duration > 0)
                {
                    progress = 0;
                }
                else
                {
                    TargetColor = targetColor;
                    targetTransform.localPosition = targetPosition;
                    targetTransform.localScale = targetScale;

                    progress = 1;
                }
            }
        }

        public void ChangeColorUnscaled(ButtonState state)
        {
            if (targetTransform != null)
            {
                GraphicOptions options = disselected;
                switch (state)
                {
                    case ButtonState.Disselected:
                        options = disselected;
                        break;
                    case ButtonState.Highlighted:
                        options = highlighted;
                        break;
                    case ButtonState.Pressed:
                        options = pressed;
                        break;
                }

                unscaled = true;

                lastColor = TargetColor;
                targetColor = options.color;

                lastPosition = targetTransform.localPosition;
                targetPosition = options.localPosition;

                lastScale = targetTransform.localScale;
                targetScale = options.localScale;

                if (duration > 0)
                {
                    progress = 0;
                }
                else
                {
                    TargetColor = targetColor;
                    targetTransform.localPosition = targetPosition;
                    targetTransform.localScale = targetScale;

                    progress = 1;
                }
            }
        }

        #endregion

        #region Editor

        [SerializeField, HideInInspector]
        private Transform preTarget;

        private ButtonState preState;
        private GraphicOptions preDisselected;
        private GraphicOptions preHighlighted;
        private GraphicOptions prePressed;

        public void EditorUpdate(ButtonState state, TargetSpace targetSpace)
        {
            targetTransform = GetTargetTransform;

            this.targetSpace = targetSpace;

            if (targetTransform != null)
            {
                if (targetTransform != preTarget)
                {
                    disselected.CopyGraphic(TargetColor, targetTransform);
                    highlighted.CopyGraphic(TargetColor, targetTransform);
                    pressed.CopyGraphic(TargetColor, targetTransform);
                }

                if (disselected != preDisselected)
                {
                    TargetColor = disselected.color;
                    targetTransform.localPosition = disselected.localPosition;
                    targetTransform.localScale = disselected.localScale;
                }
                else if (highlighted != preHighlighted)
                {
                    TargetColor = highlighted.color;
                    targetTransform.localPosition = highlighted.localPosition;
                    targetTransform.localScale = highlighted.localScale;
                }
                else if (pressed != prePressed)
                {
                    TargetColor = pressed.color;
                    targetTransform.localPosition = pressed.localPosition;
                    targetTransform.localScale = pressed.localScale;
                }
                else if (state != preState)
                {
                    switch (state)
                    {
                        case ButtonState.Disselected:
                            TargetColor = disselected.color;
                            targetTransform.localPosition = disselected.localPosition;
                            targetTransform.localScale = disselected.localScale;
                            break;
                        case ButtonState.Highlighted:
                            TargetColor = highlighted.color;
                            targetTransform.localPosition = highlighted.localPosition;
                            targetTransform.localScale = highlighted.localScale;
                            break;
                        case ButtonState.Pressed:
                            TargetColor = pressed.color;
                            targetTransform.localPosition = pressed.localPosition;
                            targetTransform.localScale = pressed.localScale;
                            break;
                    }
                }

                preDisselected = disselected;
                preHighlighted = highlighted;
                prePressed = pressed;
                preState = state;

                preTarget = targetTransform;
            }
        }

        public void SetName()
        {
            string type = targetSpace == TargetSpace.UI ? targetGraphic != null ? targetGraphic.GetType().Name : "Graphic" : targetSprite != null ? targetSprite.GetType().Name : "Sprite Renderer";
            string obj = targetTransform != null ? targetTransform.gameObject.name : "None";

            name = $"{obj} ({type})";
        }

        #endregion

    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Serializable]
    public struct GraphicOptions
    {
        public Color color;
        public Vector2 localPosition;
        public Vector2 localScale;
        //public Sprite customSprite;

        public void Initialize()
        {
            color = new Color(1, 1, 1, 1);
            localScale = Vector2.one;
        }

        public void CopyGraphic(Color targetColor, Transform transform)
        {
            color = targetColor;
            localPosition = transform.localPosition;
            localScale = transform.localScale;
        }

        public static bool operator ==(GraphicOptions a, GraphicOptions b)
        {
            return (a.color == b.color && a.localPosition == b.localPosition && a.localScale == b.localScale);
        }

        public static bool operator !=(GraphicOptions a, GraphicOptions b)
        {
            return !(a == b);
        }

        public override bool Equals(object a)
        {
            return this == (GraphicOptions)a;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private List<TargetGraphic> targetGraphics = new List<TargetGraphic>();

    [Space()]

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

    #region Overrides

    protected override void OnStart()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            t.EditorUpdate(state, targetSpace);
            t.Reset();
        }

        base.OnStart();
    }

    protected override void EditorUpdate()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            t.EditorUpdate(state, targetSpace);
            t.SetName();
        }
    }

    protected override void RuntimeUpdate()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            if (t.targetTransform != null && t.targetTransform.gameObject.activeInHierarchy)
            {
                t.StateUpdate();
            }
        }
    }

    protected override void OnDisselect()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            t.ChangeColorUnscaled(ButtonState.Disselected);
        }
    }

    protected override void OnHighlight()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            t.ChangeColorUnscaled(ButtonState.Highlighted);
        }
    }

    protected override void OnPressed()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            t.ChangeColorUnscaled(ButtonState.Pressed);
        }
    }

    protected override void ResetState()
    {
        foreach (TargetGraphic t in targetGraphics) t.Reset();
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public TargetGraphic GetTargetGraphic(int index)
    {
        if (index < targetGraphics.Count)
        {
            return targetGraphics[index];
        }

        Debug.LogError($"Index {index} is out of range ({targetGraphics.Count})");
        return null;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Editor

    [SerializeField, HideInInspector]
    private int lastTargetGraphicsLenght;

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (lastTargetGraphicsLenght < targetGraphics.Count)
            {
                for (int i = lastTargetGraphicsLenght; i < targetGraphics.Count; i++)
                {
                    targetGraphics[i].Initialize();
                }
            }

            lastTargetGraphicsLenght = targetGraphics.Count;
        }
    }

#endif

    #endregion

}