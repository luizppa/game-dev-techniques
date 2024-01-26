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
    float e = 0.1f;
    mesh = new Mesh();

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    // Facing up triangles
    for (int x = 0; x < size.x; x++)
    {
      float xAdder = 0;
      if(x == 0){
        xAdder = -e;
      }
      else if(x == size.x - 1){
        xAdder = e;
      }
      for (int z = 0; z < size.y; z++)
      {
        float zAdder = 0;
        if(z == 0){
          zAdder = -e;
        }
        else if(z == size.y - 1){
          zAdder = e;
        }
        vertices.Add(new Vector3(x + xAdder, 0, z + zAdder));
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
      float xOffset = 0;
      if(x == 0){
        xOffset = -e;
      }
      else if(x == size.x - 1){
        xOffset = e;
      }
      for (int z = 0; z < size.y; z++)
      {
        float zOffset = 0;
        if(z == 0){
          zOffset = -e;
        }
        else if(z == size.y - 1){
          zOffset = e;
        }
        vertices.Add(new Vector3(x + xOffset, 0, z + zOffset));
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
