using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceManager : MonoBehaviour
{
  [SerializeField] int chunksPerDirection = 1;
  [SerializeField] Transform relevantPosition = null;

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
  private float previousH = 0f;

  private float chunkX = 0f;
  private float previousChunkX = 0f;
  private float chunkZ = 0f;
  private float previousChunkZ = 0f;

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
    float playerX = relevantPosition.position.x;
    float playerZ = relevantPosition.position.z;
    chunkX = Mathf.Floor(playerX / (chunkWidth * chunkDensity));
    chunkZ = Mathf.Floor(playerZ / (chunkDepth * chunkDensity));

    if (previousIsoLevel != isoLevel || previousSeed != seed || previousH != elevation || previousChunkX != chunkX || previousChunkZ != chunkZ)
    {
      previousIsoLevel = isoLevel;
      previousSeed = seed;
      previousH = elevation;
      previousChunkX = chunkX;
      previousChunkZ = chunkZ;
      ReloadChunks();
    }
  }

  void InitializeProperties()
  {
    previousIsoLevel = isoLevel;
    previousSeed = seed;
    int chunkCount = (chunksPerDirection * 2) + 1;
    chunks = new GameObject[chunkCount, chunkCount];

    float playerX = relevantPosition.position.x;
    float playerZ = relevantPosition.position.z;
    chunkX = Mathf.Floor(playerX / (chunkWidth * chunkDensity));
    chunkZ = Mathf.Floor(playerZ / (chunkDepth * chunkDensity));
    previousChunkX = chunkX;
    previousChunkZ = chunkZ;
  }

  private void CreateChunks()
  {
    if (chunkPrefab != null)
    {
      CreateChunk(chunkX, chunkZ);
      for (int x = -chunksPerDirection; x <= chunksPerDirection; x++)
      {
        for (int z = -chunksPerDirection; z <= chunksPerDirection; z++)
        {
          CreateChunk(chunkX + x, chunkZ + z);
        }
      }
    }
  }

  private void CreateChunk(float x, float z)
  {
    if (chunkPrefab != null)
    {
      GameObject chunk = Instantiate(chunkPrefab, new Vector3(x * chunkWidth * chunkDensity, 0f, z * chunkDepth * chunkDensity), Quaternion.identity);
      chunk.name = "Chunk " + x + ", " + z;
      chunks.Add(chunk);
    }
  }

  private void ReloadChunks()
  {
    // Debug.Log(chunks.Count);
    foreach (GameObject chunk in chunks)
    {
      Debug.Log("Destroying chunk " + chunk.name);
      Destroy(chunk);
    }
    chunks.Clear();
    // Debug.Log(chunks.Count);
    CreateChunks();
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
