using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Sliceable : MonoBehaviour
{
  [SerializeField] GameObject slicePrefab = null;

  MeshFilter meshFilter;
  Mesh mesh;


  void Start()
  {
    meshFilter = GetComponent<MeshFilter>();
    mesh = meshFilter.mesh;
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
      Slice(new Plane(Vector3.right, Vector3.zero));
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
    slice1.GetComponent<MeshFilter>().mesh = slices[0];
    slice2.GetComponent<MeshFilter>().mesh = slices[1];
    Destroy(gameObject);
  }
}
