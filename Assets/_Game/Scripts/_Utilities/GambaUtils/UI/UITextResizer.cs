using UnityEngine;

[ExecuteAlways]
public class UITextResizer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private RectTransform text;

    [Header("Settings")]
    [SerializeField]
    [Range(1, 10)]
    private float density = 1;

    private void Resize()
    {
        if (text != null)
        {
            text.anchorMin = Vector2.zero;
            text.anchorMax = Vector2.one;

            text.localPosition = Vector2.zero;

            text.localScale = Vector3.one / density;

            RectTransform parent = text.parent?.GetComponent<RectTransform>();

            if (parent)
            {
                text.sizeDelta = parent.rect.size * (density - 1);
            }
        }
    }

    #region Editor

#if UNITY_EDITOR

    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (text == null) text = GetComponent<RectTransform>();

            Resize();
        }
    }

#endif

    #endregion

}