using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSurface : MonoBehaviour
{
  [SerializeField] GameObject player = null;
  [SerializeField] Vector2Int size = Vector2Int.one * 10;

  private EnvironmentManager environmentManager = null;
  private Mesh mesh = null;

  void Start()
  {
    environmentManager = FindObjectOfType<EnvironmentManager>();
    GenerateMesh();
  }

  void Update()
  {
    if (player)
    {
      Vector3 offset = new Vector3(size.x / 2f * transform.lossyScale.x, 0, size.y / 2f * transform.lossyScale.z);
      Vector3 origin = player.transform.position - offset;
      transform.position = new Vector3(origin.x, environmentManager.GetWaterLevel(), origin.z);
    }
  }

  void GenerateMesh()
  {
    float e = 0.001f;
    mesh = new Mesh();

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    // Facing up triangles
    for (int x = 0; x < size.x; x++)
    {
      for (int z = 0; z < size.y; z++)
      {
        vertices.Add(new Vector3(x, 0, z));
      }
    }
    for (int x = 0; x < size.x - 1; x++)
    {
      for (int z = 0; z < size.y - 1; z++)
      {
        int index = x * size.y + z;
        int indexRight = index + 1;
        int indexUp = index + size.y;
        int indexUpRight = indexUp + 1;

        triangles.Add(index);
        triangles.Add(indexRight);
        triangles.Add(indexUp);

        triangles.Add(indexRight);
        triangles.Add(indexUpRight);
        triangles.Add(indexUp);
      }
    }

    // Facing down triangles
    int offset = vertices.Count;
    for (int x = 0; x < size.x; x++)
    {
      for (int z = 0; z < size.y; z++)
      {
        vertices.Add(new Vector3(x, 0, z));
      }
    }
    for (int x = 0; x < size.x - 1; x++)
    {
      for (int z = 0; z < size.y - 1; z++)
      {
        int index = x * size.y + z;
        int indexRight = index + 1;
        int indexUp = index + size.y;
        int indexUpRight = indexUp + 1;

        triangles.Add(indexUp + offset);
        triangles.Add(indexRight + offset);
        triangles.Add(index + offset);

        triangles.Add(indexUp + offset);
        triangles.Add(indexUpRight + offset);
        triangles.Add(indexRight + offset);
      }
    }

    // Back wall
    vertices.Add(new Vector3(0, e, 0));
    vertices.Add(new Vector3(size.x, e, 0));
    vertices.Add(new Vector3(0, -100, 0));
    vertices.Add(new Vector3(size.x, -100, 0));

    triangles.Add(vertices.Count - 2);
    triangles.Add(vertices.Count - 3);
    triangles.Add(vertices.Count - 4);

    triangles.Add(vertices.Count - 2);
    triangles.Add(vertices.Count - 1);
    triangles.Add(vertices.Count - 3);

    // Front wall
    vertices.Add(new Vector3(0, e, size.y));
    vertices.Add(new Vector3(size.x, e, size.y));
    vertices.Add(new Vector3(0, -100, size.y));
    vertices.Add(new Vector3(size.x, -100, size.y));

    triangles.Add(vertices.Count - 4);
    triangles.Add(vertices.Count - 3);
    triangles.Add(vertices.Count - 2);

    triangles.Add(vertices.Count - 3);
    triangles.Add(vertices.Count - 1);
    triangles.Add(vertices.Count - 2);

    // Left wall
    vertices.Add(new Vector3(0, e, 0));
    vertices.Add(new Vector3(0, e, size.y));
    vertices.Add(new Vector3(0, -100, 0));
    vertices.Add(new Vector3(0, -100, size.y));

    triangles.Add(vertices.Count - 4);
    triangles.Add(vertices.Count - 3);
    triangles.Add(vertices.Count - 2);

    triangles.Add(vertices.Count - 3);
    triangles.Add(vertices.Count - 1);
    triangles.Add(vertices.Count - 2);

    // Right wall
    vertices.Add(new Vector3(size.x, e, 0));
    vertices.Add(new Vector3(size.x, e, size.y));
    vertices.Add(new Vector3(size.x, -100, 0));
    vertices.Add(new Vector3(size.x, -100, size.y));

    triangles.Add(vertices.Count - 2);
    triangles.Add(vertices.Count - 3);
    triangles.Add(vertices.Count - 4);

    triangles.Add(vertices.Count - 2);
    triangles.Add(vertices.Count - 1);
    triangles.Add(vertices.Count - 3);

    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();
    GetComponent<MeshFilter>().mesh = mesh;
  }
}
