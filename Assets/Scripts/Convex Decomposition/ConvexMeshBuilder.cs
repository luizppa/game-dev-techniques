using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DecompositionMethod
{
  HULL,
  HACD,
  TREE_SEARCH,
}

public enum ConcavityMetric
{
  VOLUME,
  AREA,
  BOUNDING_BOX,
  HAUSDORFF
}

public class ConvexMeshBuilder : MonoBehaviour
{
  [SerializeField] MeshFilter meshFilter = null;
  [SerializeField] DecompositionMethod method = DecompositionMethod.TREE_SEARCH;
  [SerializeField] ConcavityMetric metric = ConcavityMetric.HAUSDORFF;
  [SerializeField] float treshold = 0.5f;
  private Mesh mesh = null;

  private Mesh convexHull = null;
  private float volume = 0f;
  private float hullVolume = 0f;
  private float concavity = 0f;

  void Start()
  {
    if (meshFilter != null)
    {
      mesh = meshFilter.mesh;
    }

    CalculateProperties();
    ConvexDecomposition(method);
  }

  void Update()
  {

  }

  void CalculateProperties()
  {
    convexHull = ConvexHull(mesh);
    volume = Volume(mesh) * meshFilter.transform.lossyScale.x;
    hullVolume = Volume(convexHull) * meshFilter.transform.lossyScale.x;
    concavity = CalculateConcavity(mesh, convexHull);
  }

  // ============================= Mesh shenaningans ============================= //

  List<Mesh> ConvexDecomposition(DecompositionMethod method)
  {
    List<Mesh> meshes = new List<Mesh>();
    Queue<Mesh> queue = new Queue<Mesh>();
    queue.Enqueue(mesh);

    while (queue.Count > 0)
    {
      Mesh m = queue.Dequeue();
      Mesh hull = ConvexHull(m);
      float concavity = CalculateConcavity(m, hull);
      if (concavity < treshold)
      {
        meshes.Add(m);
      }
      else
      {
        // TODO: implement monte carlo tree search and other decomposition methods
        // Mesh[] splitMeshes = SplitMesh(m, hull);
        // queue.Enqueue(splitMeshes[0]);
        // queue.Enqueue(splitMeshes[1]);
      }
    }
    return meshes;
  }

  Mesh ConvexHull(Mesh mesh)
  {
    List<Vector3> vertices = new List<Vector3>(mesh.vertices);
    List<Vector3> hullVertices = new List<Vector3>();
    List<int> hullTriangles = new List<int>();
    List<Vector3> hullNormals = new List<Vector3>();

    GK.ConvexHullCalculator convexHullCalculator = new GK.ConvexHullCalculator();
    convexHullCalculator.GenerateHull(vertices, false, ref hullVertices, ref hullTriangles, ref hullNormals);
    Mesh convexHull = new Mesh();
    convexHull.vertices = hullVertices.ToArray();
    convexHull.triangles = hullTriangles.ToArray();
    convexHull.normals = hullNormals.ToArray();
    convexHull.RecalculateNormals();
    return convexHull;
  }

  float Volume(Mesh mesh)
  {
    float volume = 0;
    Vector3[] vertices = mesh.vertices;
    int[] triangles = mesh.triangles;
    for (int i = 0; i < mesh.triangles.Length; i += 3)
    {
      Vector3 p1 = vertices[triangles[i + 0]];
      Vector3 p2 = vertices[triangles[i + 1]];
      Vector3 p3 = vertices[triangles[i + 2]];
      volume += SignedVolumeOfTriangle(p1, p2, p3);
    }
    return Mathf.Abs(volume);
  }

  float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
  {
    float v321 = p3.x * p2.y * p1.z;
    float v231 = p2.x * p3.y * p1.z;
    float v312 = p3.x * p1.y * p2.z;
    float v132 = p1.x * p3.y * p2.z;
    float v213 = p2.x * p1.y * p3.z;
    float v123 = p1.x * p2.y * p3.z;
    return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
  }

  // ============================= Math shenanigans ============================= //

  float CalculateConcavity(Mesh mesh, Mesh convexHull)
  {
    return Mathf.Max(HausdorffDistance(mesh, convexHull), Rv(mesh, convexHull));
  }

  float Rv(Mesh mesh, Mesh convexHull)
  {
    return Mathf.Pow(((3f * hullVolume) - volume) / (4f * Mathf.PI), 1f / 3f);
  }

  float OneSidedDistance(Vector3[] surfaceA, Vector3[] surfaceB)
  {
    float max = 0f;
    foreach (Vector3 point in surfaceA)
    {
      float distance = DistanceFromSurface(point, surfaceB);
      if (distance > max)
      {
        max = distance;
      }
    }
    return max;
  }

  float DistanceFromSurface(Vector3 point, Vector3[] surfacePoints)
  {
    float min = Mathf.Infinity;
    foreach (Vector3 sPoint in surfacePoints)
    {
      float distance = Vector3.Distance(point, sPoint);
      if (distance < min)
      {
        min = distance;
      }
    }
    return min;
  }

  float HausdorffDistance(Mesh meshA, Mesh meshB)
  {
    return Mathf.Max(OneSidedDistance(meshA.vertices, meshB.vertices), OneSidedDistance(meshB.vertices, meshA.vertices));
  }

  // ============================= Gizmos ============================= //

  void OnDrawGizmos()
  {
    if (convexHull != null)
    {
      Gizmos.color = Color.green;
      Gizmos.DrawWireMesh(convexHull, meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
    }
  }

  void OnDrawGizmosSelected()
  {
    if (mesh != null)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawWireMesh(mesh, meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
    }
  }

  // ============================= Getters ============================= //

  public Mesh GetConvexHull()
  {
    return convexHull;
  }

  public float GetVolume()
  {
    return volume;
  }

  public float GetHullVolume()
  {
    return hullVolume;
  }

  public float GetConcavity()
  {
    return concavity;
  }
}
