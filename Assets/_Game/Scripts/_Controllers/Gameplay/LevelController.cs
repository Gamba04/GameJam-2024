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
    private int selectedPlayer;
    [ReadOnly, SerializeField]
    private float timer;

    #region Init

    public void Init()
    {
        InitEvents();

        playersManager.Init(charactersLibrary, characterSelection);

        Timer.CallOnDelay(StartLevel, 2);
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
        selectedPlayer = playerID;
    }

    private void OnGameOver()
    {
        GambaFunctions.ReloadScene();
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