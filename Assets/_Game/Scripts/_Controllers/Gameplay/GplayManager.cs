using System;
using System.Collections.Generic;
using UnityEngine;

public class GplayManager : MonoBehaviour
{
    [SerializeField]
    private bool debugs = true;

    [Header("Components")]
    [SerializeField]
    private PlayersManager playersManager;

    public static bool Debugs => instance != null ? instance.debugs : true;

    public static bool GamePaused { get; private set; }

    #region Singleton

    private static GplayManager instance;

    public static GplayManager Instance => GambaFunctions.GetSingleton(ref instance);

    private void Awake() => GambaFunctions.OnSingletonInit(ref instance, this);

    #endregion

    #region Start

    private void Start()
    {
        playersManager.Init();
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Update

    private void Update()
    {

    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    public static void SetPause(bool value)
    {
        GamePaused = value;
        Time.timeScale = value ? 0 : 1;

        GplayUI.OnSetPause(value);
    }

    #endregion

}