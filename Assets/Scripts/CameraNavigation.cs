using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNavigation : MonoBehaviour
{
  private float angleX = 0f;
  private float angleY = 0f;

  void Start()
  {
    angleX = transform.eulerAngles.y;
    angleY = transform.eulerAngles.x;
  }

  void Update()
  {
    if (Time.timeScale > 0)
    {
      Cursor.lockState = CursorLockMode.Locked;
      Rotate();
      Move();
    }
  }

  private void Rotate()
  {
    float x = Input.GetAxis("Look X");
    float y = -Input.GetAxis("Look Y");

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
