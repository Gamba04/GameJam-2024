using UnityEngine;

public class VSyncController : MonoBehaviour
{
    [SerializeField]
    private bool vSyncEnabled;
    [SerializeField]
    private int target;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = vSyncEnabled ? target : -1;
    }
}