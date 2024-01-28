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

    private Vector2 levelArea;

    private int startingPlayer;
    private List<Vector2> spawnPoints;

    private readonly List<Player> players = new List<Player>();

    public event Action<int> onPlayerCigarette;

    #region Init

    public void Init(CharactersLibraryScrObj charactersLibrary, CharacterSelectionScrObj characterSelection, Vector2 levelArea, List<Vector2> spawnPoints)
    {
        this.charactersLibrary = charactersLibrary;
        this.characterSelection = characterSelection;
        this.levelArea = levelArea;
        this.spawnPoints = spawnPoints;

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

        player.transform.position = spawnPoints[playerID - 1];

        player.Init(playerID, levelArea);

        player.onCigarette += () => onPlayerCigarette?.Invoke(playerID);

        players.Add(player);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void StartGame(int startingPlayer)
    {
        players.ForEach(player => player.OnStartGame(startingPlayer));
    }

    public void GameOver()
    {
        players.ForEach(player => player.GameOver());
    }

    #endregion

}