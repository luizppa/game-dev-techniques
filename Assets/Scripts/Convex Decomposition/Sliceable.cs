using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Sliceable : MonoBehaviour
{
  [SerializeField] GameObject slicePrefab = null;

  MeshFilter meshFilter;
  MeshRenderer meshRenderer;
  Mesh mesh;
  Material material;


  void Start()
  {
    meshFilter = GetComponent<MeshFilter>();
    meshRenderer = GetComponent<MeshRenderer>();
    mesh = meshFilter.mesh;
    material = meshRenderer.material;
    if (mesh.vertices.Length == 0)
    {
      mesh = new Mesh();
      mesh.vertices = new Vector3[] { new Vector3(-0.5f, 0, 0), new Vector3(-0.5f, 1, 0), new Vector3(0.5f, 0.5f, 0) };
      mesh.triangles = new int[] { 0, 1, 2 };
      mesh.RecalculateNormals();
      meshFilter.mesh = mesh;
    }
    if (gameObject.name == "Convex Mesh")
    {
      Slice(new Plane(Vector3.right.normalized, Vector3.zero));
    }
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void Slice(Plane cutPlane)
  {
    Mesh[] slices = MeshHelper.Cut(mesh, cutPlane);
    GameObject slice1 = Instantiate(slicePrefab, transform.position, transform.rotation);
    GameObject slice2 = Instantiate(slicePrefab, transform.position, transform.rotation);

    slice1.name = gameObject.name + " Slice 1";
    slice2.name = gameObject.name + " Slice 2";

    MeshFilter slice1MeshFilter = slice1.GetComponent<MeshFilter>();
    MeshFilter slice2MeshFilter = slice2.GetComponent<MeshFilter>();

    MeshRenderer slice1MeshRenderer = slice1.GetComponent<MeshRenderer>();
    MeshRenderer slice2MeshRenderer = slice2.GetComponent<MeshRenderer>();

    MeshCollider slice1MeshCollider = slice1.GetComponent<MeshCollider>();
    MeshCollider slice2MeshCollider = slice2.GetComponent<MeshCollider>();

    if (slice1MeshFilter != null)
    {
      slice1MeshFilter.mesh = slices[0];
    }
    if (slice2MeshFilter != null)
    {
      slice2MeshFilter.mesh = slices[1];
    }

    if (slice1MeshRenderer != null)
    {
      slice1MeshRenderer.material = material;
      slice1MeshFilter.mesh.RecalculateNormals();
      slice1MeshFilter.mesh.RecalculateBounds();
      slice1MeshFilter.mesh.RecalculateUVDistributionMetrics();
    }
    if (slice2MeshRenderer != null)
    {
      slice2MeshRenderer.material = material;
      slice2MeshFilter.mesh.RecalculateNormals();
      slice2MeshFilter.mesh.RecalculateBounds();
      slice2MeshFilter.mesh.RecalculateUVDistributionMetrics();
    }

    if (slice1MeshCollider != null)
    {
      slice1MeshCollider.sharedMesh = slices[0];
    }
    if (slice2MeshCollider != null)
    {
      slice2MeshCollider.sharedMesh = slices[1];
    }
    Destroy(gameObject);
  }
}
