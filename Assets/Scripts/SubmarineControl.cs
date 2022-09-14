using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineControl : MonoBehaviour
{
  [SerializeField] string horizontalAxis = "Horizontal";
  [SerializeField] string verticalAxis = "Vertical";
  private Camera gameCamera = null;
  private Rigidbody rb = null;

  void Start()
  {
    gameCamera = Camera.main;
    rb = GetComponent<Rigidbody>();
  }

  void FixedUpdate()
  {
    Move();
  }

  void Update()
  {
    Rotate();
  }

  void Move()
  {
    Vector3 direction = GetMoveDirection();
    direction = ProjectionOnGroundPlane(direction) + (Vector3.up * Input.GetAxis("Ascend"));
    if (direction.magnitude > 0f)
    {
      rb.MovePosition(transform.position + direction.normalized * Time.deltaTime * 5f);
    }
  }

  void Rotate()
  {
    Vector3 direction = GetMoveDirection();
    direction = ProjectionOnGroundPlane(direction);
    if (direction.magnitude > 0.1f)
    {
      transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90, 90, 90);
    }
  }

  private Vector3 GetMoveDirection()
  {
    Vector3 direction = (gameCamera.transform.right * Input.GetAxis(horizontalAxis)) + (gameCamera.transform.forward * Input.GetAxis(verticalAxis));
    if (direction.magnitude <= 0.1f)
    {
      return Vector3.zero;
    }
    return direction;
  }

  private Vector3 ProjectionOnGroundPlane(Vector3 v)
  {
    return Vector3.ProjectOnPlane(v, Vector3.up);
  }
}
