using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ASSET_NAME, menuName = ASSET_NAME)]
public class StagesLibraryScrObj : ScriptableObject
{
    public const string ASSET_NAME = "Stages Library";

    [SerializeField]
    private List<NamedReference<Stage>> stages = new List<NamedReference<Stage>>();

    public Stage GetRandomStage() => stages[Random.Range(0, stages.Count)].reference;

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        stages.ForEach((level, index) => level.SetName($"Stage {index + 1}"));
    }

#endif

    #endregion

}