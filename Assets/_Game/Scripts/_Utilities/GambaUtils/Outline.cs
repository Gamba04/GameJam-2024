using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Outline : MonoBehaviour
{

    #region Structures

    private class VertexGroup
    {
        public List<int> vertices = new List<int>();

        private Vector3 normalSum;

        public void AddNormal(Normal normal)
        {
            vertices.Add(normal.index);

            normalSum += normal.normal;
        }

        public void BakeNormals(ref Vector3[] normals)
        {
            Vector3 average = normalSum / vertices.Count;

            foreach (int index in vertices) normals[index] = average;
        }
    }

    private struct Normal
    {
        public int index;
        public Vector3 normal;

        public Normal(int index, Vector3 normal)
        {
            this.index = index;
            this.normal = normal;
        }
    }

    #endregion

    [SerializeField]
    private bool active;

    [Space]
    [SerializeField]
    private Material outlineMaterial;

    [Space]
    [SerializeField]
    [Tooltip("RECOMMENDED\n\nFor the shader to work properly, it might be necessary to smooth the Mesh normals. This process might take some time, so it is recommended to bake them in the Editor.")]
    private bool bakeNormals;

    [SerializeField, HideInInspector]
    private bool meshBaked;

    private Mesh mesh;
    private Mesh customMesh;

    private new Renderer renderer;
    private MeshFilter filter;

    private Material lastMaterial;

    private const int uvChannel = 7;

    private bool IsOutlineActive => active && outlineMaterial;

    public bool Active
    {
        get => active;

        set
        {
            active = value;

            UpdateMaterial();
        }
    }

    public Material OutlineMaterial
    {
        get => OutlineMaterial;

        set
        {
            OutlineMaterial = value;

            UpdateMaterial();
        }
    }

    #region Unity Callbacks

    private void Start()
    {
        if (Application.isPlaying)
        {
            if (!meshBaked) bakeNormals = true;

            UpdateData();
        }
    }

#if UNITY_EDITOR

    private void Update()
    {
        UpdateData();
    } 

#endif

    private void OnDisable() => enabled = true;

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region UpdateData

    private void UpdateData()
    {
        GetComponents();

        UpdateMesh();
        UpdateMaterial();
    }

    private void GetComponents()
    {
        if (!renderer) renderer = GetComponent<Renderer>();
        if (!filter) filter = GetComponent<MeshFilter>();
    }

    #region Mesh

    private void UpdateMesh()
    {
        if (filter.sharedMesh) // Mesh assigned
        {
            if ((customMesh && filter.sharedMesh != customMesh) || (!customMesh && mesh != filter.sharedMesh))
            {
                CleanCustomMesh();
                mesh = filter.sharedMesh;
            }
        }
        else if (customMesh || mesh) // No Mesh
        {
            CleanCustomMesh();

            mesh = null;
        }

        if (!bakeNormals) return;
        bakeNormals = false;

        if (!mesh) return;

        if (!mesh.isReadable) throw new UnauthorizedAccessException($"Cannot prepare Mesh '{mesh.name}' for outline, Read/Write must be enabled in import settings");

        meshBaked = true;

        if (!customMesh) CreateCustomMesh();

        filter.sharedMesh = customMesh;
    }

    private void CleanCustomMesh()
    {
        if (filter.sharedMesh == customMesh) filter.sharedMesh = null;

        if (Application.isPlaying) Destroy(customMesh);
        else DestroyInEditor(customMesh);

        customMesh = null;

        meshBaked = false;
    }

    private void CreateCustomMesh()
    {
        customMesh = new Mesh();
        customMesh.name = $"{mesh.name} (Baked normals)";

        customMesh.vertices = mesh.vertices;
        customMesh.triangles = mesh.triangles;
        customMesh.normals = mesh.normals;
        customMesh.uv = mesh.uv;

        SmoothNormals(customMesh);
    }

    private void SmoothNormals(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        int vertexCount = vertices.Length;

        if (normals.Length == 0) return;

        if (vertexCount != normals.Length) throw new ArgumentException("Vertices and normals do not match!");

        Dictionary<Vector3, VertexGroup> vertexGroups = new Dictionary<Vector3, VertexGroup>(vertexCount);

        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 position = vertices[i];

            if (!vertexGroups.ContainsKey(position))
            {
                vertexGroups.Add(position, new VertexGroup());
            }

            vertexGroups[position].AddNormal(new Normal(i, normals[i]));
        }

        // Bake normals
        foreach (KeyValuePair<Vector3, VertexGroup> group in vertexGroups) group.Value.BakeNormals(ref normals);

        // Set normals so uvChannel
        mesh.SetUVs(uvChannel, normals);
    }

    #endregion

    #region Material

    private void UpdateMaterial()
    {
        if (IsOutlineActive && (outlineMaterial == lastMaterial || !lastMaterial))
        {
            AddMaterialToTail();
        }
        else
        {
            RemoveMaterialFromTail();
            if (IsOutlineActive) AddMaterialToTail();
        }

        lastMaterial = outlineMaterial;
    }

    private void AddMaterialToTail()
    {
        if (renderer.sharedMaterials.Length == 0) // No materials in renderer
        {
            renderer.sharedMaterial = outlineMaterial;
        }
        else if (renderer.sharedMaterials[renderer.sharedMaterials.Length - 1] != outlineMaterial) // Last material in renderer is not outlineMaterial
        {
            // Add outlineMaterial at the end
            List<Material> materials = new List<Material>(renderer.sharedMaterials);

            materials.Add(outlineMaterial);

            renderer.sharedMaterials = materials.ToArray();
        }
    }

    private void RemoveMaterialFromTail()
    {
        if (!lastMaterial || renderer.sharedMaterials.Length == 0) return;

        if (renderer.sharedMaterials[renderer.sharedMaterials.Length - 1] == lastMaterial)
        {
            List<Material> materials = new List<Material>(renderer.sharedMaterials);

            materials.RemoveAt(materials.Count - 1);

            renderer.sharedMaterials = materials.ToArray();
        }
    }

    #endregion

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void DestroyInEditor(UnityEngine.Object obj)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () => MonoBehaviour.DestroyImmediate(obj);
#endif
    }

    #endregion

}