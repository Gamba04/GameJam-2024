using UnityEngine;

namespace GambaUtils.Resizers
{
    public class CameraResizer : Resizer
    {
        [Separator]
        [SerializeField]
        private new Camera camera;

        #region Resize

        protected override void ApplySize(float size)
        {
            if (camera == null) return;

            camera.orthographicSize = size;
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------------------------------

        #region Editor

#if UNITY_EDITOR

        protected override void EditorUpdate()
        {
            base.EditorUpdate();

            if (camera == null) camera = GetComponent<Camera>();
        }

#endif

        #endregion

    }
}