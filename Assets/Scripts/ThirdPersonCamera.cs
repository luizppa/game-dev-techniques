using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum OrbitPosition
{
  Top,
  Middle,
  Bottom
}

#region Helper Classes

[Serializable]
public class OrbitRing
{
  public float radius = 3f;
  public float height = 2.5f;
  public Color color = Color.red;

  public OrbitRing(float radius, float height, Color color)
  {
    this.radius = radius;
    this.height = height;
    this.color = color;
  }

  public OrbitRing(float radius, float height)
  {
    this.radius = radius;
    this.height = height;
    this.color = Color.green;
  }

  public float GetBorderDistanceToReference()
  {
    return Mathf.Sqrt((radius * radius) + (height * height));
  }
}

[Serializable]
public class ZoomOutOnMotionEffect
{
  public bool enabled = true;
  public float startSpeed = 10f;
  public float capSpeed = 15f;
  public float startDistanceRatio = 0f;
  public float capDistanceRatio = 0.3f;

  public float GetDistanceIncreaseForSpeed(float speed)
  {
    if (enabled && speed > startSpeed)
    {
      float speedRatio = Mathf.InverseLerp(startSpeed, capSpeed, speed);
      float distanceIncrease = 1 + Mathf.Lerp(startDistanceRatio, capDistanceRatio, speedRatio);
      return distanceIncrease;
    }
    else
    {
      return 1f;
    }
  }
}

[Serializable]
public class MotionShakeEffect
{
  public bool enabled = true;
  public float startSpeed = 10f;
  public float capSpeed = 15f;

  [Header("Vertical")]
  public float verticalStartIntensity = 0.02f;
  public float verticalCapIntensity = 0.05f;
  public float verticalSpeed = 15f;
  [Range(0, 2)] public float verticalPhase = 0.5f;

  [Header("Horizontal")]
  public float horizontalStartIntensity = 0.03f;
  public float horizontalCapIntensity = 0.07f;
  public float horizontalSpeed = 7.5f;
  [Range(0, 2)] public float horizontalPhase = 0f;

  private float phase = 0f;
  private bool running = false;

  public Vector3 Update(float speed, float deltaTime)
  {
    if (speed >= startSpeed)
    {
      if (!running)
      {
        Start();
      }
      phase += deltaTime;

      float speedRatio = Mathf.InverseLerp(startSpeed, capSpeed, speed);
      float verticalIntensity = Mathf.Lerp(verticalStartIntensity, verticalCapIntensity, speedRatio);
      float horizontalIntensity = Mathf.Lerp(horizontalStartIntensity, horizontalCapIntensity, speedRatio);

      float horizontal = Mathf.Sin((Time.time * horizontalSpeed) + (horizontalPhase * Mathf.PI)) * horizontalIntensity;
      float vertical = Mathf.Sin((Time.time * verticalSpeed) + (verticalPhase * Mathf.PI)) * verticalIntensity;
      return new Vector3(horizontal, vertical, 0f);
    }
    else
    {
      Stop();
      return Vector3.zero;
    }
  }

  private void Start()
  {
    phase = 0f;
    running = true;
  }

  private void Stop()
  {
    running = false;
  }

}

#endregion

[ExecuteInEditMode]
public class ThirdPersonCamera : MonoBehaviour
{

  #region Inspector Settings

  [Header("Editor Settings")]
  [SerializeField] bool showGizmos = true;
  [SerializeField] bool editorPreview = true;

  [Header("Targets")]
  [SerializeField] GameObject follow = null;
  [SerializeField] GameObject lookAt = null;

  [Header("Orbits")]
  [SerializeField] OrbitRing topRing = new OrbitRing(2f, 1.4f, Color.red);
  [SerializeField] OrbitRing middleRing = new OrbitRing(5f, 3f, Color.red);
  [SerializeField] OrbitRing bottomRing = new OrbitRing(1f, -1f, Color.red);

