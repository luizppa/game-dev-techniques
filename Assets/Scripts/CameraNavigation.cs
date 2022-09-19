using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNavigation : MonoBehaviour
{
  private float angleX = 0f;
  private float angleY = 0f;

  void Start()
  {
    Cursor.lockState = CursorLockMode.Locked;
  }

  void Update()
  {
    Rotate();
    Move();
  }

  private void Rotate()
  {
    float x = Input.GetAxis("Mouse X");
    float y = -Input.GetAxis("Mouse Y");

    angleX += x;
    angleY += y;

    transform.rotation = Quaternion.Euler(angleY, angleX, 0f);
  }

  private void Move()
  {
    float x = Input.GetAxis("Horizontal");
    float y = Input.GetAxis("Vertical");
    float z = Input.GetAxis("Ascend");

    transform.Translate(Vector3.right * x * 5f * Time.deltaTime);
    transform.Translate(Vector3.forward * y * 5f * Time.deltaTime);
    transform.Translate(Vector3.up * z * 5f * Time.deltaTime);
  }
}
