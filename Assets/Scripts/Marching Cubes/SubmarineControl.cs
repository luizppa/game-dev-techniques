using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineControl : MonoBehaviour
{
  [SerializeField] string horizontalAxis = "Horizontal";
  [SerializeField] string verticalAxis = "Vertical";
  [SerializeField] ParticleSystem waterParticles = null;

  [SerializeField] Camera thirdPersonCamera = null;
  [SerializeField] Camera firstPersonCamera = null;
  private bool firstPerson = false;

  void Start()
  {
    firstPersonCamera.enabled = false;
  }

  void FixedUpdate()
  {
    Move();
  }

  void Update()
  {
    ToggleView();
    Rotate();
  }

  void Move()
  {
    Vector3 direction = GetMoveDirection();
    direction = ProjectionOnGroundPlane(direction) + (Vector3.up * Input.GetAxis("Ascend"));
    if (direction.magnitude > 0f)
    {
      if (waterParticles.isPlaying == false)
      {
        waterParticles.Play();
      }
      transform.Translate(direction.normalized * Time.deltaTime * 5f, Space.World);
    }
    else if (waterParticles.isPlaying == true && direction.magnitude == 0f)
    {
      waterParticles.Stop();
    }
  }

  void Rotate()
  {
    Vector3 direction = GetLookDirection();
    direction = ProjectionOnGroundPlane(direction);
    if (direction.magnitude > 0.1f)
    {
      transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
    }
  }

  void ToggleView()
  {
    if (Input.GetKeyDown(KeyCode.V))
    {
      firstPerson = !firstPerson;
      if (firstPerson)
      {
        firstPersonCamera.enabled = true;
        thirdPersonCamera.enabled = false;
      }
      else
      {
        thirdPersonCamera.enabled = true;
        firstPersonCamera.enabled = false;
      }
    }
  }

  private Vector3 GetMoveDirection()
  {
    Vector3 direction = (thirdPersonCamera.transform.right * Input.GetAxis(horizontalAxis)) + (thirdPersonCamera.transform.forward * Input.GetAxis(verticalAxis));
    if (direction.magnitude <= 0.1f)
    {
      return Vector3.zero;
    }
    return direction;
  }

  private Vector3 GetLookDirection()
  {
    Vector3 right = thirdPersonCamera.transform.right;
    Vector3 direction = (right * Input.GetAxis(horizontalAxis)) + (thirdPersonCamera.transform.forward * Input.GetAxis(verticalAxis));
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
