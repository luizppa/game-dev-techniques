using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Triangle
{
  public Vector3 a;
  public Vector3 b;
  public Vector3 c;
  public Vector3 padding;
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
  private int verticesNumber = 0;
  private int maxTrianglesNumber = 0;

  // Refferences
  private SurfaceManager surfaceManager = null;
  private MeshFilter meshFilter = null;

  // Buffers
  ComputeBuffer trianglesBudffer = null;
  ComputeBuffer trianglesCountBuffer = null;
  ComputeBuffer verticesBuffer = null;

  void Start()
  {
    GetConfig();
    InitializeProperties();
    InitializaBuffers();
    Generate();
  }

  void OnDisable()
  {
    ReleaseBuffers();
  }

  private void GetConfig()
  {
    surfaceManager = FindObjectOfType<SurfaceManager>();
    if (surfaceManager != null)
    {
      chunkSize = surfaceManager.GetChunkSize();
      seed = surfaceManager.GetSeed();
      noiseMaps = surfaceManager.GetNoiseMaps();
    }
  }

  private void InitializeProperties()
  {
    meshFilter = GetComponent<MeshFilter>();
    voxelsNumber = (chunkSize - 1) * (chunkSize - 1) * (chunkSize - 1);
    verticesNumber = chunkSize * chunkSize * chunkSize;
    maxTrianglesNumber = voxelsNumber * 5;
    vertices = new float[verticesNumber];
    triangles = new Triangle[maxTrianglesNumber];
  }

  private void InitializaBuffers()
  {
    trianglesBudffer = new ComputeBuffer(maxTrianglesNumber, sizeof(float) * 12, ComputeBufferType.Append);
    trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    verticesBuffer = new ComputeBuffer(verticesNumber, sizeof(float));
  }

  private void ReleaseBuffers()
  {
    trianglesBudffer.Release();
    trianglesCountBuffer.Release();
    verticesBuffer.Release();
  }

  void Generate()
  {
    GenerateDensity();
    GenerateMesh();
  }

  void GenerateDensity()
  {
    int verticesNumber = chunkSize * chunkSize * chunkSize;
    // ComputeBuffer verticesBuffer = new ComputeBuffer(verticesNumber, sizeof(float));

    int kernel = meshGenerator.FindKernel("DistributeDensity");
    int numThreadsPerGroup = Mathf.CeilToInt(chunkSize / (float)threadsCount);

    meshGenerator.SetBuffer(kernel, "_ChunkVertices", verticesBuffer);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol1", noiseMaps[0]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol2", noiseMaps[1]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol3", noiseMaps[2]);
    meshGenerator.SetFloat("_IsoLevel", isoLevel);
    meshGenerator.SetVector("_ChunkPosition", transform.position);
    meshGenerator.SetInt("_ChunkSize", chunkSize);
    meshGenerator.Dispatch(kernel, numThreadsPerGroup, numThreadsPerGroup, numThreadsPerGroup);

    verticesBuffer.GetData(vertices);

    // verticesBuffer.Release();
  }

  void GenerateMesh()
  {
    // trianglesBudffer = new ComputeBuffer(triangles.Length, sizeof(float) * 12, ComputeBufferType.Append);
    // trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    // verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float));
    verticesBuffer.SetData(vertices);

    int kernel = meshGenerator.FindKernel("GenerateMesh");
    int numThreadsPerGroup = Mathf.CeilToInt(chunkSize - 1 / (float)threadsCount);

    trianglesBudffer.SetCounterValue(0);
    meshGenerator.SetBuffer(kernel, "_ChunkVertices", verticesBuffer);
    meshGenerator.SetBuffer(kernel, "_ChunkTriangles", trianglesBudffer);
    meshGenerator.SetFloat("_IsoLevel", isoLevel);
    meshGenerator.SetVector("_ChunkPosition", transform.position);
    meshGenerator.SetInt("_ChunkSize", chunkSize);

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

    // trianglesBudffer.Release();
    // trianglesCountBuffer.Release();
    // verticesBuffer.Release();
  }


  void OnDrawGizmosSelected()
  {
    if (Application.isPlaying && vertices != null && chunkSize < 16)
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
