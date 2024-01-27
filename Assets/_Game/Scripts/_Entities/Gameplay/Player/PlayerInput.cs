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

    #region Input

    #region Physics

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        onMovement?.Invoke(Input.GetAxisRaw(movementAxis));
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Buttons

    private void Update()
    {
        CheckButtons();
    }

    private void CheckButtons()
    {
        CheckButton(jumpButton, onJump);
        CheckButton(toggleButton, onToggle);
    }

    #endregion

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