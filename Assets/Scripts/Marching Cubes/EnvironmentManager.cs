using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvironmentManager : MonoBehaviour
{
  [SerializeField] Transform playerPosition = null;

  [Header("Fog Settings")]
  [SerializeField] float startFogHeight = 35f;
  [SerializeField] Color startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f);
  [SerializeField] float startFogDensity = 0.06f;
  [SerializeField] float endFogHeight = 5f;
  [SerializeField] Color endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f);
  [SerializeField] float endFogDensity = 0.15f;

  private Camera thirdPersonCamera = null;
  private Camera firstPersonCamera = null;

  void Start()
  {
    if (FindObjectsOfType<EnvironmentManager>().Length > 1)
    {
      Destroy(gameObject);
    }
    else
    {
      DontDestroyOnLoad(gameObject);
    }
    thirdPersonCamera = GameObject.FindWithTag("ThirdPersonCamera").GetComponent<Camera>();
    firstPersonCamera = GameObject.FindWithTag("FirstPersonCamera").GetComponent<Camera>();
  }

  void Update()
  {
    if (SceneManager.GetActiveScene().name != "MarchingCubes")
    {
      Destroy(gameObject);
    }
    SetFog();
  }

  void SetFog()
  {
    if (playerPosition)
    {
      float fogHeight = Mathf.InverseLerp(startFogHeight, endFogHeight, playerPosition.position.y);
      RenderSettings.fogColor = Color.Lerp(startFogColor, endFogColor, fogHeight);
      RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, endFogDensity, fogHeight);
      firstPersonCamera.backgroundColor = RenderSettings.fogColor;
      thirdPersonCamera.backgroundColor = RenderSettings.fogColor;
    }
  }
}
