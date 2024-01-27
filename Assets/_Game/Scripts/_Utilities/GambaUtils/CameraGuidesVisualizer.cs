using UnityEngine;
using UnityEditor;

public class CameraGuidesVisualizer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float size = 4.05f;
    [SerializeField]
    private Vector2 baseAspect = new Vector2(16, 9);

    [Header("Visualize Aspect")]
    [SerializeField]
    private Vector2 currentAspect = new Vector2(16, 9);
    [SerializeField]
    private bool visualizeAspect;

    [Header("Color")]
    [SerializeField]
    private Color color = Color.black;

#if UNITY_EDITOR

    #region Gizmos

    private void OnDrawGizmos()
    {
        Gizmos.color = color;

        Gizmos.DrawWireCube(transform.position, GetGuideOfAspect(4, 3));
        Gizmos.DrawWireCube(transform.position, GetGuideOfAspect(21, 9));

        if (visualizeAspect) VisualizeAspect();
    }

    private void VisualizeAspect()
    {
        Handles.color = color;

        Vector2 window = GetGuideOfAspect(currentAspect.x, currentAspect.y) / 2;
        Vector3 opaque = (Vector3)window + Vector3.one * 20;

        Vector3[] verts = new Vector3[4];

        // Left
        verts[0] = transform.position + new Vector3(-window.x, -window.y);
        verts[1] = transform.position + new Vector3(-opaque.x, -opaque.y);
        verts[2] = transform.position + new Vector3(-opaque.x, opaque.y);
        verts[3] = transform.position + new Vector3(-window.x, window.y);

        Handles.DrawAAConvexPolygon(verts);
        Handles.DrawAAPolyLine(verts);

        // Up
        verts[0] = transform.position + new Vector3(-window.x, window.y);
        verts[1] = transform.position + new Vector3(-opaque.x, opaque.y);
        verts[2] = transform.position + new Vector3(opaque.x, opaque.y);
        verts[3] = transform.position + new Vector3(window.x, window.y);

        Handles.DrawAAConvexPolygon(verts);
        Handles.DrawAAPolyLine(verts);

        // Right
        verts[0] = transform.position + new Vector3(window.x, window.y);
        verts[1] = transform.position + new Vector3(opaque.x, opaque.y);
        verts[2] = transform.position + new Vector3(opaque.x, -opaque.y);
        verts[3] = transform.position + new Vector3(window.x, -window.y);

        Handles.DrawAAConvexPolygon(verts);
        Handles.DrawAAPolyLine(verts);

        // Down
        verts[0] = transform.position + new Vector3(window.x, -window.y);
        verts[1] = transform.position + new Vector3(opaque.x, -opaque.y);
        verts[2] = transform.position + new Vector3(-opaque.x, -opaque.y);
        verts[3] = transform.position + new Vector3(-window.x, -window.y);

        Handles.DrawAAConvexPolygon(verts);
        Handles.DrawAAPolyLine(verts);
    }

    private Vector3 GetGuideOfAspect(float x, float y)
    {
        float ratio = x / y;
        float baseRatio = baseAspect.x / baseAspect.y;

        bool widthBase = ratio < baseRatio;

        x = widthBase ? size * baseRatio : size * ratio;
        y = widthBase ? size * baseRatio / ratio : size;

        return new Vector3(x, y, 0.01f);
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Inspector

    private void OnValidate()
    {
        size = Mathf.Max(size, 0);

        // Clamp current aspect
        currentAspect.x = Mathf.Max(currentAspect.x, 1);
        currentAspect.y = Mathf.Max(currentAspect.y, 1);
    }

    #endregion

#endif

}