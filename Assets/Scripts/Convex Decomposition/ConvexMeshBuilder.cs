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
  HAUSDORFF,
  BILATERAL_HAUSDORFF,
}

public static class MeshHelper
{
  static readonly float K = .3f; // factor for the cocavity metric

  public static Mesh ConvexHull(Mesh mesh)
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

  public static float Volume(Mesh mesh)
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

  // ============================= Math shenanigans ============================= //

  public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
  {
    float v321 = p3.x * p2.y * p1.z;
    float v231 = p2.x * p3.y * p1.z;
    float v312 = p3.x * p1.y * p2.z;
    float v132 = p1.x * p3.y * p2.z;
    float v213 = p2.x * p1.y * p3.z;
    float v123 = p1.x * p2.y * p3.z;
    return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
  }

  public static float CalculateConcavity(Mesh mesh, ConcavityMetric metric)
  {
    Mesh convexHull = ConvexHull(mesh);
    switch (metric)
    {
      case ConcavityMetric.VOLUME:
        return Volume(mesh) / Volume(convexHull);
      // case ConcavityMetric.AREA:
      //   return Area(mesh) / Area(convexHull);
      // case ConcavityMetric.BOUNDING_BOX:
      //   return BoundingBox(mesh).sqrMagnitude / BoundingBox(convexHull).sqrMagnitude;
      case ConcavityMetric.HAUSDORFF:
        return HausdorffDistance(mesh, convexHull);
      case ConcavityMetric.BILATERAL_HAUSDORFF:
        return Mathf.Max(HausdorffDistance(mesh, convexHull), K * Rv(mesh, convexHull));
      default:
        return HausdorffDistance(mesh, convexHull);
    }
  }

  public static float Rv(Mesh mesh, Mesh convexHull)
  {
    float volume = Volume(mesh);
    float hullVolume = Volume(convexHull);
    return Mathf.Pow(((3f * hullVolume) - volume) / (4f * Mathf.PI), 1f / 3f);
  }

  public static float HausdorffDistance(Mesh meshA, Mesh meshB)
  {
    return Mathf.Max(OneSidedDistance(meshA.vertices, meshB.vertices), OneSidedDistance(meshB.vertices, meshA.vertices));
  }

  public static float OneSidedDistance(Vector3[] surfaceA, Vector3[] surfaceB)
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

  public static float DistanceFromSurface(Vector3 point, Vector3[] surfacePoints)
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

}

public class MonteCarloTreeNode
{
  public List<Mesh> components = new List<Mesh>();
  private MonteCarloTreeNode parent = null;
  private List<MonteCarloTreeNode> children = new List<MonteCarloTreeNode>();
  private Plane cutPlane;

  private int planesPerDimension = 10;
  private bool[] planeIndices = null;
  private int depth = 0;
  private int height = 1;
  private int triedPlanes = 0;
  private float upperConfidenceBound = -1f;
  private float value = -1f;

  private readonly float C = .5f; // exploration constant

  public MonteCarloTreeNode(int planesPerDimension, List<Mesh> components)
  {
    this.planesPerDimension = planesPerDimension;
    this.components = components;
    CreatePlanes();
  }

  public MonteCarloTreeNode(int planesPerDimension, ref MonteCarloTreeNode parent, List<Mesh> components, Plane cutPlane)
  {
    this.planesPerDimension = planesPerDimension;
    this.parent = parent;
    this.depth = parent.depth + 1;
    MonteCarloTreeNode self = this;
    this.parent.AddChild(ref self);
    this.components = components;
    this.cutPlane = cutPlane;
    CreatePlanes();
  }

  private void CreatePlanes()
  {
    this.planeIndices = new bool[planesPerDimension * 3];
  }

  public int GetDepth()
  {
    return depth;
  }

  public int GetHeight()
  {
    return height;
  }

  public void AddChild(ref MonteCarloTreeNode child)
  {
    children.Add(child);
    height = Mathf.Max(height, child.GetHeight() + 1);
  }

  public List<MonteCarloTreeNode> GetChildren()
  {
    return children;
  }

  public int GetChildCount()
  {
    return children.Count;
  }

  public Plane GetCutPlane()
  {
    return cutPlane;
  }

