using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField]
    private PlayersManager playersManager;

    #region Init

    public void Init()
    {
        InitEvents();

        playersManager.Init();
    }

    private void InitEvents()
    {
        playersManager.onPlayerBareto += OnPlayerBareto;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void OnPlayerBareto(int playerID)
    {
        Debug.Log($"New player with bareto: Player {playerID}", Color.red);
    }

    #endregion

}