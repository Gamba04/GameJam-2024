using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField]
    private string movementAxis;
    [SerializeField]
    private string jumpButton;
    [SerializeField]
    private string ballButton;

    [Header("Settings")]
    [SerializeField]
    private AnimationCurve joystickCorrection = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    [Header("Info")]
    [ReadOnly, SerializeField]
    private int playerID;

    public event Action<float> onMovement;
    public event Action onJump;
    public event Action onBall;

    #region Init

    public void Init(int playerID)
    {
        this.playerID = playerID;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Input

    #region Physics

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        float input = Input.GetAxisRaw(GetInput(movementAxis));
        float magnitude = Math.Abs(input);
        float direction = Math.Sign(input);

        onMovement?.Invoke(joystickCorrection.Evaluate(magnitude) * direction);
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
        CheckButton(ballButton, onBall);
    }

    #endregion

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void CheckButton(string button, Action action)
    {
        if (Input.GetButtonDown(GetInput(button)))
        {
            action?.Invoke();
        }
    }

    private string GetInput(string input) => $"Player{playerID}_{input}";

    #endregion

}