  public float UpperConfidenceBound()
  {
    if (upperConfidenceBound < 0f)
    {
      upperConfidenceBound = this.Value();
      if (parent != null)
      {
        upperConfidenceBound += C * Mathf.Sqrt((2f * Mathf.Log(parent.triedPlanes)) / triedPlanes);
      }
    }

    return upperConfidenceBound;
  }

  public float Value()
  {
    if (value < 0f)
    {
      foreach (Mesh mesh in components)
      {
        float concavity = MeshHelper.CalculateConcavity(mesh, ConcavityMetric.HAUSDORFF);
        float candidateValue = 1f / (1f + concavity);
        if (candidateValue > value)
        {
          value = candidateValue;
        }
      }
    }
    return value;
  }

  private int[] GetPlaneValue(int planeIndex)
  {
    int dimension = Mathf.FloorToInt(planeIndex / planesPerDimension);
    int value = planeIndex % planesPerDimension;

    return new int[] { dimension, value };
  }

  public Plane GetUntriedPlane()
  {
    if (triedPlanes == planeIndices.Length)
    {
      throw new System.Exception("No untreid planes left"); ;
    }
    int index = Random.Range(0, planeIndices.Length);
    while (planeIndices[index])
    {
      index = (index + 1) % planeIndices.Length;
    }
    int[] planeValue = GetPlaneValue(index);
    int dimension = planeValue[0];
    int value = planeValue[1];
    switch (dimension)
    {
      case 0: return new Plane(Vector3.up, value);
      case 1: return new Plane(Vector3.right, value);
      case 2: return new Plane(Vector3.forward, value);
      default: throw new System.Exception("Invalid dimension");
    }
  }
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

  // Constants 
  readonly int M = 20; // factor for the number of candidate planes

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
    convexHull = MeshHelper.ConvexHull(mesh);
    volume = MeshHelper.Volume(mesh) * meshFilter.transform.lossyScale.x;
    hullVolume = MeshHelper.Volume(convexHull) * meshFilter.transform.lossyScale.x;
    concavity = MeshHelper.CalculateConcavity(mesh, metric);
  }

  // ============================= Algorithms ============================= //

  Plane MonteCarloTreeSearch(Mesh mesh, int iterations = 500, int depth = 4)
  {
    MonteCarloTreeNode root = new MonteCarloTreeNode(M, new List<Mesh> { mesh });
    MonteCarloTreeNode current = root;
    List<Plane> planes = new List<Plane>();
    Plane bestPlane = new Plane();

    for (int i = 0; i < iterations; i++)
    {
      MonteCarloTreeNode child = null;
      planes = TreePolicy(current, ref child, depth);
      // TODO
      // Calculate the planes for the default policy
      // Measure the quality of the planes
      // Backpropagate the quality of the planes
    }

    // TODO
    // Select the best child 𝑣* of the root node
    // bestPlaane = plane for 𝑣*

    return bestPlane;
  }

  List<Plane> TreePolicy(MonteCarloTreeNode node, ref MonteCarloTreeNode bestChild, int depth)
  {
    List<Plane> planes = new List<Plane>();
    MonteCarloTreeNode current = node;
    while (current.GetDepth() < depth)
    {
      Mesh mesh = current.components.OrderBy(c => MeshHelper.CalculateConcavity(c, metric)).First();
      if (current.GetChildCount() == (3 * M))
      {
        current = current.GetChildren().OrderBy(c => c.UpperConfidenceBound()).First();
        planes.Add(current.GetCutPlane());
      }
      else
      {
        // TODO
        Plane candidatePlane = current.GetUntriedPlane();
        // Cut mesh into 𝑐𝑙 and 𝑐𝑟 with P
        // Create a new child 𝑣′ to current with P, 𝑐𝑙 and 𝑐𝑟
        // bestChild = 𝑣′
        planes.Add(candidatePlane);
        // return planes + { P }
      }
    }
    bestChild = current;
    return planes;
  }

  void DefaultPolicy()
  {
    // TODO
  }

  void Backup(Mesh mesh, Plane[] planes)
  {
    // TODO
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
      float concavity = MeshHelper.CalculateConcavity(m, metric);
      if (concavity < treshold)
      {
        meshes.Add(m);
      }
      else
      {
        // TODO: implement monte carlo tree search and other decomposition methods
        // List<Mesh> splitMeshes = MonteCarloTreeSearch(m, hull);
        // queue.Enqueue(splitMeshes[0]);
        // queue.Enqueue(splitMeshes[1]);
      }
    }
    return meshes;
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