  [Header("Positioning")]
  [SerializeField] bool lockHeight = false;
  [SerializeField][ShowIf("lockHeight")] float fixedHeight = .5f;
  [SerializeField] bool lockTranslation = false;
  [SerializeField][Range(0f, 360f)][ShowIf("lockTranslation")] float fixedTranslation = 0f;
  [SerializeField] bool avoidClipping = true;
  [SerializeField] float clipDistance = 5f;
  [ShowIf(nameof(avoidClipping))][SerializeField] float clippingOffset = 0f;
  [SerializeField][Range(-180, 180)] float horizontalTilt = 0f;
  [SerializeField] float horizontalOffset = 0f;
  [SerializeField][Range(-180, 180)] float verticalTilt = 0f;
  [SerializeField] float verticalOffset = 0f;
  [SerializeField] bool useTargetNormal = true;

  [Header("Controls")]
  [SerializeField] bool captureCursor = false;

  [Header("X axis")]
  [SerializeField] string horizontalAxis = "Mouse X";
  [SerializeField] float horizontalSensitivity = 1f;
  [SerializeField] bool invertX = false;
  [Header("Y axis")]
  [SerializeField] string verticalAxis = "Mouse Y";
  [SerializeField] float verticalSensitivity = 0.8f;
  [SerializeField] bool invertY = true;

  [Header("Effects")]
  [SerializeField] ZoomOutOnMotionEffect zoomOutOnMotion = new ZoomOutOnMotionEffect();
  [SerializeField] MotionShakeEffect motionShake = new MotionShakeEffect();

  #endregion

  #region Private Variables

  private float cameraTranslation = 0f;
  private float verticalMultiplier = 10f;
  private float referenceHeight = 0f;
  private float referenceDistance;
  private float noClippingHeight;
  private float noClippingDistance;
  private OrbitRing cameraRing = null;
  private Vector3 up;
  private Vector3 right;
  private Vector3 forward;

  #endregion

  // ===================== Lifecycle ===================== //
  #region Lifecycle Methods

  void Start()
  {
    referenceHeight = middleRing.height;
  }

  void Update()
  {
    if ((Application.isPlaying || editorPreview) && Time.timeScale > 0)
    {
      if (captureCursor && Application.isPlaying)
      {
        Cursor.lockState = CursorLockMode.Locked;
      }
      SetNormalVectors();
      SetPosition();
      SetRotation();
      ApplyEffects();
    }
  }

  private void OnDrawGizmos()
  {
    if (follow != null && showGizmos)
    {
      DrawRing(topRing);
      DrawRing(middleRing);
      DrawRing(bottomRing);
    }
  }

  #endregion

  // ===================== Update steps ===================== //
  #region Update Steps

  private void SetNormalVectors()
  {
    up = useTargetNormal ? follow.transform.up : Vector3.up;
    right = Vector3.Cross(up, Vector3.right);
    forward = Vector3.Cross(up, right);
  }

  private void SetPosition()
  {
    ReadInputs();
    referenceDistance = 0f;

    cameraRing = GetCameraRing();

    referenceHeight = cameraRing.height;
    float distance = cameraRing.GetBorderDistanceToReference();
    referenceDistance = Mathf.Sqrt((distance * distance) - (referenceHeight * referenceHeight));
    referenceDistance = ApplyZoomOutOnMotion(referenceDistance);
    if (avoidClipping)
    {
      CorrectClipping(Mathf.Min(distance, clipDistance));
    }

    Vector3 heightVector = up * (avoidClipping ? noClippingHeight : referenceHeight);
    Vector3 distanceVector = -forward * (avoidClipping ? noClippingDistance : referenceDistance);

    transform.position = follow.transform.position + heightVector + distanceVector;
    transform.RotateAround(follow.transform.position, up, cameraTranslation);
  }

