using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GenerationTechology
{
  CPU,
  GPU
}

public class SurfaceManager : MonoBehaviour
{
  [SerializeField] int chunksPerDirection = 1;
  [SerializeField] Transform playerPosition = null;

  [Header("Chunk Configuration")]
  [SerializeField][Range(1, 32)] int chunkSize = 8;
  [SerializeField] float chunkDensity = 1f;
  [SerializeField][Range(-1f, 1f)] float isoLevel = 0.5f;
  [SerializeField] int seed = 0;
  [SerializeField] float elevation = 1f;

  [Header("Generation Technology")]
  [SerializeField] GenerationTechology generationTechnology = GenerationTechology.CPU;
  [SerializeField] GameObject CPUChunkPrefab = null;
  [SerializeField] GameObject GPUChunkPrefab = null;

  [Header("Noise Configuration")]
  [SerializeField] int noiseLevels = 2;
  [SerializeField] Vector2Int noiseResolution = new Vector2Int(256, 256);
  public List<Texture2D> noiseMaps = new List<Texture2D>();

  private GameObject[,] chunks = null;
  private float previousIsoLevel = 0f;
  private int previousSeed = 0;
  private float previousElevation = 0f;

  private Vector2 centralPosition = new Vector2(0, 0);
  private Vector2 previousCentralPosition;

  void Awake()
  {
    GenerateNoiseMaps();
  }

  void Start()
  {
    if (FindObjectsOfType<SurfaceManager>().Length > 1)
    {
      Destroy(gameObject);
    }
    else
    {
      DontDestroyOnLoad(gameObject);
    }
    GenerateNoiseMaps();
    InitializeProperties();
    CreateChunks();
  }

  void Update()
  {
    if (SceneManager.GetActiveScene().name != "MarchingCubes")
    {
      Destroy(gameObject);
    }
    else if (ShouldUpdate())
    {
      UpdateProperties();
      UpdateChunks();
    }
  }

  void GenerateNoiseMaps()
  {
    noiseMaps.Clear();
    for (int i = 0; i < noiseLevels; i++)
    {
      float scale = Mathf.Pow(10f, i + 1);
      Texture2D noiseMap = new Texture2D(noiseResolution.x, noiseResolution.y);
      for (int x = 0; x < noiseResolution.x; x++)
      {
        for (int y = 0; y < noiseResolution.y; y++)
        {
          noiseMap.SetPixel(x, y, CalculateNoiseColor(x, y, scale));
        }
      }
      noiseMap.Apply();
      noiseMaps.Add(noiseMap);
    }
  }

  private Color CalculateNoiseColor(int x, int y, float scale)
  {
    float xCord = (float)x / noiseResolution.x * scale;
    float yCord = (float)y / noiseResolution.y * scale;

    float sample = Mathf.PerlinNoise(seed + xCord, seed + yCord);
    return new Color(sample, sample, sample);
  }

  void InitializeProperties()
  {
    previousIsoLevel = isoLevel;
    previousSeed = seed;
    previousElevation = elevation;

    int chunkCount = (chunksPerDirection * 2) + 1;
    chunks = new GameObject[chunkCount, chunkCount];

    float playerX = playerPosition.position.x;
    float playerZ = playerPosition.position.z;
    centralPosition = new Vector2(Mathf.Floor(playerX / (chunkSize * chunkDensity)), Mathf.Floor(playerZ / (chunkSize * chunkDensity)));
    previousCentralPosition = centralPosition;
  }

  private bool ShouldUpdate()
  {
    float playerX = playerPosition.position.x;
    float playerZ = playerPosition.position.z;
    centralPosition = new Vector2(Mathf.Floor(playerX / (chunkSize * chunkDensity)), Mathf.Floor(playerZ / (chunkSize * chunkDensity)));

    return centralPosition - previousCentralPosition != Vector2.zero || previousIsoLevel != isoLevel || previousSeed != seed || previousElevation != elevation;
  }

  private void UpdateProperties()
  {
    previousIsoLevel = isoLevel;
    previousSeed = seed;
    previousElevation = elevation;
  }

