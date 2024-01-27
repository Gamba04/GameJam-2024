using System;
using UnityEngine;
using UnityEditor;

public class ScrollController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private TouchDetector touchDetector;

    [Header("Settings")]
    [SerializeField]
    [Range(0, 360)]
    private float angle;
    [SerializeField]
    private float speed;
    [SerializeField]
    [Range(0, 1)]
    private float inertiaSmoothing;
    [SerializeField]
    private float friction;
    [SerializeField]
    [Range(0, 1)]
    private float boundsResistence;

    [Header("Info")]
    [ReadOnly, SerializeField]
    private bool active;
    [ReadOnly, SerializeField]
    private bool scrolling;
    [ReadOnly, SerializeField]
    private float scrollValue;
    [ReadOnly, SerializeField]
    private float inertia;

    private Vector2 scrollDirection;

    private Vector2 startPosition;
    private float startValue;

    private float minValue = Mathf.NegativeInfinity;
    private float maxValue = Mathf.Infinity;

    public event Action onScrollStart;
    public event Action<float> onScrollUpdate;
    public event Action onScrollEnd;

    public bool Active => active;

    public bool Scrolling => scrolling;

    public float Value => scrollValue;

    public float StartValue => startValue;

    public float Inertia => inertia;

    #region Init

    public void Init()
    {
        InitEvents();

        Setup();
    }

    private void InitEvents()
    {
        touchDetector.onTouch += OnTouchInteraction;
    }

    private void Setup()
    {
        scrollDirection = GetDirection();
    }

    private Vector2 GetDirection()
    {
        float rads = angle * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rads), Mathf.Sin(rads));
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Update

    private void Update()
    {
        InertiaUpdate();
    }

    private void InertiaUpdate()
    {
        if (scrolling || inertia == 0) return;

        SetValue(scrollValue + inertia);

        inertia *= Mathf.Min(1 - (Time.deltaTime * friction), 1);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void SetActive(bool value)
    {
        active = value;
    }

    public void Stop()
    {
        inertia = 0;
    }

    public void SetValue(float value)
    {
        scrollValue = value;

        float clampedValue = Mathf.Clamp(value, minValue, maxValue);

        if (value != clampedValue)
        {
            float distance = Mathf.Abs(value - clampedValue);

            value = Mathf.Lerp(value, clampedValue, 1 - 1 / (1 + distance));
        }

        onScrollUpdate?.Invoke(value);
    }

    public void SetLimits(float minValue, float maxValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

    public bool IsInRange(float value) => value >= minValue && value <= maxValue;

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Interactions

    private void OnTouchInteraction(GameTouch touch, bool isOnArea, bool beganOnArea)
    {
        if (!active) return;

        switch (touch.state)
        {
            case InputState.Start:
                if (!scrolling)
                {
                    StartScroll(touch.screenPos);
                }

                break;

            case InputState.Stay:
                if (scrolling)
                {
                    UpdateScroll(touch.screenPos, touch.deltaPos);
                }

                break;

            case InputState.End:
            case InputState.Canceled:
                if (scrolling)
                {
                    EndScroll();
                }

                break;
        }
    }

    private void StartScroll(Vector2 position)
    {
        onScrollStart?.Invoke();

        scrolling = true;

        startPosition = position;
        startValue = scrollValue;

        inertia = 0;
    }

    private void UpdateScroll(Vector2 position, Vector2 deltaPos)
    {
        Vector2 displacement = startPosition - position;

        float scrollMovement = GetValue(displacement);

        SetValue(startValue + scrollMovement);

        float targetIntertia = GetValue(-deltaPos);
        inertia = inertiaSmoothing < 1 ? Mathf.Lerp(inertia, targetIntertia, 1 - inertiaSmoothing) : targetIntertia;
    }

    private void EndScroll()
    {
        onScrollEnd?.Invoke();

        scrolling = false;

        startPosition = default;
        startValue = default;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private float GetValue(Vector2 displacement)
    {
        displacement = UIManager.ScreenToCanvasVector(displacement);

        return Vector2.Dot(displacement, scrollDirection) * speed;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    #region OnDrawGizmos

    private void OnDrawGizmosSelected()
    {
        DrawDirectionArrow();
    }

    private void DrawDirectionArrow()
    {
        Vector2 direction = GetDirection();

        Handles.color = Color.yellow;
        Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(direction), 1, EventType.Repaint);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region OnValidate

    private void OnValidate()
    {
        BlockNegativeValues(ref speed);
        BlockNegativeValues(ref friction);
        BlockNegativeValues(ref boundsResistence);
    }

    private void BlockNegativeValues(ref float value) => value = Mathf.Max(value, 0);

    #endregion

#endif

    #endregion

}