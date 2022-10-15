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
    transform.position = player.transform.position + player.transform.TransformVector(offsetPosition);
    transform.rotation = player.transform.rotation * Quaternion.Euler(offsetRotation);
  }
}
