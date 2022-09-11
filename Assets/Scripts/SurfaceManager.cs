using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceManager : MonoBehaviour
{
  [SerializeField] int chunkHeight = 10;
  [SerializeField] int chunkWidth = 10;
  [SerializeField] int chunkDepth = 10;
  [SerializeField] float chunkDensity = 1f;
  [SerializeField][Range(0f, 1f)] float isoLevel = 0.5f;
  [SerializeField] int seed = 0;


  [SerializeField] GameObject chunkPrefab = null;


  private GameObject chunk = null;
  private float previousIsoLevel = 0f;
  private int previousSeed = 0;

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

    previousIsoLevel = isoLevel;
    previousSeed = seed;
    CreateChunk();
  }

  void Update()
  {
    if (previousIsoLevel != isoLevel || previousSeed != seed)
    {
      previousIsoLevel = isoLevel;
      previousSeed = seed;
      ReloadChunk();
    }
  }

  private void CreateChunk()
  {
    if (chunkPrefab != null)
    {
      chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
    }
  }

  private void ReloadChunk()
  {
    if (chunk != null)
    {
      Destroy(chunk);
      CreateChunk();
    }
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
}
