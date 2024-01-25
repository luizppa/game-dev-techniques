using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CameraListener
{
  public void OnToggleView();
}

[RequireComponent(typeof(ThirdPersonCamera))]
[RequireComponent(typeof(FirstPersonCamera))]
public class CameraManager : MonoBehaviour
{

  private ThirdPersonCamera thirdPersonCamera = null;
  private FirstPersonCamera firstPersonCamera = null;
  private bool firstPerson = false;
  private List<CameraListener> listeners = new List<CameraListener>();

  void Start()
  {
    thirdPersonCamera = GetComponent<ThirdPersonCamera>();
    firstPersonCamera = GetComponent<FirstPersonCamera>();
    SetEnabledCamera();
  }

  void Update()
  {
    ToggleView();
  }

  void ToggleView()
  {
    if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.JoystickButton4))
    {
      firstPerson = !firstPerson;
      SetEnabledCamera(true);
      foreach (CameraListener listener in listeners)
      {
        listener.OnToggleView();
      }
    }
  }

  void SetEnabledCamera(bool animate = false)
  {
    if (firstPerson)
    {
      thirdPersonCamera.enabled = false;
      if (animate)
      {
        StartCoroutine(AnimateToFirstPerson());
      }
      else
      {
        firstPersonCamera.enabled = true;
      }
    }
    else
    {
      firstPersonCamera.enabled = false;
      thirdPersonCamera.enabled = true;
    }
  }

  private IEnumerator AnimateToFirstPerson()
  {
    Vector3 target = firstPersonCamera.GetTargetPosition();
    while ((transform.position - target).magnitude > 0.1f)
    {
      transform.position = Vector3.MoveTowards(transform.position, target, 40f * Time.deltaTime);
      yield return null;
    }
    firstPersonCamera.enabled = true;
  }

  public void AddListener(CameraListener listener)
  {
    listeners.Add(listener);
  }

  public bool IsFirstPerson()
  {
    return firstPerson;
  }

  public bool IsThirdPerson()
  {
    return !firstPerson;
  }
}
