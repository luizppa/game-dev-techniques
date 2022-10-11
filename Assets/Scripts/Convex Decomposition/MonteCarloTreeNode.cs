using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

  public MonteCarloTreeNode(int planesPerDimension, List<Mesh> components, ref MonteCarloTreeNode parent, Plane cutPlane)
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
    planeIndices[index] = true;
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
