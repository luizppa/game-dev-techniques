using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineControl : MonoBehaviour
{
  [SerializeField] string horizontalAxis = "Horizontal";
  [SerializeField] string verticalAxis = "Vertical";
  [SerializeField] ParticleSystem waterParticles = null;
  [SerializeField] Rotate proppeler = null;
  [SerializeField] List<Light> lights = new List<Light>();

  [SerializeField] float maxSpeed = 5f;
  [SerializeField] float acceleration = 2f;

  [SerializeField] Camera thirdPersonCamera = null;
  [SerializeField] Camera firstPersonCamera = null;

  private bool firstPerson = false;
  private bool lightState = true;
  private Vector2 firstPersonRotation = Vector2.zero;

  private Rigidbody rb = null;

  void Start()
  {
    rb = GetComponent<Rigidbody>();
    firstPersonCamera.enabled = false;
  }

  void Update()
  {
    Action();
    Move();
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
        proppeler.Play();
      }

      rb.AddForce(direction * acceleration, ForceMode.Acceleration);
      if (rb.velocity.magnitude > maxSpeed)
      {
        rb.velocity = rb.velocity.normalized * maxSpeed;
      }
    }

    if (rb.velocity.magnitude < 0.1f && waterParticles.isPlaying == true)
    {
      waterParticles.Stop();
      proppeler.Stop();
    }
    else if (rb.velocity.magnitude > 0.1f && waterParticles.isPlaying == false)
    {
      waterParticles.Play();
      proppeler.Play();
    }

    proppeler.SetSpeed(rb.velocity.magnitude);
  }

  void Rotate()
  {
    float sensitivityMultiplier = firstPerson ? 5f : 1f;
    firstPersonRotation.x += Input.GetAxis("Look X") * sensitivityMultiplier * Time.deltaTime * 500f;
    firstPersonRotation.y -= Input.GetAxis("Look Y") * sensitivityMultiplier * Time.deltaTime * 300f;
    if (firstPerson)
    {
      transform.rotation = Quaternion.Euler(firstPersonRotation.y, firstPersonRotation.x, 0f);
    }
    else
    {
      Vector3 direction = GetLookDirection();
      if (direction.magnitude > 0.1f)
      {
        transform.rotation = Quaternion.LookRotation(direction);
      }
    }
  }

  void Action()
  {
    ControlLights();
    ToggleView();
  }

  void ControlLights()
  {
    if (Input.GetKeyDown(KeyCode.Mouse1))
    {
      lightState = !lightState;
      foreach (Light light in lights)
      {
        light.enabled = lightState;
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
    float e = 0.01f;
    Vector3 modelForward = ProjectionOnGroundPlane(transform.forward).normalized * e;
    Vector3 right = thirdPersonCamera.transform.right;
    Vector3 forward = thirdPersonCamera.transform.forward;

    float horizontalAxisValue = Input.GetAxis(horizontalAxis);
    float verticalAxisValue = Input.GetAxis(verticalAxis);


    Vector3 direction = (right * horizontalAxisValue) + (forward * verticalAxisValue) + modelForward;

    if (firstPerson == false)
    {
      direction = ProjectionOnGroundPlane(direction);
      direction += (Vector3.up * Input.GetAxis("Ascend") * 0.9f);
    }
    return direction;
  }

  private Vector3 ProjectionOnGroundPlane(Vector3 v)
  {
    Vector3 normal = Vector3.up;
    return Vector3.ProjectOnPlane(v, normal);
  }

}
