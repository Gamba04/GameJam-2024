using System;
using UnityEngine;

public class CigaretteOverlay : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Transform overlay;
    [SerializeField]
    private Animator boxAnim;

    [Header("Settings")]
    [SerializeField]
    private float delay;
    [SerializeField]
    private MultiTransition<Vector2, float> overlayTransition;
    [SerializeField]
    [Range(0, 1)]
    private float targetSize;

    private Player startingPlayer;

    public event Action onFinishSequence;

    #region Public Methods

    public void StartSequence(Player startingPlayer)
    {
        this.startingPlayer = startingPlayer;

        Timer.CallOnDelay(StartTransition, delay);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Update

    private void Update()
    {
        UpdateTransition();   
    }

    private void UpdateTransition()
    {
        overlayTransition.UpdateTransition(OnOverlayTransition);
    }

    private void OnOverlayTransition(Vector2 position, float size)
    {
        overlay.position = position;
        overlay.localScale = GambaFunctions.GetScaleOf(size);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void StartTransition()
    {
        overlayTransition.SetValues(overlay.position, overlay.localScale.x);
        overlayTransition.StartTransition(startingPlayer.transform.position, targetSize, OnFinishSequence);
    }

    private void OnFinishSequence()
    {
        overlay.gameObject.SetActive(false);
        boxAnim.SetTrigger("Out");

        onFinishSequence?.Invoke();
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        UpdateEditorFields();   
    }

    private void UpdateEditorFields()
    {
        GambaFunctions.RestrictNegativeValues(ref delay);
    }

#endif

    #endregion

}