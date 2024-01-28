using System;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField]
    private List<Vector2> spawnPoints = new List<Vector2>();

    public List<Vector2> SpawnPoints => spawnPoints;

    #region Editor

#if UNITY_EDITOR

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        spawnPoints.ForEach(point => Gizmos.DrawWireSphere(point, 0.5f));
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region OnValidate

    private void OnValidate()
    {
        spawnPoints.Resize(CharacterSelectionScrObj.MAX_PLAYERS);
    }

    #endregion

#endif

    #endregion

}