  private void CreateChunks()
  {
    GameObject chunkPrefab = GetPrefab();
    if (chunkPrefab != null)
    {
      for (int x = -chunksPerDirection; x <= chunksPerDirection; x++)
      {
        for (int z = -chunksPerDirection; z <= chunksPerDirection; z++)
        {
          if (chunks[chunksPerDirection + x, chunksPerDirection + z] == null)
          {
            chunks[chunksPerDirection + x, chunksPerDirection + z] = CreateChunk(chunkPrefab, centralPosition.x + x, centralPosition.y + z);
          }
        }
      }
    }
  }

  private void UpdateChunks()
  {
    if (centralPosition.x != previousCentralPosition.x)
    {
      ShiftChunkRow();
    }

    if (centralPosition.y != previousCentralPosition.y)
    {
      ShiftChunkColumn();
    }
    previousCentralPosition = centralPosition;
    CreateChunks();
  }

  private void ShiftChunkRow()
  {
    int shiftValue = centralPosition.x > previousCentralPosition.x ? 1 : -1;

    // Remove
    int discardRow = shiftValue > 0 ? 0 : chunksPerDirection * 2;
    for (int z = 0; z <= chunksPerDirection * 2; z++)
    {
      GameObject chunk = chunks[discardRow, z];
      if (chunk != null)
      {
        Destroy(chunk);
      }
    }

    // Shift
    if (shiftValue > 0)
    {
      for (int x = 0; x < chunksPerDirection * 2; x++)
      {
        for (int z = 0; z <= chunksPerDirection * 2; z++)
        {
          chunks[x, z] = chunks[x + 1, z];
        }
      }
    }
    else
    {
      for (int x = chunksPerDirection * 2; x > 0; x--)
      {
        for (int z = 0; z <= chunksPerDirection * 2; z++)
        {
          chunks[x, z] = chunks[x - 1, z];
        }
      }
    }

    // Set null
    int nullRow = shiftValue > 0 ? chunksPerDirection * 2 : 0;
    for (int z = 0; z <= chunksPerDirection * 2; z++)
    {
      chunks[nullRow, z] = null;
    }
  }



  private void ShiftChunkColumn()
  {
    int shiftValue = centralPosition.y > previousCentralPosition.y ? 1 : -1;

    // Remove
    int discardColumn = shiftValue > 0 ? 0 : chunksPerDirection * 2;
    for (int x = 0; x <= chunksPerDirection * 2; x++)
    {
      GameObject chunk = chunks[x, discardColumn];
      if (chunk != null)
      {
        Destroy(chunk);
      }
    }

    // Shift
    if (shiftValue > 0)
    {
      for (int x = 0; x <= chunksPerDirection * 2; x++)
      {
        for (int z = 0; z < chunksPerDirection * 2; z++)
        {
          chunks[x, z] = chunks[x, z + 1];
        }
      }
    }
    else
    {
      for (int x = 0; x <= chunksPerDirection * 2; x++)
      {
        for (int z = chunksPerDirection * 2; z > 0; z--)
        {
          chunks[x, z] = chunks[x, z - 1];
        }
      }
    }

    // Set null
    int nullColumn = shiftValue > 0 ? chunksPerDirection * 2 : 0;
    for (int x = 0; x <= chunksPerDirection * 2; x++)
    {
      chunks[x, nullColumn] = null;
    }
  }

  private GameObject CreateChunk(GameObject chunkPrefab, float x, float z)
  {
    if (chunkPrefab != null)
    {
      GameObject chunk = Instantiate(chunkPrefab, new Vector3(x * (chunkSize - 1) * chunkDensity, 0f, z * (chunkSize - 1) * chunkDensity), Quaternion.identity);
      chunk.name = "Chunk " + x + ", " + z;
      return chunk;
    }
    return null;
  }

  private GameObject GetPrefab()
  {
    if (generationTechnology == GenerationTechology.CPU)
    {
      return CPUChunkPrefab;
    }
    else
    {
      return GPUChunkPrefab;
    }
  }

  public List<Texture2D> GetNoiseMaps()
  {
    return noiseMaps;
  }

  public int GetChunkSize()
  {
    return chunkSize;
  }

  public float GetChunkDensity()
  {
    return chunkDensity;
  }

  public float GetIsoLevel()
  {
    return isoLevel;
  }

  public int GetSeed()
  {
    return seed;
  }

  public float GetElevation()
  {
    return elevation;
  }
}
