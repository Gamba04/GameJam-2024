using System;
using System.Collections.Generic;
using UnityEngine;

public class GplayUI : UIManager
{
    [Separator]
    [Header("Components")]
    [SerializeField]
    private HUDController hudController;
    [SerializeField]
    private PauseController pauseController;
    [SerializeField]
    private GameOver gameOver;

    #region Singleton Override

    public new static GplayUI Instance => GetSingletonOverride<GplayUI>();

    #endregion

    #region Start

    protected override void Init()
    {
        base.Init();

        Instance.pauseController.SetVisible(false, true);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    public static void OnSetPause(bool value)
    {
        Instance.hudController.SetVisible(!value);
        Instance.pauseController.SetVisible(value);
    }

    public static void GameOver(int playerID)
    {
        Instance.gameOver.Play(playerID);
    }

    #endregion

}