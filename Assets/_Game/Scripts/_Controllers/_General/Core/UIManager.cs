using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UIManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    protected Canvas canvas;
    [SerializeField]
    protected EventSystem eventSystem;
    [SerializeField]
    protected UIFade fade;
    [SerializeField]
    private PerformanceMetrics metrics;

    public static event Action onFinishFade;

    public static Canvas Canvas => Instance.canvas;

    public static bool IsOnTransition => Instance.fade.IsOnTransition;

    #region Init

    #region Singleton

    protected static UIManager instance;

    public static UIManager Instance => GambaFunctions.GetSingleton(ref instance);

    protected static T GetSingletonOverride<T>()
        where T : UIManager
    {
        T instance = UIManager.instance as T;

        return GambaFunctions.GetSingleton(ref instance);
    }

    private void Awake() => GambaFunctions.OnSingletonInit(ref instance, this);

    #endregion

    private void Start() => Init();

    protected virtual void Init()
    {
        InitEvents();

        fade.Init();
    }

    private void InitEvents()
    {
        fade.onFinishFade += OnFinishFade;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Virtual Methods

    public virtual void OnSetInteractions(bool enabled)
    {
        Button.Interactable = enabled;

        if (eventSystem) eventSystem.enabled = enabled;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    #region General

    public static Vector3 ScreenToCanvasPos(Vector2 position)
    {
        Canvas canvas = Canvas;

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

    public static Vector2 CanvasToScreenPos(Vector3 position)
    {
        Canvas canvas = Canvas;

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            return canvas.worldCamera.WorldToScreenPoint(position);
        }
        else
        {
            return position;
        }
    }

    public static Vector3 ScreenToCanvasVector(Vector2 vector)
    {
        Canvas canvas = Canvas;

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 newVec = vector / Screen.height * canvas.worldCamera.orthographicSize * 2;

            return newVec;
        }
        else
        {
            return vector;
        }
    }

    public static void SetInteractions(bool enabled) => Instance.OnSetInteractions(enabled);

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Fade

    public static void SetFade(bool value) => SetFadeInternal(value);

    public static void SetFade(bool value, bool instant) => SetFadeInternal(value, instant);

    public static void SetFade(bool value, Action onFinishFade) => SetFadeInternal(value, onFinishFade: onFinishFade);

    public static void SetFade(bool value, bool instant, Action onFinishFade) => SetFadeInternal(value, instant, onFinishFade);

    private static void SetFadeInternal(bool value, bool instant = false, Action onFinishFade = null) => Instance.fade.SetFade(value, instant, onFinishFade);

    private static void OnFinishFade()
    {
        onFinishFade?.Invoke();
        onFinishFade = null;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Metrics

    public static PerformanceOptions GetPerformanceMetrics() => Instance.metrics.GetOptions();

    public static void SetPerformanceMetrics(PerformanceOptions options) => Instance.metrics.SetOptions(options);

    #endregion

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    // Reset static events
    private void OnDestroy()
    {
        onFinishFade = null;
    }

    #endregion

}