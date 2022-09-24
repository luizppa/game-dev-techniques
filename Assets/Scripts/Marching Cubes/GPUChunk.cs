using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Triangle
{
  Vector3 vertexC;
  Vector3 vertexB;
  Vector3 vertexA;
};

public class GPUChunk : MonoBehaviour
{
  // Compute shader
  private const int threadsCount = 8;
  [SerializeField] ComputeShader meshGenerator = null;

  // Mesh
  private float[] vertices = null;

  // Configs
  private int chunkSize = 4;
  private int seed = 42;
  private List<Texture2D> noiseMaps = new List<Texture2D>();

  // Managers
  private SurfaceManager surfaceManager = null;

  void Start()
  {
    surfaceManager = FindObjectOfType<SurfaceManager>();
    vertices = new float[chunkSize * chunkSize * chunkSize];
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
  }

  void GenerateDensity()
  {
    ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float));

    int kernel = meshGenerator.FindKernel("DistributeDensity");
    int numThreads = Mathf.CeilToInt(chunkSize / (float)threadsCount);

    meshGenerator.SetBuffer(kernel, "_ChunkVertices", verticesBuffer);
    meshGenerator.SetInt("_ChunkSize", chunkSize);
    meshGenerator.SetVector("_ChunkPosition", transform.position);
    meshGenerator.Dispatch(kernel, numThreads, numThreads, numThreads);

    verticesBuffer.GetData(vertices);
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
