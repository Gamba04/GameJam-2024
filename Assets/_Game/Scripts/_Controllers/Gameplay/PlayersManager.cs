using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Player playerPrefab;
    [SerializeField]
    private Transform playersParent;

    #region Init

    public void Init()
    {
        CreatePlayers();
    }

    private void CreatePlayers()
    {
        Player player = Instantiate(playerPrefab, playersParent);
        player.name = playerPrefab.name;

        player.Init(0);
    }

    #endregion

}