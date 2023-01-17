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

public class ConvexMeshBuilder : MonoBehaviour
{
  [SerializeField] MeshFilter meshFilter = null;
  // [SerializeField] DecompositionMethod method = DecompositionMethod.TREE_SEARCH;
  [SerializeField] ConcavityMetric metric = ConcavityMetric.HAUSDORFF;
  [SerializeField] float threshold = 0.5f;
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
    // ConvexDecomposition(method);
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

  List<Mesh> ConvexDecomposition(DecompositionMethod method)
  {
    List<Mesh> meshes = new List<Mesh>();
    Queue<Mesh> queue = new Queue<Mesh>();
    queue.Enqueue(mesh);

    while (queue.Count > 0)
    {
      Mesh m = queue.Dequeue();
      float concavity = MeshHelper.CalculateConcavity(m, metric);
      if (concavity < threshold)
      {
        meshes.Add(m);
      }
      else
      {
        // TODO: implement other decomposition methods
        Plane cutPlane = MonteCarloTreeSearch(m);
        Mesh[] cutMeshes = MeshHelper.Cut(m, cutPlane);
        queue.Enqueue(cutMeshes[0]);
        queue.Enqueue(cutMeshes[1]);
      }
    }
    return meshes;
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
    // bestPlane = plane for 𝑣*

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
        Plane candidatePlane = current.GetUntriedPlane();
        Mesh[] meshes = MeshHelper.Cut(mesh, candidatePlane);
        Mesh cl = meshes[0];
        Mesh cr = meshes[1];
        List<Mesh> components = current.components.GetRange(0, current.components.Count);
        components.Remove(mesh);
        components.Add(cl);
        components.Add(cr);
        MonteCarloTreeNode child = new MonteCarloTreeNode(M, components, ref current, candidatePlane);
        current.AddChild(ref child);
        current = child;
        planes.Add(candidatePlane);
        return planes;
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
