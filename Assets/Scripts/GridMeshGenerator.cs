using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridMeshGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [Min(2)] public int xSegments = 100;
    [Min(2)] public int zSegments = 100;
    [Min(0.1f)] public float width = 50f;
    [Min(0.1f)] public float length = 50f;
    [Min(0.1f)] public float uvScale = 4f;

    [Header("Generation")]
    public bool generateOnStart = true;

    private MeshFilter meshFilter;
    private Mesh generatedMesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    /// <summary>
    /// Builds a rectangular grid mesh on the XZ plane.
    /// </summary>
    [ContextMenu("Generate Grid Mesh")]
    public void Generate()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (generatedMesh != null)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }
        }

        generatedMesh = new Mesh
        {
            name = "Water Grid"
        };

        int vertexCountX = xSegments + 1;
        int vertexCountZ = zSegments + 1;
        int vertexCount = vertexCountX * vertexCountZ;

        if (vertexCount > 65535)
        {
            generatedMesh.indexFormat = IndexFormat.UInt32;
        }

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[xSegments * zSegments * 6];

        float xStep = width / xSegments;
        float zStep = length / zSegments;
        float xOrigin = -width * 0.5f;
        float zOrigin = -length * 0.5f;

        int vertexIndex = 0;
        for (int z = 0; z < vertexCountZ; z++)
        {
            for (int x = 0; x < vertexCountX; x++)
            {
                float xPos = xOrigin + x * xStep;
                float zPos = zOrigin + z * zStep;

                vertices[vertexIndex] = new Vector3(xPos, 0f, zPos);
                uvs[vertexIndex] = new Vector2(
                    ((float)x / xSegments) * uvScale,
                    ((float)z / zSegments) * uvScale);

                vertexIndex++;
            }
        }

        int triangleIndex = 0;
        for (int z = 0; z < zSegments; z++)
        {
            for (int x = 0; x < xSegments; x++)
            {
                int start = z * vertexCountX + x;

                triangles[triangleIndex++] = start;
                triangles[triangleIndex++] = start + vertexCountX;
                triangles[triangleIndex++] = start + 1;

                triangles[triangleIndex++] = start + 1;
                triangles[triangleIndex++] = start + vertexCountX;
                triangles[triangleIndex++] = start + vertexCountX + 1;
            }
        }

        generatedMesh.vertices = vertices;
        generatedMesh.uv = uvs;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();

        meshFilter.sharedMesh = generatedMesh;
    }
}