  private void SetRotation()
  {
    LookAt(up, lookAt.transform);

    Vector3 verticalAngles = forward * verticalTilt;
    Vector3 horizontalAngles = up * horizontalTilt;

    Vector3 eulerRotation = verticalAngles + horizontalAngles;
    transform.Rotate(eulerRotation.x, eulerRotation.y, eulerRotation.z);
    ApplyPositionOffset();
  }

  private void ApplyEffects()
  {
    if (motionShake.enabled)
    {
      ApplyMotionShake();
    }
  }

  #endregion

  // ===================== Input ===================== //
  #region Input Methods

  private void ReadInputs()
  {
    if (lockHeight)
    {
      referenceHeight = fixedHeight;
    }
    else if (Application.isPlaying)
    {
      referenceHeight += Input.GetAxis(verticalAxis) * verticalSensitivity * (invertY ? -1 : 1);
    }

    if (lockTranslation)
    {
      cameraTranslation = fixedTranslation;
    }
    else if (Application.isPlaying)
    {
      cameraTranslation += Input.GetAxis(horizontalAxis) * verticalMultiplier * horizontalSensitivity * (invertX ? -1 : 1);
      if (cameraTranslation > 360f)
      {
        cameraTranslation -= 360f;
      }
      else if (cameraTranslation < 0f)
      {
        cameraTranslation += 360f;
      }
    }
  }

  #endregion

  // ===================== Positioning ===================== //
  #region Positioning Methods

  private OrbitRing GetCameraRing()
  {
    if (referenceHeight >= topRing.height)
    {
      return new OrbitRing(topRing.radius, topRing.height);
    }
    else if (referenceHeight >= middleRing.height)
    {
      float radius = EaseLerpRingRadius(middleRing, topRing);
      return new OrbitRing(radius, referenceHeight);
    }
    else if (referenceHeight >= bottomRing.height)
    {
      float radius = EaseLerpRingRadius(bottomRing, middleRing);
      return new OrbitRing(radius, referenceHeight);
    }
    else
    {
      return new OrbitRing(bottomRing.radius, bottomRing.height);
    }
  }

  private void CorrectClipping(float raycastDistance)
  {
    RaycastHit hit;
    Ray ray = new Ray(follow.transform.position, (transform.position - follow.transform.position).normalized);

    if (avoidClipping && Physics.Raycast(ray, out hit, raycastDistance))
    {
      float safeDistance = hit.distance - clippingOffset;
      float sinAngl = referenceHeight / raycastDistance;
      float cosAngl = referenceDistance / raycastDistance;

      noClippingHeight = safeDistance * sinAngl;
      noClippingDistance = safeDistance * cosAngl;
    }
    else
    {
      noClippingHeight = referenceHeight;
      noClippingDistance = referenceDistance;
    }
  }

  private void ApplyPositionOffset()
  {
    transform.position = transform.position + (transform.right * horizontalOffset) + (transform.up * verticalOffset);
  }

  #endregion

  // ===================== Rotation ===================== //
  #region Rotation Methods

  private void LookAt(Vector3 normal, Transform lookAt)
  {
    Vector3 targetDirection = (lookAt.position - transform.position).normalized;
    transform.localRotation = Quaternion.LookRotation(targetDirection, normal);
  }

  #endregion

  // ===================== Effects ===================== //
  #region Effects Methods

  private float ApplyZoomOutOnMotion(float distance)
  {
    Rigidbody rb = follow.GetComponent<Rigidbody>();
    if (rb != null)
    {
      float speed = follow.GetComponent<Rigidbody>().velocity.magnitude;
      float distanceIncrease = zoomOutOnMotion.GetDistanceIncreaseForSpeed(speed);
      return distanceIncrease * distance;
    }
    else
    {
      return distance;
    }
  }

  private void ApplyMotionShake()
  {
    Rigidbody rb = follow.GetComponent<Rigidbody>();
    if (rb == null)
    {
      return;
    }
    float speed = follow.GetComponent<Rigidbody>().velocity.magnitude;
    Vector3 shake = motionShake.Update(speed, Time.deltaTime);
    Vector3 relativeShake = transform.right * shake.x + transform.up * shake.y;
    transform.position += relativeShake;
  }

