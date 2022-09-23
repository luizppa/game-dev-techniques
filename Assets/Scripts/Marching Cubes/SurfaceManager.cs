using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SurfaceManager : MonoBehaviour
{
  [SerializeField] int chunksPerDirection = 1;
  [SerializeField] Transform playerPosition = null;

  [Header("Chunk Configuration")]
  [SerializeField] int chunkHeight = 10;
  [SerializeField] int chunkWidth = 10;
  [SerializeField] int chunkDepth = 10;
  [SerializeField] float chunkDensity = 1f;
  [SerializeField][Range(0f, 1f)] float isoLevel = 0.5f;
  [SerializeField] int seed = 0;
  [SerializeField] float elevation = 1f;

  [SerializeField] GameObject chunkPrefab = null;


  private GameObject[,] chunks = null;
  private float previousIsoLevel = 0f;
  private int previousSeed = 0;
  private float previousElevation = 0f;

  private Vector2 centralPosition = new Vector2(0, 0);
  private Vector2 previousCentralPosition;

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

  void InitializeProperties()
  {
    previousIsoLevel = isoLevel;
    previousSeed = seed;
    previousElevation = elevation;

    int chunkCount = (chunksPerDirection * 2) + 1;
    chunks = new GameObject[chunkCount, chunkCount];

    float playerX = playerPosition.position.x;
    float playerZ = playerPosition.position.z;
    centralPosition = new Vector2(Mathf.Floor(playerX / (chunkWidth * chunkDensity)), Mathf.Floor(playerZ / (chunkDepth * chunkDensity)));
    previousCentralPosition = centralPosition;
  }

  private bool ShouldUpdate()
  {
    float playerX = playerPosition.position.x;
    float playerZ = playerPosition.position.z;
    centralPosition = new Vector2(Mathf.Floor(playerX / (chunkWidth * chunkDensity)), Mathf.Floor(playerZ / (chunkDepth * chunkDensity)));

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
    if (chunkPrefab != null)
    {
      for (int x = -chunksPerDirection; x <= chunksPerDirection; x++)
      {
        for (int z = -chunksPerDirection; z <= chunksPerDirection; z++)
        {
          if (chunks[chunksPerDirection + x, chunksPerDirection + z] == null)
          {
            chunks[chunksPerDirection + x, chunksPerDirection + z] = CreateChunk(centralPosition.x + x, centralPosition.y + z);
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

  private GameObject CreateChunk(float x, float z)
  {
    if (chunkPrefab != null)
    {
      GameObject chunk = Instantiate(chunkPrefab, new Vector3(x * (chunkWidth - 1) * chunkDensity, 0f, z * (chunkDepth - 1) * chunkDensity), Quaternion.identity);
      chunk.name = "Chunk " + x + ", " + z;
      return chunk;
    }
    return null;
  }

  public int getChunkHeight()
  {
    return chunkHeight;
  }

  public int getChunkWidth()
  {
    return chunkWidth;
  }

  public int getChunkDepth()
  {
    return chunkDepth;
  }

  public float getChunkDensity()
  {
    return chunkDensity;
  }

  public float getIsoLevel()
  {
    return isoLevel;
  }

  public int getSeed()
  {
    return seed;
  }

  public float getElevation()
  {
    return elevation;
  }
}
