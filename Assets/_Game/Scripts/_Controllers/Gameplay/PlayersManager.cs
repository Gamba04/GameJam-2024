using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Transform playersParent;

    private CharactersLibraryScrObj charactersLibrary;
    private CharacterSelectionScrObj characterSelection;

    private readonly List<Player> players = new List<Player>();

    public event Action<int> onPlayerCigarette;

    #region Init

    public void Init(CharactersLibraryScrObj charactersLibrary, CharacterSelectionScrObj characterSelection)
    {
        this.charactersLibrary = charactersLibrary;
        this.characterSelection = characterSelection;

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

        player.onCigarette += () => onPlayerCigarette?.Invoke(playerID);

        players.Add(player);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void StartLevel(int startingPlayer)
    {
        players.ForEach(ProcessPlayer);

        void ProcessPlayer(Player player)
        {
            bool selected = player.PlayerID == startingPlayer;

            player.SetCigarette(selected);
        }
    }

    #endregion

}