  #endregion

  // ===================== Utils ===================== //
  #region Utils Methods

  private float EaseLerpRingRadius(OrbitRing r1, OrbitRing r2)
  {
    float lerpState = Mathf.InverseLerp(r1.height, r2.height, referenceHeight);
    if (r1.radius > r2.radius)
    {
      lerpState = lerpState * lerpState;
    }
    else
    {
      lerpState = Mathf.Sqrt(lerpState);
    }
    float radius = Mathf.Lerp(r1.radius, r2.radius, lerpState);
    return radius;
  }

  private void DrawRing(OrbitRing ring)
  {
#if UNITY_EDITOR
    Handles.color = ring.color;
    Vector3 position = follow.transform.position + (up * ring.height);
    Handles.DrawWireDisc(position, up, ring.radius);
#endif
  }

  #endregion

  // ===================== Setters ===================== //
  #region Setters Methods

  public void SetFollow(GameObject follow)
  {
    this.follow = follow;
  }

  public void SetLookAt(GameObject lookAt)
  {
    this.lookAt = lookAt;
  }

  public void SetOrbitRing(OrbitPosition position, OrbitRing orbit)
  {
    if (position == OrbitPosition.Top)
    {
      topRing = orbit;
    }
    else if (position == OrbitPosition.Middle)
    {
      middleRing = orbit;
    }
    else if (position == OrbitPosition.Bottom)
    {
      bottomRing = orbit;
    }
  }

  public void SetLockHeight(bool lockHeight)
  {
    this.lockHeight = lockHeight;
  }

  public void SetLockTranslation(bool lockTranslation)
  {
    this.lockTranslation = lockTranslation;
  }

  public void SetAvoidClipping(bool avoidClipping)
  {
    this.avoidClipping = avoidClipping;
  }

  public void SetClipDistance(float clipDistance)
  {
    this.clipDistance = clipDistance;
  }

  public void SetClippingOffset(float clippingOffset)
  {
    this.clippingOffset = clippingOffset;
  }

  public void SetHorizontalTilt(float horizontalTilt)
  {
    this.horizontalTilt = horizontalTilt;
  }

  public void SetHorizontalOffset(float horizontalOffset)
  {
    this.horizontalOffset = horizontalOffset;
  }

  public void SetVerticalTilt(float verticalTilt)
  {
    this.verticalTilt = verticalTilt;
  }
  public void SetVerticalOffset(float verticalOffset)
  {
    this.verticalOffset = verticalOffset;
  }
  public void SetUseTargetNormal(bool useTargetNormal)
  {
    this.useTargetNormal = useTargetNormal;
  }

  public void SetCaptureCursor(bool captureCursor)
  {
    this.captureCursor = captureCursor;
  }

  public void SetHorizontalAxis(string horizontalAxis)
  {
    this.horizontalAxis = horizontalAxis;
  }

  public void SetHorizontalSensitivity(float horizontalSensitivity)
  {
    this.horizontalSensitivity = horizontalSensitivity;
  }

  public void SetInvertX(bool invertX)
  {
    this.invertX = invertX;
  }

  public void SetVerticalAxis(string verticalAxis)
  {
    this.verticalAxis = verticalAxis;
  }

  public void SetVerticalSensitivity(float verticalSensitivity)
  {
    this.verticalSensitivity = verticalSensitivity;
  }

  public void SetInvertY(bool invertY)
  {
    this.invertY = invertY;
  }

  public void SetZoomOutOnMotion(ZoomOutOnMotionEffect zoomOutOnMotion)
  {
    this.zoomOutOnMotion = zoomOutOnMotion;
  }

  public void SetMotionShake(MotionShakeEffect motionShake)
  {
    this.motionShake = motionShake;
  }

  #endregion
}
