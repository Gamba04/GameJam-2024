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

    [Header("Components")]
    [SerializeField]
    private PlayersManager playersManager;

    [Header("Settings")]
    [SerializeField]
    private float minTimer;
    [SerializeField]
    private float maxTimer;

    [Header("Info")]
    [ReadOnly, SerializeField]
    private int currentPlayer;
    [ReadOnly, SerializeField]
    private float timer;

    private bool gameOver;

    #region Init

    public void Init()
    {
        InitEvents();

        playersManager.Init(charactersLibrary, characterSelection);

        StartLevel();
    }

    private void InitEvents()
    {
        playersManager.onPlayerCigarette += OnPlayerCigarette;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region StartLevel

    private void StartLevel()
    {
        StartTimer();
        playersManager.StartLevel(GetRandomPlayer());
    }

    private void StartTimer()
    {
        timer = UnityEngine.Random.Range(minTimer, maxTimer);
    }

    private int GetRandomPlayer() => UnityEngine.Random.Range(0, characterSelection.Players.Count) + 1;

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Update

    private void Update()
    {
        UpdateTimer();
        UpdateInputs();
    }

    private void UpdateTimer()
    {
        Timer.ReduceCooldown(ref timer, OnGameOver);
    }

    private void UpdateInputs()
    {
        if (gameOver && Input.GetButtonDown("Restart"))
        {
            UIManager.SetFade(false, GambaFunctions.ReloadScene);
        }
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
        gameOver = true;

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

    private void OnValidate()
    {
        UpdateEditorFields();
    }

    private void UpdateEditorFields()
    {
        GambaFunctions.RestrictNegativeValues(ref minTimer);
        GambaFunctions.RestrictNegativeValues(ref maxTimer);
    }

#endif

    #endregion

}