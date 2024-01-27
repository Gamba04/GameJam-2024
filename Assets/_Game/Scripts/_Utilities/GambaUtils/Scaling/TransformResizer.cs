using UnityEngine;

namespace GambaUtils.Resizers
{
    public class TransformResizer : Resizer
    {
        [Separator]
        [SerializeField]
        private bool includeZAxis;

        #region Resize

        protected override void ApplySize(float size)
        {
            Vector3 scale = Vector3.one * size;
            if (!includeZAxis) scale.z = 0;

            transform.localScale = scale;
        }

        #endregion

    }
}