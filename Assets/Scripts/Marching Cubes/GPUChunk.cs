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
  private Vector3[] vegetation = null;

  // Configs
  private int chunkSize = 4;
  private int seed = 42;
  private float isoLevel = 0f;
  private float elevation = 1f;
  private float chunkScale = 1f;
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
  ComputeBuffer vegetationBuffer = null;
  ComputeBuffer vegetationBufferCount = null;

  // Props
  [SerializeField] List<GameObject> grassPrefabs = new List<GameObject>();

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
      isoLevel = surfaceManager.GetIsoLevel();
      elevation = surfaceManager.GetElevation();
      chunkScale = surfaceManager.GetChunkScale();
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
    vegetation = new Vector3[maxTrianglesNumber];
  }

  private void InitializaBuffers()
  {
    trianglesBudffer = new ComputeBuffer(maxTrianglesNumber, sizeof(float) * 12, ComputeBufferType.Append);
    trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    verticesBuffer = new ComputeBuffer(verticesNumber, sizeof(float));
    vegetationBuffer = new ComputeBuffer(maxTrianglesNumber, sizeof(float) * 3);
    vegetationBufferCount = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
  }

  private void ReleaseBuffers()
  {
    trianglesBudffer.Release();
    trianglesCountBuffer.Release();
    verticesBuffer.Release();
    vegetationBuffer.Release();
    vegetationBufferCount.Release();
  }

  void Generate()
  {
    GenerateDensity();
    GenerateMesh();
  }

  void GenerateDensity()
  {
    int kernel = SetupDensityComputeShader();
    int numThreadsPerGroup = Mathf.CeilToInt(chunkSize / (float)threadsCount);
    meshGenerator.Dispatch(kernel, numThreadsPerGroup, numThreadsPerGroup, numThreadsPerGroup);
    verticesBuffer.GetData(vertices);
  }

  void GenerateMesh()
  {
    verticesBuffer.SetData(vertices);
    int kernel = SetupMeshComputeShader();
    int numThreadsPerGroup = Mathf.CeilToInt(chunkSize - 1 / (float)threadsCount);
    meshGenerator.Dispatch(kernel, numThreadsPerGroup, numThreadsPerGroup, numThreadsPerGroup);

    // Get triangles
    ComputeBuffer.CopyCount(trianglesBudffer, trianglesCountBuffer, 0);
    int[] trianglesCount = new int[1];
    trianglesCountBuffer.GetData(trianglesCount);

    Triangle[] generatedTriangles = new Triangle[trianglesCount[0]];
    trianglesBudffer.GetData(generatedTriangles, 0, 0, generatedTriangles.Length);

    GeneratePolygons(trianglesCount[0], generatedTriangles);

    // Get vegetation
    // ComputeBuffer.CopyCount(vegetationBuffer, vegetationBufferCount, 0);
    // int[] vegetationCount = new int[1];
    // vegetationBufferCount.GetData(vegetationCount);
    // Vector3[] generatedVegetation = new Vector3[vegetationCount[0]];
    // vegetationBuffer.GetData(generatedVegetation, 0, 0, generatedVegetation.Length);

    // GenerateVegetation(vegetationCount[0], generatedVegetation);
  }

  private int SetupDensityComputeShader()
  {
    int kernel = meshGenerator.FindKernel("DistributeDensity");

    // Noise maps
    meshGenerator.SetTexture(kernel, "_NoiseMapVol1", noiseMaps[0]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol2", noiseMaps[1]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol3", noiseMaps[2]);

    // Buffers
    meshGenerator.SetBuffer(kernel, "_ChunkVertices", verticesBuffer);

    // Configs
    meshGenerator.SetFloat("_IsoLevel", isoLevel);
    meshGenerator.SetVector("_ChunkPosition", transform.position);
    meshGenerator.SetInt("_ChunkSize", chunkSize);

    return kernel;
  }

  private int SetupMeshComputeShader()
  {
    int kernel = meshGenerator.FindKernel("GenerateMesh");

    // Init data
    verticesBuffer.SetData(vertices);
    trianglesBudffer.SetCounterValue(0);
    vegetationBuffer.SetCounterValue(0);

    // Noise maps
    meshGenerator.SetTexture(kernel, "_NoiseMapVol1", noiseMaps[0]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol2", noiseMaps[1]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol3", noiseMaps[2]);

    // Buffers
    meshGenerator.SetBuffer(kernel, "_ChunkVertices", verticesBuffer);
    meshGenerator.SetBuffer(kernel, "_ChunkTriangles", trianglesBudffer);
    meshGenerator.SetBuffer(kernel, "_ChunkVegetation", vegetationBuffer);

    // Configs
    meshGenerator.SetFloat("_IsoLevel", isoLevel);
    meshGenerator.SetFloat("_Elevation", elevation);
    meshGenerator.SetVector("_ChunkPosition", transform.position);
    meshGenerator.SetFloat("_Scale", chunkScale);
    meshGenerator.SetInt("_ChunkSize", chunkSize);

    return kernel;
  }

  private void GeneratePolygons(int trianglesCount, Triangle[] generatedTriangles)
  {
    int[] meshTriangles = new int[trianglesCount * 3];
    Vector3[] meshVertices = new Vector3[trianglesCount * 3];
    for (int i = 0; i < trianglesCount; i++)
    {
      Triangle tri = generatedTriangles[i];
      int baseIndex = i * 3;
      meshVertices[baseIndex] = tri.a * chunkScale;
      meshVertices[baseIndex + 1] = tri.b * chunkScale;
      meshVertices[baseIndex + 2] = tri.c * chunkScale;

      meshTriangles[baseIndex] = baseIndex;
      meshTriangles[baseIndex + 1] = baseIndex + 1;
      meshTriangles[baseIndex + 2] = baseIndex + 2;
    }

    Mesh mesh = new Mesh();
    mesh.vertices = meshVertices;
    mesh.triangles = meshTriangles;
    mesh.RecalculateNormals();
    meshFilter.mesh = mesh;
  }

  private void GenerateVegetation(int vegetationCount, Vector3[] generatedVegetation)
  {
    for (int i = 0; i < vegetationCount; i++)
    {
      Vector3 pos = transform.position + (generatedVegetation[i] * chunkScale);
      Instantiate(grassPrefabs[0], pos, Quaternion.identity, transform);
    }
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
            float shade = density < isoLevel ? 1f : 0f;
            Gizmos.color = new Color(shade, shade, shade, 1f);
            Gizmos.DrawSphere((new Vector3(x, y, z) * chunkScale) + transform.position, 0.03f);
          }
        }
      }
    }
    Gizmos.color = Color.white;
    Vector3 dimension = new Vector3(chunkSize - 1, chunkSize - 1, chunkSize - 1) * chunkScale;
    Gizmos.DrawWireCube(transform.position + dimension / 2f, dimension);
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
