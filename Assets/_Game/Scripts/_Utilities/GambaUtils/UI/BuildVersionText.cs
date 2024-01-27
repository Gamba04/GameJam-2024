using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    [ExecuteAlways]
    public class BuildVersionText : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private Text text;

        [Header("Settings")]
        [SerializeField]
        private string prefix;

        private void Awake()
        {
            UpdateVersion();
        }

        private void UpdateVersion()
        {
            if (text == null) return;

            text.text = prefix + Application.version;
        }

#if UNITY_EDITOR

        private void Update()
        {
            if (!Application.isPlaying) UpdateVersion();
        }

#endif

    }
}