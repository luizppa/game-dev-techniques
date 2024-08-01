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

public struct ChunkData
{
  public float[] vertices;
  public Mesh mesh;
  public RenderTexture biomeOutput1;
  public RenderTexture biomeOutput2;
  public RenderTexture biomeOutput3;
  public RenderTexture biomeOutput4;
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class GPUChunk : MonoBehaviour
{
  // Constants
  private const int threadsCount = 8;
  private const int maxTrianglesPerVoxel = 5;
  private const int triangleMemorySize = sizeof(float) * 3 * 4;
  private const int vector3MemorySize = sizeof(float) * 3;
  private const int vector4MemorySize = sizeof(float) * 4;


  // Compute shader
  [SerializeField] ComputeShader meshGenerator = null;

  // Mesh
  private float[] vertices = null;
  private Triangle[] triangles = null;
  private Vector3[] vegetation = null;

  // Configs
  private string id;
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

  // References
  private SurfaceManager surfaceManager = null;
  private EnvironmentManager environmentManager = null;
  private MeshFilter meshFilter = null;
  private MeshCollider meshCollider = null;
  private GameObject boid = null;
  private List<GameObject> vegetationInstances = new List<GameObject>();

  // Buffers
  ComputeBuffer trianglesBuffer = null;
  ComputeBuffer trianglesCountBuffer = null;
  ComputeBuffer verticesBuffer = null;
  ComputeBuffer vegetationBuffer = null;
  ComputeBuffer vegetationBufferCount = null;

  // Biome maps
  public RenderTexture biomeOutput1 = null;
  public RenderTexture biomeOutput2 = null;
  public RenderTexture biomeOutput3 = null;
  public RenderTexture biomeOutput4 = null;

  // Props
  [SerializeField] List<GameObject> grassPrefabs = new List<GameObject>();
  [SerializeField] List<GameObject> shallowWaterPrefabs = new List<GameObject>();
  [SerializeField] List<GameObject> treePrefabs = new List<GameObject>();
  [SerializeField] List<GameObject> rockPrefabs = new List<GameObject>();
  [SerializeField] List<GameObject> bushPrefabs = new List<GameObject>();
  [SerializeField] List<GameObject> flowerPrefabs = new List<GameObject>();
  [SerializeField] List<GameObject> coralPrefabs = new List<GameObject>();

  [SerializeField] List<GameObject> boidUnitPrefabs = new List<GameObject>();
  [SerializeField] uint boidsNumber = 100;

  void Start()
  {
    GetConfig();
    InitializeProperties();
    InitializeBuffers();
    Generate();
  }

  void OnDisable()
  {
    ReleaseBuffers();
  }

  void OnDestroy()
  {
    surfaceManager.SetChunkCache(id, new ChunkData { vertices = vertices, mesh = meshFilter.mesh, biomeOutput1 = biomeOutput1, biomeOutput2 = biomeOutput2, biomeOutput3 = biomeOutput3, biomeOutput4 = biomeOutput4 });
    Destroy(boid);
  }

  private void GetConfig()
  {
    surfaceManager = SurfaceManager.Instance;
    environmentManager = EnvironmentManager.Instance;
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
    meshCollider = GetComponent<MeshCollider>();
    voxelsNumber = (chunkSize - 1) * (chunkSize - 1) * (chunkSize - 1);
    verticesNumber = chunkSize * chunkSize * chunkSize;
    maxTrianglesNumber = voxelsNumber * maxTrianglesPerVoxel;

    vertices = new float[verticesNumber];
    triangles = new Triangle[maxTrianglesNumber];
    vegetation = new Vector3[maxTrianglesNumber];
  }

  private void InitializeBuffers()
  {
    trianglesBuffer = new ComputeBuffer(maxTrianglesNumber, triangleMemorySize, ComputeBufferType.Append);
    trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    verticesBuffer = new ComputeBuffer(verticesNumber, sizeof(float));
    vegetationBuffer = new ComputeBuffer(maxTrianglesNumber, vector4MemorySize, ComputeBufferType.Append);
    vegetationBufferCount = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

    biomeOutput1 = new RenderTexture(32, 32, 0, RenderTextureFormat.ARGBFloat);
    biomeOutput1.enableRandomWrite = true;
    biomeOutput1.Create();

    biomeOutput2 = new RenderTexture(32, 32, 0, RenderTextureFormat.ARGBFloat);
    biomeOutput2.enableRandomWrite = true;
    biomeOutput2.Create();

    biomeOutput3 = new RenderTexture(32, 32, 0, RenderTextureFormat.ARGBFloat);
    biomeOutput3.enableRandomWrite = true;
    biomeOutput3.Create();

    biomeOutput4 = new RenderTexture(32, 32, 0, RenderTextureFormat.ARGBFloat);
    biomeOutput4.enableRandomWrite = true;
    biomeOutput4.Create();
  }

  private void ReleaseBuffers()
  {
    trianglesBuffer.Release();
    trianglesCountBuffer.Release();
    vegetationBuffer.Release();
    vegetationBufferCount.Release();
    verticesBuffer.Release();
  }

  void Generate()
  {
    ChunkData chunkCache;
    if (surfaceManager.GetChunkCache(id, out chunkCache))
    {
      vertices = chunkCache.vertices;
      meshFilter.mesh = chunkCache.mesh;
      meshCollider.sharedMesh = meshFilter.mesh;
      biomeOutput1 = chunkCache.biomeOutput1;
      biomeOutput2 = chunkCache.biomeOutput2;
      biomeOutput3 = chunkCache.biomeOutput3;
    }
    else
    {
      GenerateDensity();
      GenerateMesh();
    }
    SetMaterialBiomeMaps();
    GenerateBoids();
  }

  void GenerateDensity()
  {
    int kernel = SetupDensityComputeShader();
    int numThreadsPerGroup = Mathf.CeilToInt(chunkSize / (float)threadsCount);
    meshGenerator.Dispatch(kernel, numThreadsPerGroup, numThreadsPerGroup, numThreadsPerGroup);
    verticesBuffer.GetData(vertices);
  }

  void GenerateMesh(bool generateVegetation = true)
  {
    verticesBuffer.SetData(vertices);
    int kernel = SetupMeshComputeShader();
    int numThreadsPerGroup = Mathf.CeilToInt(chunkSize - 1 / (float)threadsCount);
    meshGenerator.Dispatch(kernel, numThreadsPerGroup, numThreadsPerGroup, numThreadsPerGroup);

    // Get triangles
    ComputeBuffer.CopyCount(trianglesBuffer, trianglesCountBuffer, 0);
    int[] trianglesCount = new int[1];
    trianglesCountBuffer.GetData(trianglesCount);
    Triangle[] generatedTriangles = new Triangle[trianglesCount[0]];
    trianglesBuffer.GetData(generatedTriangles, 0, 0, generatedTriangles.Length);
    GeneratePolygons(trianglesCount[0], generatedTriangles);

    // Get vegetation
    if (generateVegetation)
    {
      ComputeBuffer.CopyCount(vegetationBuffer, vegetationBufferCount, 0);
      int[] vegetationCount = new int[1];
      vegetationBufferCount.GetData(vegetationCount);
      Vector4[] generatedVegetation = new Vector4[vegetationCount[0]];
      vegetationBuffer.GetData(generatedVegetation, 0, 0, generatedVegetation.Length);
      GenerateVegetation(vegetationCount[0], generatedVegetation);
    }
  }

  void SetMaterialBiomeMaps()
  {
    Material material = meshFilter.GetComponent<Renderer>().material;
    material.SetTexture("_BiomeMap1", biomeOutput1);
    material.SetTexture("_BiomeMap2", biomeOutput2);
    material.SetTexture("_BiomeMap3", biomeOutput3);
    material.SetTexture("_BiomeMap4", biomeOutput4);
  }

  public void Terraform(Vector3 position, float radius, float strength, TerraformMode mode)
  {
    Vector3 localPosition = transform.InverseTransformPoint(position);

    verticesBuffer.SetData(vertices);
    int kernel = SetupTerraformComputeShader(localPosition, radius, strength, mode);
    int numThreadsPerGroup = Mathf.CeilToInt(chunkSize - 1 / (float)threadsCount);
    meshGenerator.Dispatch(kernel, numThreadsPerGroup, numThreadsPerGroup, numThreadsPerGroup);
    verticesBuffer.GetData(vertices);
    GenerateMesh(false);
  }

  void GenerateBoids()
  {
    if (boidUnitPrefabs.Count > 0)
    {
      float size = chunkSize * chunkScale;

      boid = new GameObject();
      boid.name = "Boids " + this.name;

      Boid boidComponent = boid.AddComponent<Boid>();
      boidComponent.SetUnitPrefab(boidUnitPrefabs[Random.Range(0, boidUnitPrefabs.Count)]);
      boidComponent.SetUnits(boidsNumber);

      Vector3 chunkCenter = transform.position + new Vector3(size / 2, (size / 2) - 2, size / 2);
      boidComponent.SetNavigationBounds(new Bounds(chunkCenter, new Vector3(size, size - 4f, size)));
      boidComponent.SetSpawnBounds(new Bounds(chunkCenter + (Vector3.up * (size - 5) / 2f), new Vector3(size, 1, size)));
    }
  }

  private int SetupDensityComputeShader()
  {
    int kernel = meshGenerator.FindKernel("DistributeDensity");

    // Noise maps
    meshGenerator.SetTexture(kernel, "_NoiseMapVol1", noiseMaps[0]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol2", noiseMaps[1]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol3", noiseMaps[2]);

    // Biome maps
    meshGenerator.SetTexture(kernel, "_ErosionMap", environmentManager.GetBiomeMap("Erosion"));
    meshGenerator.SetTexture(kernel, "_TemperatureMap", environmentManager.GetBiomeMap("Temperature"));
    meshGenerator.SetTexture(kernel, "_PrecipitationMap", environmentManager.GetBiomeMap("Precipitation"));
    meshGenerator.SetTexture(kernel, "_SeismicMap", environmentManager.GetBiomeMap("Seismic Activity"));

    meshGenerator.SetTexture(kernel, "_BiomeOutput1", biomeOutput1);
    meshGenerator.SetTexture(kernel, "_BiomeOutput2", biomeOutput2);
    meshGenerator.SetTexture(kernel, "_BiomeOutput3", biomeOutput3);
    meshGenerator.SetTexture(kernel, "_BiomeOutput4", biomeOutput4);

    // Biome Features
    meshGenerator.SetFloat("_LowFeatureThreshold", environmentManager.GetLowFeatureThreshold());
    meshGenerator.SetFloat("_HighFeatureThreshold", environmentManager.GetHighFeatureThreshold());
    meshGenerator.SetFloat("_BiomeStep", environmentManager.GetBiomeStep());

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
    trianglesBuffer.SetCounterValue(0);
    vegetationBuffer.SetCounterValue(0);

    // Noise maps
    meshGenerator.SetTexture(kernel, "_NoiseMapVol1", noiseMaps[0]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol2", noiseMaps[1]);
    meshGenerator.SetTexture(kernel, "_NoiseMapVol3", noiseMaps[2]);

    // Buffers
    meshGenerator.SetBuffer(kernel, "_ChunkVertices", verticesBuffer);
    meshGenerator.SetBuffer(kernel, "_ChunkTriangles", trianglesBuffer);
    meshGenerator.SetBuffer(kernel, "_ChunkVegetation", vegetationBuffer);

    // Configs
    meshGenerator.SetFloat("_IsoLevel", isoLevel);
    meshGenerator.SetFloat("_Elevation", elevation);
    meshGenerator.SetVector("_ChunkPosition", transform.position);
    meshGenerator.SetFloat("_Scale", chunkScale);
    meshGenerator.SetInt("_ChunkSize", chunkSize);

    return kernel;
  }

  private int SetupTerraformComputeShader(Vector3 position, float radius, float strength, TerraformMode mode)
  {
    int kernel = meshGenerator.FindKernel("Terraform");

    // Init data
    verticesBuffer.SetData(vertices);

    // Terraform properties
    meshGenerator.SetBuffer(kernel, "_ChunkVertices", verticesBuffer);
    meshGenerator.SetVector("_TerraformPosition", position);
    meshGenerator.SetFloat("_TerraformRadius", radius);
    meshGenerator.SetFloat("_TerraformStrength", strength);
    meshGenerator.SetInt("_TerraformDirection", ((int)mode));

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
    meshCollider.sharedMesh = mesh;
  }

  private void GenerateVegetation(int vegetationCount, Vector4[] generatedVegetation)
  {
    foreach (GameObject vegetation in vegetationInstances)
    {
      Destroy(vegetation);
    }
    vegetationInstances.Clear();
    Random.InitState(seed);

    if (grassPrefabs.Count > 0)
    {
      for (int i = 0; i < vegetationCount; i++)
      {
        Vector3 pos = transform.position + (Vector4ToVector3(generatedVegetation[i]) * chunkScale);
        float dotProd = generatedVegetation[i].w;

        if (pos.y >= EnvironmentManager.Instance.GetWaterLevel())
        {
          InsertDryLandVegetation(pos, dotProd);
        }
        else
        {
          InsertUnderwaterVegetation(pos, dotProd);
        }
      }
    }
  }

  void InsertDryLandVegetation(Vector3 pos, float dotProd)
  {
    GameObject prefab;
    float scale = 1f;

    float random = Random.Range(0f, 1f);

    if (random < 0.3)
    {
      if (treePrefabs.Count <= 0)
      {
        return;
      }
      prefab = treePrefabs[Random.Range(0, treePrefabs.Count)];
    }
    else if (random < 0.6)
    {
      if (rockPrefabs.Count <= 0)
      {
        return;
      }
      prefab = rockPrefabs[Random.Range(0, rockPrefabs.Count)];
      scale = Random.Range(0.5f, 1.5f);
    }
    else
    {
      if (bushPrefabs.Count <= 0)
      {
        return;
      }
      prefab = bushPrefabs[Random.Range(0, grassPrefabs.Count)];
    }

    GameObject vegetation = Instantiate(prefab, pos, Quaternion.identity, transform);
    vegetation.transform.Rotate(0, Random.Range(0f, 360f), 0f);
    vegetation.transform.localScale *= scale;
    vegetationInstances.Add(vegetation);
  }

  void InsertUnderwaterVegetation(Vector3 pos, float dotProd)
  {
    GameObject prefab = null;
    float angle = Mathf.Acos(dotProd) * Mathf.Rad2Deg;
    float scale = 1f;

    if (EnvironmentManager.Instance.GetWaterLevel() - pos.y < 2f)
    {
      if (shallowWaterPrefabs.Count <= 0)
      {
        return;
      }
      prefab = shallowWaterPrefabs[Random.Range(0, shallowWaterPrefabs.Count)];
      angle = 0f;
      scale = 3f;
    }
    else if (dotProd > 0.7f)
    {
      if (grassPrefabs.Count <= 0)
      {
        return;
      }
      prefab = grassPrefabs[Random.Range(0, grassPrefabs.Count)];
      scale = .6f;
    }
    else if (dotProd < -0.3f)
    {
      if (coralPrefabs.Count <= 0)
      {
        return;
      }
      prefab = coralPrefabs[Random.Range(0, coralPrefabs.Count)];
      // angle = 180f;
      scale = 3f;
    }

    if (prefab == null)
    {
      return;
    }


    GameObject vegetation = Instantiate(prefab, pos, Quaternion.identity, transform);
    Vector3 normal = Quaternion.AngleAxis(-angle, vegetation.transform.forward) * vegetation.transform.up;
    vegetation.transform.Rotate(0, Random.Range(0f, 360f), 0f);
    vegetation.transform.up = normal;
    vegetation.transform.localScale *= scale;
    vegetationInstances.Add(vegetation);
  }

  private Vector3 Vector4ToVector3(Vector4 vec)
  {
    return new Vector3(vec.x, vec.y, vec.z);
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
    float actualSize = (chunkSize - 1) * chunkScale;
    Vector3 dimension = Vector3.one * actualSize;
    Gizmos.DrawWireCube(transform.position + dimension / 2f, dimension);
  }

  public int PositionToBufferIndex(int x, int y, int z)
  {
    return x + (y * chunkSize) + (z * chunkSize * chunkSize);
  }

  public Vector3 BufferIndexToPosition(int idx)
  {
    int x = idx % chunkSize;
    int y = ((idx - x) / chunkSize) % chunkSize;
    int z = ((((idx - x) / chunkSize) - y) / chunkSize) % chunkSize;
    return new Vector3(x, y, z);
  }

  public void SetId(string id)
  {
    this.id = id;
  }
}
