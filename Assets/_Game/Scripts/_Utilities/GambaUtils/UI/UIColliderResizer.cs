using UnityEngine;

[ExecuteAlways]
public class UIColliderResizer : MonoBehaviour
{
    [SerializeField]
    private new BoxCollider2D collider;
    [SerializeField]
    private RectTransform rectTransform;

    private void Resize()
    {
        if (collider != null && rectTransform != null)
        {
            collider.size = rectTransform.rect.size;
        }
    }

#if UNITY_EDITOR

    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (collider == null) collider = GetComponent<BoxCollider2D>();
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

            Resize();
        }
    }

#endif

}