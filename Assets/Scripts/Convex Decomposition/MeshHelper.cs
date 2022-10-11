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

  // TODO: implement with compute shaders
  public static Mesh[] Cut(Mesh mesh, Plane cutPlane)
  {
    Mesh upperMesh = new Mesh();
    Mesh lowerMesh = new Mesh();
    List<int> upperTriangles = new List<int>();
    List<int> lowerTriangles = new List<int>();
    List<Vector3> upperVertices = new List<Vector3>();
    List<Vector3> lowerVertices = new List<Vector3>();

    Vector3[] vertices = mesh.vertices;
    int[] triangles = mesh.triangles;

    for (int i = 0; (i + 2) < triangles.Length; i += 3)
    {

      List<int> upSide = new List<int>();
      List<int> downSide = new List<int>();

      for (int j = 0; j < 3; j++)
      {
        if (cutPlane.GetSide(vertices[triangles[i + j]]))
        {
          upSide.Add(j);
        }
        else
        {
          downSide.Add(j);
        }
      }

      if (upSide.Count == 3)
      {
        for (int j = 0; j < 3; j++)
        {
          upperTriangles.Add(upperVertices.Count);
          upperVertices.Add(vertices[triangles[i + j]]);
        }
      }
      else if (upSide.Count == 0)
      {
        for (int j = 0; j < 3; j++)
        {
          lowerTriangles.Add(lowerVertices.Count);
          lowerVertices.Add(vertices[triangles[i + j]]);
        }
      }
      else
      {
        CutTriangle(mesh, i, cutPlane, upSide, downSide, ref upperTriangles, ref lowerTriangles, ref upperVertices, ref lowerVertices);
      }
    }

    upperMesh.vertices = upperVertices.ToArray();
    upperMesh.triangles = upperTriangles.ToArray();
    upperMesh.RecalculateNormals();

    lowerMesh.vertices = lowerVertices.ToArray();
    lowerMesh.triangles = lowerTriangles.ToArray();
    lowerMesh.RecalculateNormals();
    Mesh[] result = new Mesh[] { upperMesh, lowerMesh };
    return result;
  }

  public static void CutTriangle(Mesh mesh, int triangleIndex, Plane cutPlane, List<int> upSide, List<int> downSide, ref List<int> upperTriangles, ref List<int> lowerTriangles, ref List<Vector3> upperVertices, ref List<Vector3> lowerVertices)
  {
    int a, b, c;
    if (upSide.Count == 1)
    {
      a = upSide[0];
    }
    else
    {
      a = downSide[0];
    }

    b = GetLeftTriangleVertex(a);
    c = GetRightTriangleVertex(a);

    Vector3 aVertex = mesh.vertices[mesh.triangles[triangleIndex + a]];
    Vector3 bVertex = mesh.vertices[mesh.triangles[triangleIndex + b]];
    Vector3 cVertex = mesh.vertices[mesh.triangles[triangleIndex + c]];

    Ray rayLeft = new Ray(aVertex, (bVertex - aVertex).normalized);
    Ray rayRight = new Ray(aVertex, (cVertex - aVertex).normalized);

    float distanceLeft;
    float distanceRight;
    cutPlane.Raycast(rayLeft, out distanceLeft);
    cutPlane.Raycast(rayRight, out distanceRight);

    Vector3 v1 = rayRight.GetPoint(distanceRight);
    Vector3 v2 = rayLeft.GetPoint(distanceLeft);

    if (upSide.Count == 1)
    {
      upperTriangles.Add(upperVertices.Count);
      upperVertices.Add(aVertex);
      upperTriangles.Add(upperVertices.Count);
      upperVertices.Add(v2);
      upperTriangles.Add(upperVertices.Count);
      upperVertices.Add(v1);

      lowerTriangles.Add(lowerVertices.Count);
      lowerVertices.Add(cVertex);
      lowerTriangles.Add(lowerVertices.Count);
      lowerVertices.Add(v1);
      lowerTriangles.Add(lowerVertices.Count);
      lowerVertices.Add(v2);

      lowerTriangles.Add(lowerVertices.Count);
      lowerVertices.Add(cVertex);
      lowerTriangles.Add(lowerVertices.Count);
      lowerVertices.Add(v2);
      lowerTriangles.Add(lowerVertices.Count);
      lowerVertices.Add(bVertex);
    }
    else
    {

      lowerTriangles.Add(lowerVertices.Count);
      lowerVertices.Add(aVertex);
      lowerTriangles.Add(lowerVertices.Count);
      lowerVertices.Add(v2);
      lowerTriangles.Add(lowerVertices.Count);
      lowerVertices.Add(v1);

      upperTriangles.Add(upperVertices.Count);
      upperVertices.Add(cVertex);
      upperTriangles.Add(upperVertices.Count);
      upperVertices.Add(v1);
      upperTriangles.Add(upperVertices.Count);
      upperVertices.Add(v2);

      upperTriangles.Add(upperVertices.Count);
      upperVertices.Add(cVertex);
      upperTriangles.Add(upperVertices.Count);
      upperVertices.Add(v2);
      upperTriangles.Add(upperVertices.Count);
      upperVertices.Add(bVertex);
    }
  }

  private static int GetLeftTriangleVertex(int v)
  {
    return (v + 1) % 3;
  }

  private static int GetRightTriangleVertex(int v)
  {
    return (v + 2) % 3;
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
