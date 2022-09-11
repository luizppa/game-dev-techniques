using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
  private float value = 0f;

  public void SetValue(float value)
  {
    this.value = value;
  }

  public float GetValue()
  {
    return value;
  }

  void OnDrawGizmosSelected()
  {
    Gizmos.color = new Color(value, value, value, 1f);
    Gizmos.DrawSphere(transform.position, 0.03f);
  }
}
