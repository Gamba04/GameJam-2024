using UnityEngine;

namespace GambaUtils
{
    [ExecuteAlways]
    public class UIScalerOverride : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private Canvas canvas;
        [SerializeField]
        private RectTransform rectTransform;

        [Header("Settings")]
        [SerializeField]
        private Vector2 originalScale = Vector2.one;
        [SerializeField]
        private Vector2 referenceResolution = new Vector2(1920, 1080);
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("0: Width\n1: Height")]
        private float match;

        private void Start()
        {
            Resize();
        }

        private void Resize()
        {
            bool componentsNull = canvas == null || rectTransform == null;
            bool resolutionInvalid = referenceResolution.x == 0 || referenceResolution.y == 0;

            if (componentsNull || resolutionInvalid) return;

            float widthRatio = Screen.width / referenceResolution.x;
            float heightRatio = Screen.height / referenceResolution.y;
            
            float scaleMult = ((widthRatio * (1 - match)) + (heightRatio * match)) / canvas.scaleFactor;

            Vector3 scale = originalScale * scaleMult;
            scale.z = 1;

            rectTransform.localScale = scale;
        }

        #region Editor

#if UNITY_EDITOR

        private void Update()
        {
            if (!Application.isPlaying) EditorUpdate();
        }

        private void OnValidate()
        {
            EditorUpdate();
        }

        private void EditorUpdate()
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

            Resize();
        }

#endif

        #endregion

    }
}