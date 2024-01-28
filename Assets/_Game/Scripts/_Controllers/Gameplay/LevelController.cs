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
        playersManager.onPlayerCigarette += OnPlayerCigarette;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void OnPlayerCigarette(int playerID)
    {
        Debug.Log($"New player with bareto: Player {playerID}", Color.cyan);
    }

    #endregion

}