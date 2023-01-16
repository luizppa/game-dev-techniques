using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class QuadPlane : MonoBehaviour
{
  [SerializeField] Vector2Int size = Vector2Int.one * 10;
  [SerializeField] float cellSize = 1f;

  private Mesh mesh;
  private List<Vector3> vertices = new List<Vector3>();
  private List<int> triangles = new List<int>();

  private MeshCollider meshCollider = null;
  private MeshFilter meshFilter = null;

  void Start()
  {
    meshFilter = GetComponent<MeshFilter>();
    meshCollider = GetComponent<MeshCollider>();
    mesh = new Mesh();
    Generate();
  }

  void Generate()
  {
    for (int x = 0; x < size.x; x++)
    {
      for (int y = 0; y < size.y; y++)
      {
        // Create vertices
        vertices.Add(new Vector3(x * cellSize, 0, y * cellSize));
        vertices.Add(new Vector3(x * cellSize, 0, (y + 1) * cellSize));
        vertices.Add(new Vector3((x + 1) * cellSize, 0, (y + 1) * cellSize));
        vertices.Add(new Vector3((x + 1) * cellSize, 0, y * cellSize));

        // Create triangles
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
      }
    }

    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();

    meshFilter.mesh = mesh;
    if (meshCollider != null)
    {
      meshCollider.sharedMesh = mesh;
    }
  }
}
