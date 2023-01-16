using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class ProceduralSegment : MonoBehaviour
{
  [SerializeField] public Vector3 controlPoint = Vector3.right;

  void Start()
  {

  }
  void Update()
  {

  }
  void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(transform.TransformPoint(controlPoint), 0.05f);
  }
}
