using System;
using UnityEngine;
using UnityEngine.UI;

public class UIFade : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Image fade;

    [Header("Settings")]
    [SerializeField]
    private bool startWithFade = true;
    [SerializeField]
    private bool enableInteractionsAfterFade = true;

    [Space]
    [SerializeField]
    private Color color = Color.black;
    [SerializeField]
    private TransitionColor transition;

    public event Action onFinishFade;

    public bool IsOnTransition => transition.IsOnTransition;

    #region Init

    public void Init()
    {
        if (!startWithFade) return;

        UIManager.SetInteractions(false);

        SetFade(true, true);

        Action onFinishFade = null;
        if (enableInteractionsAfterFade) onFinishFade = () => UIManager.SetInteractions(true);

        SetFade(false, onFinishFade: onFinishFade);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Update

    private void Update()
    {
        transition.UpdateTransition(color => fade.color = color);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void SetFade(bool value, bool instant = false, Action onFinishFade = null)
    {
        // SetActive
        if (value) fade.gameObject.SetActive(true);
        else onFinishFade += () => fade.gameObject.SetActive(false);

        // Fade events
        onFinishFade += this.onFinishFade;

        // Transition
        Color targetColor = value ? color : color.GetAlpha(0);

        if (instant)
        {
            transition.value = targetColor;
            fade.color = targetColor;

            onFinishFade?.Invoke();
        }
        else
        {
            transition.StartTransition(targetColor, onFinishFade);
        }
    }

    #endregion

}