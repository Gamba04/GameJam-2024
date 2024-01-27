using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ASSET_NAME, menuName = "Characters/" + ASSET_NAME)]
public class CharacterSelectionScrObj : ScriptableObject
{
    private const string ASSET_NAME = "Character Selection";

    [SerializeField]
    private List<NamedReference<Character>> players = new List<NamedReference<Character>>();

    public List<Character> Players => players.ConvertAll(player => player.reference);

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        players.ForEach((player, index) => player.SetName($"Player {index}"));
    }

#endif

    #endregion

}