using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ThirdPersonCamera))]
[RequireComponent(typeof(FirstPersonCamera))]
public class CameraManager : MonoBehaviour
{
  private ThirdPersonCamera thirdPersonCamera = null;
  private FirstPersonCamera firstPersonCamera = null;
  private bool firstPerson = false;

  void Start()
  {
    thirdPersonCamera = GetComponent<ThirdPersonCamera>();
    firstPersonCamera = GetComponent<FirstPersonCamera>();
    SetEnabledCamera();
  }

  // Update is called once per frame
  void Update()
  {
    ToogleView();
  }

  void ToogleView()
  {
    if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.JoystickButton4))
    {
      firstPerson = !firstPerson;
      SetEnabledCamera();
    }
  }

  void SetEnabledCamera()
  {
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

  public bool IsFirstPerson()
  {
    return firstPerson;
  }

  public bool IsThirdPerson()
  {
    return !firstPerson;
  }
}
