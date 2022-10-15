using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
  [SerializeField] GameObject player = null;
  [SerializeField] Vector3 offsetPosition = Vector3.zero;
  [SerializeField] Vector3 offsetRotation = Vector3.zero;

  void Update()
  {
    UpdatePosition();
    UpdateRotation();
  }

  void OnEnable()
  {
    UpdatePosition();
    UpdateRotation();
  }

  private void UpdatePosition()
  {
    transform.position = GetTargetPosition();
  }

  private void UpdateRotation()
  {
    transform.rotation = player.transform.rotation * Quaternion.Euler(offsetRotation);
  }

  public Vector3 GetTargetPosition()
  {
    return player.transform.position + player.transform.TransformVector(offsetPosition);
  }
}
