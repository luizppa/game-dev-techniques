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
  HAUSDORFF,
  BILATERAL_HAUSDORFF,
}

public class MonteCarloTreeNode
{
  public List<Mesh> components = new List<Mesh>();
  private MonteCarloTreeNode parent = null;
  private List<MonteCarloTreeNode> children = new List<MonteCarloTreeNode>();
  private Plane cutPlane = null;

  private int depth = 0;
  private int height = 1;
  private float upperConfidenceBound = null;

  readonly float C = 1f; // factor for the upper confidence bound

  public MonteCarloTreeNode(List<Mesh> components)
  {
    this.parent = null;
    this.components = components;
  }

  public MonteCarloTreeNode(ref MonteCarloTreeNode parent, List<Mesh> components, Plane cutPlane)
  {
    this.parent = parent;
    this.depth = parent.depth + 1;
    this.parent.AddChild(ref this);
    this.components = components;
    this.cutPlane = cutPlane;
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
    children.Add(ref child);
    height = Mathf.Max(height, child.GetHeight() + 1);
  }

  public int GetChildCount()
  {
    return children.Count;
  }

  public float UpperConfidenceBound()
  {
    if (upperConfidenceBound == null)
    {
      // TODO: calculate upper confidence bound
    }

    return upperConfidenceBound;
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
  readonly float K = 1f; // factor for the cocavity metric
  readonly int M = 5; // factor for the number of candidate planes

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

  // ============================= Algorithms ============================= //

  Plane MonteCarloTreeSearch(Mesh mesh, int iterations, int depth)
  {
    MonteCarloTreeNode root = new MonteCarloTreeNode(new List<Mesh> { mesh });
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
      Mesh mesh = current.components.OrderBy(c => CalculateConcavity(c)).First();
      if (current.GetChildCount() == (3 * M))
      {
        current = current.children.OrderBy(c => c.UpperConfidenceBound()).First();
        planes.Add(current.cutPlane);
      }
      else
      {
        // TODO
        // Randomly select a untried cutting plane P of mesh
        // Cut mesh into 𝑐𝑙 and 𝑐𝑟 with P
        // Create a new child 𝑣′ to current with P, 𝑐𝑙 and 𝑐𝑟
        // bestChild = 𝑣′
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
      Mesh hull = ConvexHull(m);
      float concavity = CalculateConcavity(m, hull);
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

  float CalculateConcavity(Mesh mesh)
  {
    return CalculateConcavity(mesh, ConvexHull(mesh));
  }

  float CalculateConcavity(Mesh mesh, Mesh convexHull)
  {
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

  float Rv(Mesh mesh, Mesh convexHull)
  {
    return Mathf.Pow(((3f * hullVolume) - volume) / (4f * Mathf.PI), 1f / 3f);
  }

  float HausdorffDistance(Mesh meshA, Mesh meshB)
  {
    return Mathf.Max(OneSidedDistance(meshA.vertices, meshB.vertices), OneSidedDistance(meshB.vertices, meshA.vertices));
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
