using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class GambaLineRenderer : MonoBehaviour
{
    private class MeshData
    {
        [SerializeField]
        private List<Vector3> vertices;
        [SerializeField]
        private List<Vector2> uv;
        [SerializeField]
        private List<int> triangles;

        public int CurrentVertexCount => vertices != null ? vertices.Count : -1;

        public MeshData()
        {
            vertices = new List<Vector3>();
            uv = new List<Vector2>();
            triangles = new List<int>();
        }

        public MeshData(List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
        {
            this.vertices = vertices;
            this.uv = uv;
            this.triangles = triangles;
        }

        public void AppendData(MeshData data)
        {
            if (data == null) return;

            vertices.AddRange(data.vertices);
            uv.AddRange(data.uv);
            triangles.AddRange(data.triangles);
        }

        public void SetMesh(Mesh mesh)
        {
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();
        }
    }

    [Header("Settings")]
    [SerializeField]
    private Material material;
    [SerializeField]
    private Color color = Color.white;
    [SerializeField]
    [Range(0, 5)]
    private float lineThickness = 0.2f;
    [SerializeField]
    [Range(0, 32)]
    private int edgeSmoothness = 4;

    [Space]
    [SerializeField]
    private List<Vector2> positions = new List<Vector2>();

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Mesh mesh;
    private MeshData meshData;

    private Material previousMaterial;
    private Material mat;

    public int PositionCount => positions.Count;

    public Color Color
    {
        get => color;

        set
        {
            color = value;

            UpdateMaterial();
        }
    }

    #region Awake

    private void Awake()
    {
        GetComponents();
        UpdateMaterial();
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Update

    private void UpdateMesh()
    {
        // Init Mesh
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "GambaLineRenderer";
        }

        // Assign Mesh
        if (meshFilter.sharedMesh != mesh) meshFilter.sharedMesh = mesh;

        // Clean Mesh
        mesh.Clear();
        mesh.bounds = default;

        // Return if not enough positions
        if (this.positions.Count < 2) return;

        // Scale positions
        List<Vector2> positions = new List<Vector2>(this.positions);

        for (int i = 0; i < positions.Count; i++)
        {
            positions[i] = new Vector2(positions[i].x / transform.localScale.x, positions[i].y / transform.localScale.y);
        }

        // Generate Mesh
        meshData = new MeshData();

        // Initial semi-circle
        meshData.AppendData(GetSemiCircle(positions[0], (positions[0] - positions[1]).normalized));

        for (int i = 1; i < positions.Count; i++)
        {
            // Get position values
            Vector2 a = positions[i - 1];
            Vector2 b = positions[i];

            Vector2 dir = (b - a).normalized;

            // Get next dir
            Vector2 nextDir = i < positions.Count - 1 ? positions[i + 1] - b : Vector2.zero;
            nextDir.Normalize();

            // Get Line Data
            meshData.AppendData(GetLine(a, b));
            meshData.AppendData(GetSemiCircle(b, dir, nextDir));
        }

        // Set Mesh values
        meshData.SetMesh(mesh);
    }

    private void GetComponents()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
    }

    private void UpdateMaterial()
    {
        if (material == null)
        {
            meshRenderer.sharedMaterials = new Material[0];

            return;
        }
        else // Material assigned
        {
            if (meshRenderer.sharedMaterials.Length != 1) meshRenderer.sharedMaterials = new Material[1];

#if UNITY_EDITOR
            // Detect material change
            if (previousMaterial != material)
            {
                if (mat != null) DestroyInEditor(mat);
                mat = null;

                previousMaterial = material;
            }
#endif

            // Generate material
            if (mat == null)
            {
                mat = new Material(material);
                mat.name = material.name;
            }

            if (meshRenderer.sharedMaterial != mat) meshRenderer.sharedMaterial = mat;
        }

        // Update color
        mat.color = color;
        mat.SetColor("_Color", color);
    }


    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Geometry

    private MeshData GetLine(Vector2 a, Vector2 b)
    {
        // Get current vertex count
        int currentVertexCount = meshData.CurrentVertexCount;

        // Get base data
        Vector2 dir = (b - a).normalized;
        Vector2 perpendicular = new Vector2(dir.y, -dir.x);

        Vector2 halfBase = perpendicular * lineThickness / 2;

        // Generate values
        List<Vector3> vertices = new List<Vector3>(4)
            {
                a - halfBase,
                b - halfBase,
                b + halfBase,
                a + halfBase,
            };

        List<Vector2> uv = new List<Vector2>(4)
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
            };

        List<int> triangles = new List<int>(6)
            {
                currentVertexCount + 0,
                currentVertexCount + 1,
                currentVertexCount + 2,

                currentVertexCount + 0,
                currentVertexCount + 2,
                currentVertexCount + 3,
            };

        return new MeshData(vertices, uv, triangles);
    }

    private MeshData GetSemiCircle(Vector2 origin, Vector2 dir, Vector2 nextSegmentDir = default)
    {
        if (edgeSmoothness == 0) return null;

        bool usesNextSegmentOpt = nextSegmentDir != default;

        // Get current vertex count
        int currentVertexCount = meshData.CurrentVertexCount;

        // Get base data
        Vector2 perpendicular = new Vector2(dir.y, -dir.x);
        float radius = lineThickness / 2;

        int circleSegments = edgeSmoothness + 2;
        float segmentArc = Mathf.PI / (circleSegments - 1);

        // Init values
        int capacity = 3 * circleSegments - 1;

        List<Vector3> vertices = new List<Vector3>(capacity);
        List<Vector2> uv = new List<Vector2>(capacity);
        List<int> triangles = new List<int>(capacity);

        // Get first previousPos
        Vector2 previousPos = origin + perpendicular * radius;

        // Get next segment dot
        Vector2 nextSegmentPerpendicular = usesNextSegmentOpt ? new Vector2(nextSegmentDir.y, -nextSegmentDir.x) : default;

        bool nextSegmentIsLeft = usesNextSegmentOpt ? Vector2.Dot(nextSegmentPerpendicular, dir) < 0 : default;
        float nextSegmentDot = usesNextSegmentOpt ? Vector2.Dot(nextSegmentPerpendicular, perpendicular) * (nextSegmentIsLeft ? -1 : 1) : default;

        for (int i = 1; i < circleSegments; i++)
        {
            if (usesNextSegmentOpt && !nextSegmentIsLeft) // Right side is showing
            {
                Vector2 previousDir = previousPos - origin;

                float dot = Vector2.Dot(previousDir.normalized, perpendicular);

                if (dot < nextSegmentDot) continue;
            }

            // Generate triangle
            float currentAngle = i * segmentArc;

            Vector2 currentDir = perpendicular * Mathf.Cos(currentAngle) + dir * Mathf.Sin(currentAngle);
            Vector2 currentPos = origin + currentDir * radius;

            if (usesNextSegmentOpt && nextSegmentIsLeft) // Left side is showing
            {
                float dot = Vector2.Dot(currentDir, perpendicular);

                if (dot > nextSegmentDot)
                {
                    // Update previousPos
                    previousPos = currentPos;

                    continue;
                }
            }

            // Add vertices
            vertices.Add(origin);
            vertices.Add(previousPos);
            vertices.Add(currentPos);

            // Add UVs
            uv.Add(default);
            uv.Add(default);
            uv.Add(default);

            // Add triangles
            triangles.Add(currentVertexCount + 2);
            triangles.Add(currentVertexCount + 1);
            triangles.Add(currentVertexCount + 0);

            // Update vertex count
            currentVertexCount += 3;

            // Update previousPos
            previousPos = currentPos;
        }

        return new MeshData(vertices, uv, triangles);
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public List<Vector2> GetPositions() => positions;

    public void SetPositions(IEnumerable<Vector2> positions)
    {
        this.positions = new List<Vector2>(positions);

        UpdateMesh();
    }

    public void SetPosition(int index, Vector2 position, bool updateMesh = true)
    {
        if (index < 0 || index >= positions.Count) return;

        positions[index] = position;

        if (updateMesh) UpdateMesh();
    }

    public void AddPosition(Vector2 position)
    {
        positions.Add(position);

        UpdateMesh();
    }

    public void ClearPositions()
    {
        positions.Clear();

        UpdateMesh();
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Other

    private void DestroyInEditor(Object obj)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            DestroyImmediate(obj);
        };
#endif
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        GetComponents();
        UpdateMaterial();
        UpdateMesh();
    }

#endif

    #endregion

}