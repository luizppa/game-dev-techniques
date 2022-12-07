using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class SubmarineControl : MonoBehaviour, CameraListener
{
  [Header("Stats")]
  [SerializeField] float maxHealth = 100f;
  [SerializeField] float maxCollisionDamage = 20f;

  [Header("Controls")]
  [SerializeField] string horizontalAxis = "Horizontal";
  [SerializeField] string verticalAxis = "Vertical";
  [SerializeField] string horizontalLookAxis = "Look X";
  [SerializeField] string verticalLookAxis = "Look Y";
  [SerializeField] string ascendAxis = "Ascend";

  [Header("Terraform")]
  // [SerializeField] float terraformRadius = 5f;
  // [SerializeField] float terraformStrength = 0.5f;
  // [SerializeField] float terraformInterval = 0.5f;

  [Header("Effects")]
  [SerializeField] Rotate proppeler = null;
  [SerializeField] List<Light> lights = new List<Light>();
  [SerializeField] ParticleSystem waterParticles = null;
  [SerializeField] List<AudioClip> impactSounds = new List<AudioClip>();
  [SerializeField] GameObject impactParticles = null;

  [Header("Movement")]
  [SerializeField] float maxSpeed = 5f;
  [SerializeField] float acceleration = 2f;

  [Header("Camera")]
  [SerializeField] Camera gameCamera = null;
  [SerializeField] GameObject windShield = null;

  private float health;

  private bool lightState = true;
  // private bool canTerraform = true;
  private Vector2 firstPersonRotation = Vector2.zero;

  private Rigidbody rb = null;
  private AudioSource audioSource = null;
  private CameraManager cameraManager = null;
  private EnvironmentManager environmentManager = null;
  private LineRenderer lineRenderer = null;


  // ================================ Unity messages ================================ //
  void Start()
  {
    health = maxHealth;
    rb = GetComponent<Rigidbody>();
    audioSource = GetComponent<AudioSource>();
    lineRenderer = GetComponent<LineRenderer>();
    if (gameCamera == null)
    {
      gameCamera = Camera.main;
    }
    cameraManager = gameCamera.GetComponent<CameraManager>();
    cameraManager.AddListener(this);
    environmentManager = EnvironmentManager.Instance;
  }

  void FixedUpdate()
  {
    Move();
  }

  void Update()
  {
    UpdateEffects();
    Action();
    Rotate();
  }

  void OnCollisionEnter(Collision other)
  {
    PlayCollisionEffects(other.relativeVelocity.magnitude, other.contacts);
    ApplyCollisionDamage(other.relativeVelocity.magnitude);
  }

  public void OnToggleView()
  {
    if (cameraManager.IsFirstPerson())
    {
      firstPersonRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
    }
    if (windShield != null)
    {
      windShield.GetComponent<MeshRenderer>().enabled = !cameraManager.IsFirstPerson();
    }
  }

  // ================================ Movement ================================ //

  void Move()
  {
    Vector3 direction;
    direction = GetMoveDirection();
    if (cameraManager.IsThirdPerson())
    {
      direction = ProjectionOnGroundPlane(direction);
    }
    direction += (Vector3.up * Input.GetAxis(ascendAxis));

    if (direction.magnitude > 0f && IsSubmerged())
    {
      float instantAcceleration = acceleration * Time.fixedDeltaTime * 60f;
      rb.AddForce(direction.normalized * instantAcceleration, ForceMode.Acceleration);
      if (rb.velocity.magnitude > maxSpeed)
      {
        rb.velocity = rb.velocity.normalized * maxSpeed;
      }
    }

    rb.useGravity = !IsSubmerged();
  }

  void Rotate()
  {
    firstPersonRotation.x += Input.GetAxis(horizontalLookAxis) * Time.deltaTime * 2500;
    firstPersonRotation.y -= Input.GetAxis(verticalLookAxis) * Time.deltaTime * 1500f;
    if (cameraManager.IsFirstPerson())
    {
      transform.rotation = Quaternion.Euler(firstPersonRotation.y, firstPersonRotation.x, 0f);
    }
    else
    {
      Vector3 direction = GetLookDirection();
      if (direction.magnitude > 0.1f && Time.timeScale > 0f)
      {
        transform.rotation = Quaternion.LookRotation(direction);
      }
    }
  }

  void UpdateEffects()
  {
    if ((rb.velocity.magnitude < 0.1f || !IsSubmerged()) && waterParticles.isPlaying == true)
    {
      waterParticles.Stop();
    }
    else if (rb.velocity.magnitude > 0.1f && IsSubmerged() && waterParticles.isPlaying == false)
    {
      waterParticles.Play();
    }

    proppeler.SetSpeed(rb.velocity.magnitude);
  }

  // ================================ Actions ================================ //

  void Action()
  {
    ControlDayNightCycle();
    ControlLights();
    // Terraform();
  }

  void ControlDayNightCycle()
  {
    if (Input.GetKeyDown(KeyCode.L))
    {
      environmentManager.SetDayNightCycleSpeed(30f);
    }
    if (Input.GetKeyDown(KeyCode.K))
    {
      environmentManager.SetDayNightCycleSpeed(0.25f);
    }
  }

  void ControlLights()
  {
    if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Joystick1Button9))
    {
      lightState = !lightState;
      foreach (Light light in lights)
      {
        light.enabled = lightState;
      }
    }
  }

  // void Terraform()
  // {
  //   if(!canTerraform) return;
  //   if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Joystick1Button8))
  //   {
  //     RaycastHit hit;
  //     if (Physics.Raycast(transform.position, gameCamera.transform.forward, out hit, 10f, LayerMask.GetMask("Terrain")))
  //     {
  //       List<GPUChunk> chunks = GetTerraformAffectedChunks(hit.point);
  //       foreach (GPUChunk chunk in chunks)
  //       {
  //         chunk.Terraform(hit.point, terraformRadius, terraformStrength, TerraformMode.Add);
  //       }
  //       canTerraform = false;
  //       StartCoroutine(TerraformCooldown());
  //     }
  //   }
  // }

  // void DrawTerraformEffect(Vector3 position)
  // {
  //   lineRenderer.SetPositions(new Vector3[] { transform.position, position });
  // }

  // List<GPUChunk> GetTerraformAffectedChunks(Vector3 position)
  // {
  //   List<GPUChunk> chunks = new List<GPUChunk>();
  //   foreach (Collider collider in Physics.OverlapSphere(position, terraformRadius, LayerMask.GetMask("Terrain")))
  //   {
  //     GPUChunk chunk = collider.gameObject.GetComponent<GPUChunk>();
  //     if (chunk != null)
  //     {
  //       chunks.Add(chunk);
  //     }
  //   }
  //   return chunks;
  // }

  // IEnumerator TerraformCooldown()
  // {
  //   yield return new WaitForSeconds(terraformInterval);
  //   canTerraform = true;
  // }

  // ================================ Side effects ================================ //
  void PlayCollisionEffects(float relativeVelocity, ContactPoint[] contacts)
  {
    if (impactSounds.Count > 0)
    {
      int index = Random.Range(0, impactSounds.Count);
      float volumeScale = Mathf.InverseLerp(2f, 10f, relativeVelocity);
      audioSource.PlayOneShot(impactSounds[index], volumeScale);
    }
    if (impactParticles != null && relativeVelocity > 2f)
    {
      foreach (ContactPoint contact in contacts)
      {
        GameObject particles = Instantiate(impactParticles, contact.point, Quaternion.identity);
      }
    }
  }

  void ApplyCollisionDamage(float relativeVelocity)
  {
    float damageMultiplier = Mathf.InverseLerp(2f, 10f, relativeVelocity);
    float damage = maxCollisionDamage * damageMultiplier;
    health = Mathf.Max(0f, health - damage);
  }

  // ================================ Helpers ================================ //

  private bool IsSubmerged()
  {
    return transform.position.y < environmentManager.GetWaterLevel();
  }

  private bool IsCloseToSurface()
  {
    float distance = transform.position.y - environmentManager.GetWaterLevel();
    return Mathf.Abs(distance) < 1.5f;
  }

  private Vector3 GetMoveDirection()
  {
    Vector3 right = gameCamera.transform.right;
    Vector3 forward = gameCamera.transform.forward;
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
    Vector3 right = gameCamera.transform.right;
    Vector3 forward = gameCamera.transform.forward;

    float horizontalAxisValue = Input.GetAxis(horizontalAxis);
    float verticalAxisValue = Input.GetAxis(verticalAxis);

    Vector3 direction = (right * horizontalAxisValue) + (forward * verticalAxisValue) + modelForward;

    if (cameraManager.IsThirdPerson())
    {
      direction = ProjectionOnGroundPlane(direction);
      direction += (Vector3.up * Input.GetAxis(ascendAxis) * 0.9f);
    }
    return direction;
  }

  private Vector3 ProjectionOnGroundPlane(Vector3 v)
  {
    Vector3 normal = Vector3.up;
    return Vector3.ProjectOnPlane(v, normal);
  }

  // ================================ Getters ================================ //
  public float GetHealth()
  {
    return health;
  }

  public float GetMaxHealth()
  {
    return maxHealth;
  }

}
