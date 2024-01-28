using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CharactersLibraryScrObj charactersLibrary;
    [SerializeField]
    private CharacterSelectionScrObj characterSelection;
    [SerializeField]
    private LevelsLibraryScrObj levelsLibrary;

    [Header("Components")]
    [SerializeField]
    private PlayersManager playersManager;
    [SerializeField]
    private Transform levelsParent;
    [SerializeField]
    private CigaretteOverlay cigaretteOverlay;

    [Header("Settings")]
    [SerializeField]
    private float minTimer;
    [SerializeField]
    private float maxTimer;

    [Space]
    [SerializeField]
    private Vector2 levelArea;

    [Header("Info")]
    [ReadOnly, SerializeField]
    private int currentPlayer;
    [ReadOnly, SerializeField]
    private float timer;

    #region Init

    public void Init()
    {
        InitEvents();

        BuildLevel(out List<Vector2> spawnPoints);
        playersManager.Init(charactersLibrary, characterSelection, levelArea, spawnPoints);
        currentPlayer = GetRandomPlayer();
        cigaretteOverlay.StartSequence(playersManager.GetPlayer(currentPlayer));
    }

    private void InitEvents()
    {
        playersManager.onPlayerCigarette += OnPlayerCigarette;
        cigaretteOverlay.onFinishSequence += StartGame;
    }

    private void BuildLevel(out List<Vector2> spawnPoints)
    {
        Level levelPrefab = levelsLibrary.GetRandomLevel();

        Level level = Instantiate(levelPrefab, levelsParent);
        level.name = levelPrefab.name;

        spawnPoints = level.SpawnPoints;
    }

    private int GetRandomPlayer() => UnityEngine.Random.Range(0, characterSelection.Players.Count) + 1;

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Start Game

    private void StartGame()
    {
        StartTimer();
        playersManager.StartGame(currentPlayer);
    }

    private void StartTimer()
    {
        timer = UnityEngine.Random.Range(minTimer, maxTimer);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Update

    private void Update()
    {
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        Timer.ReduceCooldown(ref timer, OnGameOver);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void OnPlayerCigarette(int playerID)
    {
        currentPlayer = playerID;
    }

    private void OnGameOver()
    {
        playersManager.GameOver();

        PlayUI();

        void PlayUI()
        {
            Character character = characterSelection.GetCharacter(currentPlayer);
            Sprite playerSprite = charactersLibrary.GetCharacterInfo(character).winSprite;
            GplayUI.GameOver(currentPlayer, playerSprite);
        }
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(transform.position, levelArea * 2);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region OnValidate

    private void OnValidate()
    {
        UpdateEditorFields();
    }

    private void UpdateEditorFields()
    {
        GambaFunctions.RestrictNegativeValues(ref minTimer);
        GambaFunctions.RestrictNegativeValues(ref maxTimer);
    }

    #endregion

#endif

    #endregion

}