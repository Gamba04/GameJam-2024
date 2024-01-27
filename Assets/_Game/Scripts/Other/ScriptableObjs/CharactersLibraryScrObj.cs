using System;
using System.Collections.Generic;
using UnityEngine;

#region Custom Data

public enum Character
{
    FatBoi,
    TNT,
    Dynamite,
    PipeGun,
}

[Serializable]
public class CharacterInfo
{
    [SerializeField, HideInInspector] private string name;

    public string displayName;
    public Player prefab;

    public void EditorUpdate(Character character)
    {
        if (displayName == "") displayName = character.ToString();

        name = $"{displayName} ({character})";
    }
}

#endregion

[CreateAssetMenu(fileName = ASSET_NAME, menuName = "Characters/" + ASSET_NAME)]
public class CharactersLibraryScrObj : ScriptableObject
{
    private const string ASSET_NAME = "Characters Library";

    [SerializeField]
    private List<CharacterInfo> characters = new List<CharacterInfo>();

    #region Public Methods

    public CharacterInfo GetCharacterInfo(Character character) => characters[(int)character];

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        characters.Resize(typeof(Character));
        characters.ForEach((character, index) => character.EditorUpdate((Character)index));
    }

#endif

    #endregion

}