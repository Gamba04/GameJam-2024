using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ASSET_NAME, menuName = ASSET_NAME)]
public class LevelsLibraryScrObj : ScriptableObject
{
    public const string ASSET_NAME = "Levels Library";

    [SerializeField]
    private List<NamedReference<Level>> levels = new List<NamedReference<Level>>();

    public Level GetRandomLevel() => levels[Random.Range(0, levels.Count)].reference;

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        levels.ForEach((level, index) => level.SetName($"Stage {index + 1}"));
    }

#endif

    #endregion

}