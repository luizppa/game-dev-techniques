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
  [SerializeField] Vector3 offsetRotation = new Vector3(90f, 0f, 0f);
  private bool firstPerson = false;
  private Vector2 firstPersonRotation = Vector2.zero;

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
    Vector3 direction;
    direction = GetMoveDirection();
    if (firstPerson == false)
    {
      direction = ProjectionOnGroundPlane(direction);
    }
    direction += (Vector3.up * Input.GetAxis("Ascend"));

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
    float sensitivityMultiplier = firstPerson ? 5f : 1f;
    firstPersonRotation.x += Input.GetAxis("Look X") * sensitivityMultiplier * Time.deltaTime * 500f;
    firstPersonRotation.y -= Input.GetAxis("Look Y") * sensitivityMultiplier * Time.deltaTime * 300f;
    if (firstPerson)
    {
      transform.rotation = Quaternion.Euler(firstPersonRotation.y, firstPersonRotation.x, 0f) * Quaternion.Euler(offsetRotation.x, offsetRotation.y, offsetRotation.z);
    }
    else
    {
      Vector3 direction = GetLookDirection();
      if (direction.magnitude > 0.1f)
      {
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(offsetRotation.x, offsetRotation.y, offsetRotation.z);
      }
    }
  }

  void ToggleView()
  {
    if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.JoystickButton4))
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
        float x = Vector3.Angle(Vector3.forward, thirdPersonCamera.transform.forward);
        float y = Vector3.Angle(Vector3.right, thirdPersonCamera.transform.right);
        firstPersonRotation = new Vector2(x, y);
      }
    }
  }

  private Vector3 GetMoveDirection()
  {
    Camera referenceCamera = firstPerson ? firstPersonCamera : thirdPersonCamera;
    Vector3 right = referenceCamera.transform.right;
    Vector3 forward = referenceCamera.transform.forward;
    Vector3 direction = (right * Input.GetAxis(horizontalAxis)) + (forward * Input.GetAxis(verticalAxis));

    if (direction.magnitude <= 0.1f)
    {
      return Vector3.zero;
    }
    return direction;
  }

  private Vector3 GetLookDirection()
  {
    Vector3 right = thirdPersonCamera.transform.right;
    Vector3 forward = thirdPersonCamera.transform.forward;

    float horizontalAxisValue = Input.GetAxis(horizontalAxis);
    float verticalAxisValue = Input.GetAxis(verticalAxis);

    Vector3 direction = (right * horizontalAxisValue) + (forward * verticalAxisValue);

    if (firstPerson == false)
    {
      direction = ProjectionOnGroundPlane(direction);
      direction += (Vector3.up * Input.GetAxis("Ascend"));
    }

    if (direction.magnitude <= 0.1f)
    {
      return Vector3.zero;
    }
    return direction;
  }

  private Vector3 ProjectionOnGroundPlane(Vector3 v)
  {
    Vector3 normal = Vector3.up;
    return Vector3.ProjectOnPlane(v, normal);
  }

}
