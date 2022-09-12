using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
  [SerializeField] Camera gameCamera = null;
  [SerializeField] float moveSpeed = 1f;
  [SerializeField] float runSpeed = 1.5f;

  private Rigidbody rb = null;

  void Start()
  {
    rb = GetComponent<Rigidbody>();
  }

  void FixedUpdate()
  {
    ControlPlayer();
  }

  void ControlPlayer()
  {
    Rotate();
    Move();
  }

  void Rotate()
  {
    Vector3 direction = GetMoveDirection();
    if (direction.magnitude > 0f)
    {
      transform.rotation = Quaternion.LookRotation(direction);
    }
  }

  void Move()
  {
    Vector3 direction = GetMoveDirection();
    if (direction.magnitude > 0f)
    {
      Vector3 velocity = direction * GetMoveSpeed() * Time.deltaTime * 60f;
      rb.AddForce(velocity, ForceMode.VelocityChange);
    }
  }

  private Vector3 GetMoveDirection()
  {
    Vector3 direction = (gameCamera.transform.right * Input.GetAxis("Horizontal")) + (gameCamera.transform.forward * Input.GetAxis("Vertical"));
    if (direction.magnitude <= 0.1f)
    {
      return Vector3.zero;
    }
    direction = Vector3.ProjectOnPlane(direction, transform.up).normalized;
    return direction;
  }

  private float GetMoveSpeed()
  {
    if (Input.GetKey(KeyCode.LeftShift))
    {
      return runSpeed;
    }
    return moveSpeed;
  }
}
