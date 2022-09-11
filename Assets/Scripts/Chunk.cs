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

  private Vertex[,,] vertices = null;

  private SurfaceManager surfaceManager = null;

  void Start()
  {
    vertices = new Vertex[chunkWidth, chunkHeight, chunkDepth];
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
    GenerateMesh();
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
            GameObject vertex = Instantiate(vertexPrefab, position * chunkDensity, Quaternion.identity, transform);

            vertices[x, y, z] = vertex.GetComponent<Vertex>();
            vertices[x, y, z].SetValue(GenerateValue(position));
          }
        }
      }
    }
  }

  private void GenerateMesh()
  {
    for (int x = 0; x < chunkWidth - 1; x++)
    {
      for (int y = 0; y < chunkHeight - 1; y++)
      {
        for (int z = 0; z < chunkDepth - 1; z++)
        {
          int cubeIndex = GetCubeIndex(x, y, z);
        }
      }
    }
  }

  private int GetCubeIndex(int x, int y, int z)
  {
    int cubeindex = 0;

    if (vertices[x, y, z + 1].GetValue() > isoLevel) cubeindex |= 1;
    if (vertices[x + 1, y, z + 1].GetValue() > isoLevel) cubeindex |= 2;
    if (vertices[x + 1, y, z].GetValue() > isoLevel) cubeindex |= 4;
    if (vertices[x, y, z].GetValue() > isoLevel) cubeindex |= 8;
    if (vertices[x, y + 1, z + 1].GetValue() > isoLevel) cubeindex |= 16;
    if (vertices[x + 1, y + 1, z + 1].GetValue() > isoLevel) cubeindex |= 32;
    if (vertices[x + 1, y + 1, z].GetValue() > isoLevel) cubeindex |= 64;
    if (vertices[x, y + 1, z].GetValue() > isoLevel) cubeindex |= 128;

    return cubeindex;
  }

  float GenerateValue(Vector3 position)
  {
    float value = Mathf.Clamp01(Random.Range(0f, (float)chunkHeight) / (1f + position.y));
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
