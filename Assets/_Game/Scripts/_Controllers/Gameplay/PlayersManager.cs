using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CharactersLibraryScrObj charactersLibrary;
    [SerializeField]
    private CharacterSelectionScrObj characterSelection;

    [Header("Components")]
    [SerializeField]
    private Transform playersParent;

    public event Action<int> onPlayerBareto;

    #region Init

    public void Init()
    {
        CreatePlayers();
    }

    private void CreatePlayers()
    {
        characterSelection.Players.ForEach(SpawnCharacter);
    }

    private void SpawnCharacter(Character character, int index)
    {
        CharacterInfo info = charactersLibrary.GetCharacterInfo(character);

        BuildPlayer(info, index + 1);
    }

    private void BuildPlayer(CharacterInfo character, int playerID)
    {
        Player player = Instantiate(character.prefab, playersParent);
        player.name = $"Player {playerID}: {character.displayName}";

        player.Init(playerID);

        player.onBareto += () => onPlayerBareto?.Invoke(playerID);
    }

    #endregion

}