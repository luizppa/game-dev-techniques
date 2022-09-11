using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
  [SerializeField] private GameObject vertexPrefab = null;

  int chunkHeight = 10;
  int chunkWidth = 10;
  int chunkDepth = 10;
  float chunkDensity = 1f;
  float isoLevel = 0.5f;
  int seed = 0;

  private GameObject[,,] vertices = null;

  private SurfaceManager surfaceManager = null;

  // Start is called before the first frame update
  void Start()
  {
    vertices = new GameObject[chunkWidth, chunkHeight, chunkDepth];
    surfaceManager = FindObjectOfType<SurfaceManager>();
    GetConfig();
    GenerateChunk();
  }

  private void GetConfig()
  {
    if (surfaceManager != null)
    {
      chunkHeight = surfaceManager.getChunkHeight();
      chunkWidth = surfaceManager.getChunkWidth();
      chunkDepth = surfaceManager.getChunkDepth();
      chunkDensity = surfaceManager.getChunkDensity();
      isoLevel = surfaceManager.getIsoLevel();
      seed = surfaceManager.getSeed();
    }
  }

  private void GenerateChunk()
  {
    Random.InitState((int)Mathf.Floor(seed * transform.position.magnitude));
    DistributeVertices();
  }

  private void DistributeVertices()
  {
    if (vertexPrefab != null)
    {
      for (int x = 0; x < chunkWidth; x++)
      {
        for (int y = 0; y < chunkHeight; y++)
        {
          for (int z = 0; z < chunkDepth; z++)
          {
            Vector3 position = new Vector3(x, y, z);
            vertices[x, y, z] = Instantiate(vertexPrefab, position * chunkDensity, Quaternion.identity, transform);
            vertices[x, y, z].GetComponent<Vertex>().SetValue(GenerateValue(position));
          }
        }
      }
    }
  }

  float GenerateValue(Vector3 position)
  {
    float value = Mathf.Clamp01(Random.Range(0f, (float)chunkHeight) / (1f + position.y));
    // Debug.Log(position.y + ": " + value);
    return value;
  }

  void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.red;
    for (int x = 0; x < chunkWidth; x++)
    {
      for (int y = 0; y < chunkHeight; y++)
      {
        for (int z = 0; z < chunkDepth; z++)
        {
          Gizmos.DrawLine(new Vector3(0, y, z), new Vector3(chunkWidth - 1, y, z));
          Gizmos.DrawLine(new Vector3(x, 0, z), new Vector3(x, chunkHeight - 1, z));
          Gizmos.DrawLine(new Vector3(x, y, 0), new Vector3(x, y, chunkDepth - 1));
        }
      }
    }
  }
}
