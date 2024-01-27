using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public abstract class Button : MonoBehaviour
{
    [Serializable]
    protected class AdvancedSettings
    {
        public DetectionType detectionType;
        public RectTransform rectDetection;
        public Collider2D colliderDetection;
    }

    public enum TargetSpace
    {
        UI,
        SpriteRenderer
    }

    public enum ButtonState
    {
        Disselected,
        Highlighted,
        Pressed
    }

    protected enum DetectionType
    {
        Rect,
        Collider
    }

    #region ButtonLowerLayout

    protected virtual ButtonState NewState { get; set; }
    public virtual UnityEvent NewOnClick { get; set; }
    protected virtual AdvancedSettings NewAdvancedSettings { get; set; }

    #endregion

    [HideInInspector]
    public bool pressed;

    public bool interactable = true;
    public TargetSpace targetSpace;

    [Space()]

    private int currentTouchId = -1;

    protected Canvas canvas;

    public event Action onHover;
    public event Action onPress;
    public event Action onDisselect;
    public event Action onButtonClick;

    public static event Action<Button> onStaticHover;
    public static event Action<Button> onStaticPress;
    public static event Action<Button> onStaticDisselect;
    public static event Action<Button> onStaticClick;

    public static bool Interactable = true;

    public ButtonState CurrentState => NewState;

    private void Start()
    {
        canvas = UIManager.Canvas;

        OnStart();
    }

    protected virtual void OnStart()
    {
        if (targetSpace == TargetSpace.SpriteRenderer)
        {
            NewAdvancedSettings.detectionType = DetectionType.Collider;
        }

        ForceChangeState(ButtonState.Disselected);
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (NewAdvancedSettings != null)
            {
                if (Interactable && interactable)
                {
                    CheckInteraction();
                }
                else
                {
                    ChangeState(ButtonState.Disselected);
                    pressed = false;
                }
            }

            RuntimeUpdate();
        }
        else
        {
            if (NewAdvancedSettings != null)
            {
                if (NewAdvancedSettings.rectDetection == null)
                {
                    NewAdvancedSettings.rectDetection = GetComponent<RectTransform>();
                }

                if (NewAdvancedSettings.colliderDetection == null)
                {
                    NewAdvancedSettings.colliderDetection = GetComponent<Collider2D>();
                }
            }

            EditorUpdate();
        }
    }

    #region StateControl

    private void CheckInteraction()
    {
        // MOUSE INTERACTION
        if (!Input.multiTouchEnabled || Input.mousePresent)
        {
            bool over = InputOver(Input.mousePosition);

            if (!pressed) // Not Pressed
            {
                if (over) // Over
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ChangeState(ButtonState.Pressed);
                        pressed = true;
                    }
                    else // Highlighted
                    {
                        ChangeState(ButtonState.Highlighted);
                    }
                }
                else // Outside
                {
                    ChangeState(ButtonState.Disselected);
                }
            }
            else // Button Pressed
            {
                if (over)
                {
                    if (Input.GetMouseButtonUp(0)) // OnClick
                    {
                        OnClick();

                        pressed = false;
                    }
                }
                else
                {
                    pressed = false;
                }
            }
        }

        // TOUCH INTERACTION
        if (Input.multiTouchEnabled)
        {
            if (!pressed) // Not Pressed
            {
                if (TouchPress())
                {
                    ChangeState(ButtonState.Pressed);
                    pressed = true;
                }
                else if (!Input.mousePresent)
                {
                    ChangeState(ButtonState.Disselected);
                }
            }
            else // Button Pressed
            {
                if (TouchRelease())
                {
                    pressed = false;
                }
                else if (TouchClick())
                {
                    OnClick();

                    pressed = false;
                }
            }
        }
    }

    private void ChangeState(ButtonState state)
    {
        if (state != NewState)
        {
            NewState = state;

            switch (state)
            {
                case ButtonState.Disselected:
                    onDisselect?.Invoke();
                    onStaticDisselect?.Invoke(this);
                    OnDisselect();
                    break;
                case ButtonState.Highlighted:
                    onHover?.Invoke();
                    onStaticHover?.Invoke(this);
                    OnHighlight();
                    break;
                case ButtonState.Pressed:
                    onPress?.Invoke();
                    onStaticPress?.Invoke(this);
                    OnPressed();
                    break;
            }
        }
    }

    private void ForceChangeState(ButtonState state)
    {
        NewState = state;

        switch (state)
        {
            case ButtonState.Disselected:
                OnDisselect();
                break;
            case ButtonState.Highlighted:
                OnHighlight();
                break;
            case ButtonState.Pressed:
                OnPressed();
                break;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Virtual Methods

    protected virtual void OnClick()
    {
        onStaticClick?.Invoke(this);
        onButtonClick?.Invoke();

        NewOnClick?.Invoke();
    }

    protected virtual void OnDisselect() { }

    protected virtual void OnHighlight() { }

    protected virtual void OnPressed() { }

    protected virtual void EditorUpdate() { }

    protected virtual void RuntimeUpdate() { }

    protected virtual void ResetState() { }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void SetDetectionSize(float? width, float? height)
    {
        switch (NewAdvancedSettings.detectionType)
        {
            case DetectionType.Collider:
                if (NewAdvancedSettings.colliderDetection != null)
                {
                    BoxCollider2D box = NewAdvancedSettings.colliderDetection as BoxCollider2D;

                    if (box != null)
                    {
                        float newWidth = box.size.x;
                        float newHeight = box.size.y;

                        if (width.HasValue)
                        {
                            newWidth = width.Value;
                        }

                        if (height.HasValue)
                        {
                            newHeight = height.Value;
                        }

                        box.size = new Vector2(newWidth, newHeight);
                    }
                }
                break;
            case DetectionType.Rect:
                if (NewAdvancedSettings.rectDetection != null)
                {
                    float newWidth = NewAdvancedSettings.rectDetection.sizeDelta.x;
                    float newHeight = NewAdvancedSettings.rectDetection.sizeDelta.y;

                    if (width.HasValue)
                    {
                        newWidth = width.Value;
                    }

                    if (height.HasValue)
                    {
                        newHeight = height.Value;
                    }

                    NewAdvancedSettings.rectDetection.sizeDelta = new Vector2(newWidth, newHeight);
                }
                break;
        }
    }

    public void RefreshState()
    {
        ForceChangeState(NewState);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    /// <param name="position"> Screen coordinates. </param>
    private bool InputOver(Vector2 position)
    {
        bool over = false;

        var advancedSettings = NewAdvancedSettings;

        switch (advancedSettings.detectionType)
        {
            case DetectionType.Rect:
                if (advancedSettings.rectDetection != null)
                {
                    Vector2 pos = (position - CanvasToScreenPos((Vector2)advancedSettings.rectDetection.position)) / canvas.scaleFactor;

                    Vector3 rectLossy = advancedSettings.rectDetection.lossyScale;
                    Vector3 canvasLossy = canvas.transform.lossyScale;

                    Vector2 scaledPos = new Vector2(pos.x / (rectLossy.x / canvasLossy.x), pos.y / (rectLossy.y / canvasLossy.y));

                    over = advancedSettings.rectDetection.rect.Contains(scaledPos);
                }
                break;
            case DetectionType.Collider:
                if (advancedSettings.colliderDetection != null)
                {
                    switch (targetSpace)
                    {
                        case TargetSpace.UI:
                            over = advancedSettings.colliderDetection.OverlapPoint(ScreenToCanvasPos(position));
                            break;
                        case TargetSpace.SpriteRenderer:
                            if (Camera.main != null)
                            {
                                over = advancedSettings.colliderDetection.OverlapPoint(Camera.main.ScreenToWorldPoint(position));
                            }
                            break;
                    }
                }
                break;
        }

        return over;
    }

    private bool TouchPress()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                if (InputOver(touch.position))
                {
                    currentTouchId = touch.fingerId;
                    return true;
                }
            }
        }

        return false;
    }

    private bool TouchRelease()
    {
        if (currentTouchId != -1)
        {
            Touch touch = new List<Touch>(Input.touches).Find(f => f.fingerId == currentTouchId);

            if (touch.phase == TouchPhase.Canceled || !InputOver(touch.position))
            {
                currentTouchId = -1;
                return true;
            }
        }

        return false;
    }

    private bool TouchClick()
    {
        if (currentTouchId != -1)
        {
            Touch touch = new List<Touch>(Input.touches).Find(f => f.fingerId == currentTouchId);

            if (touch.phase == TouchPhase.Canceled)
            {
                currentTouchId = -1;
                return false;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                currentTouchId = -1;

                if (InputOver(touch.position))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private Vector2 ScreenToCanvasPos(Vector3 position)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector3 newPos = canvas.worldCamera.ScreenToWorldPoint(position);
            newPos.z = canvas.transform.position.z;

            return newPos;
        }
        else
        {
            return position;
        }
    }

    private Vector2 CanvasToScreenPos(Vector3 position)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            return canvas.worldCamera.WorldToScreenPoint(position);
        }
        else
        {
            return position;
        }
    }

    private void OnEnable()
    {
        ResetState();
    }

    #endregion

}