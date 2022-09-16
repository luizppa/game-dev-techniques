using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeVertex
{
  private float value = 0f;
  private Vector3 position = Vector3.zero;

  public CubeVertex(float value, Vector3 position)
  {
    this.value = value;
    this.position = position;
  }

  public void SetValue(float value)
  {
    this.value = value;
  }

  public float GetValue()
  {
    return value;
  }

  public Vector3 GetPosition()
  {
    return position;
  }
}
