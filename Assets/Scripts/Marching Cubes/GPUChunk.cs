using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Triangle
{
  public Vector3 a;
  public Vector3 b;
  public Vector3 c;
};

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GPUChunk : MonoBehaviour
{
  // Compute shader
  private const int threadsCount = 8;
  [SerializeField] ComputeShader meshGenerator = null;

  // Mesh
  private float[] vertices = null;
  private Triangle[] triangles = null;

  // Configs
  private int chunkSize = 4;
  private int seed = 42;
  private float isoLevel = 0.5f;
  private List<Texture2D> noiseMaps = new List<Texture2D>();

  // Bounds
  private int voxelsNumber = 0;
  private int maxTrianglesNumber = 0;

  // Refferences
  private SurfaceManager surfaceManager = null;
  private MeshFilter meshFilter = null;

  void Start()
  {
    surfaceManager = FindObjectOfType<SurfaceManager>();
    meshFilter = GetComponent<MeshFilter>();

    vertices = new float[chunkSize * chunkSize * chunkSize];
    GetConfig();

    voxelsNumber = chunkSize * chunkSize * chunkSize;
    maxTrianglesNumber = voxelsNumber * 5;
    triangles = new Triangle[maxTrianglesNumber];

    Generate();
  }

  private void GetConfig()
  {
    if (surfaceManager != null)
    {
      chunkSize = surfaceManager.GetChunkSize();
      seed = surfaceManager.GetSeed();
      noiseMaps = surfaceManager.GetNoiseMaps();
    }
  }

  void Generate()
  {
    GenerateDensity();
    GenerateMesh();
  }

  void GenerateDensity()
  {
    ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float));

    int kernel = meshGenerator.FindKernel("DistributeDensity");
    int numThreadsPerGroup = Mathf.CeilToInt(chunkSize / (float)threadsCount);

    meshGenerator.SetBuffer(kernel, "_ChunkVertices", verticesBuffer);
    meshGenerator.SetTexture(kernel, "_NoiseMap", noiseMaps[0]);
    meshGenerator.SetFloat("_IsoLevel", isoLevel);
    meshGenerator.SetVector("_ChunkPosition", transform.position);
    meshGenerator.SetInt("_ChunkSize", chunkSize);
    meshGenerator.Dispatch(kernel, numThreadsPerGroup, numThreadsPerGroup, numThreadsPerGroup);

    verticesBuffer.GetData(vertices);
    verticesBuffer.Release();
  }

  void GenerateMesh()
  {
    ComputeBuffer trianglesBudffer = new ComputeBuffer(triangles.Length, sizeof(float) * 9, ComputeBufferType.Append);
    ComputeBuffer trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float));

    int kernel = meshGenerator.FindKernel("GenerateMesh");
    int numThreadsPerGroup = Mathf.CeilToInt(chunkSize - 1 / (float)threadsCount);

    meshGenerator.SetBuffer(kernel, "_ChunkVertices", verticesBuffer);
    meshGenerator.SetBuffer(kernel, "_ChunkTriangles", trianglesBudffer);
    meshGenerator.SetFloat("_IsoLevel", isoLevel);
    meshGenerator.SetVector("_ChunkPosition", transform.position);
    meshGenerator.SetInt("_ChunkSize", chunkSize);

    trianglesBudffer.SetCounterValue(0);
    meshGenerator.Dispatch(kernel, numThreadsPerGroup, numThreadsPerGroup, numThreadsPerGroup);

    ComputeBuffer.CopyCount(trianglesBudffer, trianglesCountBuffer, 0);
    int[] trianglesCount = new int[1];
    trianglesCountBuffer.GetData(trianglesCount);

    Triangle[] generatedTriangles = new Triangle[trianglesCount[0]];
    int[] meshTriangles = new int[trianglesCount[0] * 3];
    Vector3[] meshVertices = new Vector3[trianglesCount[0] * 3];
    Debug.Log("Generated " + trianglesCount[0] + " triangles");

    trianglesBudffer.GetData(generatedTriangles, 0, 0, generatedTriangles.Length);
    for (int i = 0; i < trianglesCount[0]; i++)
    {
      Triangle tri = generatedTriangles[i];
      int baseIndex = i * 3;
      meshVertices[baseIndex] = tri.a;
      meshVertices[baseIndex + 1] = tri.b;
      meshVertices[baseIndex + 2] = tri.c;

      meshTriangles[baseIndex] = baseIndex;
      meshTriangles[baseIndex + 1] = baseIndex + 1;
      meshTriangles[baseIndex + 2] = baseIndex + 2;
    }

    Mesh mesh = new Mesh();
    mesh.vertices = meshVertices;
    mesh.triangles = meshTriangles;
    mesh.RecalculateNormals();
    meshFilter.mesh = mesh;

    trianglesBudffer.Release();
    trianglesCountBuffer.Release();
    verticesBuffer.Release();
  }

  void OnDrawGizmos()
  {
    if (Application.isPlaying && vertices != null)
    {
      for (int x = 0; x < chunkSize; x++)
      {
        for (int y = 0; y < chunkSize; y++)
        {
          for (int z = 0; z < chunkSize; z++)
          {
            int idx = PositionToBufferIndex(x, y, z);
            float density = vertices[idx];
            Gizmos.color = new Color(density, density, density, 1f);
            Gizmos.DrawSphere(new Vector3(x, y, z) + transform.position, 0.03f);
          }
        }
      }
    }
  }

  public int PositionToBufferIndex(int x, int y, int z)
  {
    return x + (y * chunkSize) + (z * chunkSize * chunkSize);
  }

  public Vector3 BurfferIndexToPosition(int idx)
  {
    int x = idx % chunkSize;
    int y = ((idx - x) / chunkSize) % chunkSize;
    int z = ((((idx - x) / chunkSize) - y) / chunkSize) % chunkSize;
    return new Vector3(x, y, z);
  }
}
