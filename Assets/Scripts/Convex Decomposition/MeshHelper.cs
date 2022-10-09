using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

  // ============================= Geometri shenaningans ============================= //
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

  public static Mesh[] Cut(Mesh mesh, Plane cutPlane)
  {
    Mesh[] result = new Mesh[2];
    // TODO: Cut mesh with plane
    return result;
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
