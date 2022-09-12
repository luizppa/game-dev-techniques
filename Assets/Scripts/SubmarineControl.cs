using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineControl : MonoBehaviour
{
  [SerializeField] string horizontalAxis = "Horizontal";
  [SerializeField] string verticalAxis = "Vertical";
  private Camera gameCamera = null;

  // Start is called before the first frame update
  void Start()
  {
    gameCamera = Camera.main;
  }

  // Update is called once per frame
  void Update()
  {
    Move();
    Rotate();
  }

  void Move()
  {
    Vector3 direction = GetMoveDirection();
    direction = ProjectionOnGroundPlane(direction) + (Vector3.up * Input.GetAxis("Ascend"));
    if (direction.magnitude > 0f)
    {
      transform.Translate(direction.normalized * Time.deltaTime * 5f, Space.World);
    }
  }

  void Rotate()
  {
    Vector3 direction = GetMoveDirection();
    direction = ProjectionOnGroundPlane(direction);
    if (direction.magnitude > 0.1f)
    {
      transform.rotation = Quaternion.LookRotation(direction);
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
