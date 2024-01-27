using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private PlayerInput input;

    #region Init

    public void Init()
    {
        InitEvents();
    }

    private void InitEvents()
    {
        input.onMovement += Move;
        input.onJump += Jump;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Mechanics

    #region Movement

    private void Move(float input)
    {
        Debug.Log(input);
    }

    private void Jump()
    {
        Debug.Log("Jump");
    }

    #endregion

    #endregion

}