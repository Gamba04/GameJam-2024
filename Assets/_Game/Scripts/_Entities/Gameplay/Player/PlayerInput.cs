using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private string movementAxis;
    [SerializeField]
    private string jumpButton;
    [SerializeField]
    private string toggleButton;
    
    public event Action<float> onMovement;
    public event Action onJump;
    public event Action onToggle;

    #region Update

    private void Update()
    {
        CheckMovement();
        CheckButtons();
    }

    private void CheckMovement()
    {
        onMovement?.Invoke(Input.GetAxisRaw(movementAxis));
    }

    private void CheckButtons()
    {
        CheckButton(jumpButton, onJump);
        CheckButton(toggleButton, onToggle);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void CheckButton(string button, Action action)
    {
        if (Input.GetButtonDown(button))
        {
            action?.Invoke();
        }
    }

    #endregion

}