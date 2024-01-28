using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Stage : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private VideoPlayer videoPlayer;

    [Header("Settings")]
    [SerializeField]
    private List<Vector2> spawnPoints = new List<Vector2>();

    public List<Vector2> SpawnPoints => spawnPoints;

    #region Init

    public void Init()
    {
        videoPlayer.targetCamera = Camera.main;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

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