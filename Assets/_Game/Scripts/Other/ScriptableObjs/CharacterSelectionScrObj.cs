using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ASSET_NAME, menuName = "Characters/" + ASSET_NAME)]
public class CharacterSelectionScrObj : ScriptableObject
{
    private const string ASSET_NAME = "Character Selection";
    public const int MAX_PLAYERS = 4;

    [SerializeField]
    private List<NamedReference<Character>> players = new List<NamedReference<Character>>();

    public List<Character> Players => players.ConvertAll(player => player.reference);

    #region Public Methods

    public Character GetCharacter(int playerID) => Players[playerID - 1];

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        players.Resize(Mathf.Min(players.Count, MAX_PLAYERS));
        players.ForEach((player, index) => player.SetName($"Player {index + 1}"));
    }

#endif

    #endregion

}