using UnityEngine;

public abstract class ButtonTrigger : MonoBehaviour
{
    [ReadOnly(true), SerializeField]
    private Button button;

    #region Awake

    private void Awake()
    {
        InitEvents();
    }

    private void InitEvents()
    {
        if (button == null) return;

        button.onHover += OnHover;
        button.onDisselect += OnDisselect;
        button.onPress += OnPress;
        button.onButtonClick += OnClick;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Virtual Methods

    protected virtual void OnHover() { }

    protected virtual void OnDisselect() { }

    protected virtual void OnPress() { }

    protected virtual void OnClick() { }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (button == null) button = GetComponent<Button>();
    }

#endif

    #endregion

}