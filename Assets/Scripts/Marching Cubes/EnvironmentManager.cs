using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvironmentManager : SingletonMonoBehaviour<EnvironmentManager>
{
  [SerializeField] Transform playerPosition = null;

  [Header("Light")]
  [SerializeField] Light sun = null;
  [SerializeField] Gradient sunColor = null;
  [SerializeField] float sunIntensity = 1f;
  [SerializeField] float dayNightCycleSpeed = 1f;

  [Header("Water")]
  [SerializeField] float waterLevel = 50f;

  [Header("Fog Settings")]
  [SerializeField] float startFogHeight = 35f;
  [SerializeField] Color startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f);
  [SerializeField] float startFogDensity = 0.06f;
  [SerializeField] float endFogHeight = 5f;
  [SerializeField] Color endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f);
  [SerializeField] float endFogDensity = 0.15f;

  private Camera gameCamera = null;

  void Start()
  {
    if (gameCamera == null)
    {
      gameCamera = Camera.main;
    }
    if (sun)
    {
      sun.transform.position = new Vector3(0, 1000, 0);
      sun.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
  }

  void Update()
  {
    if (SceneManager.GetActiveScene().name != "MarchingCubes")
    {
      Destroy(gameObject);
    }
    if (sun)
    {
      DayNightCycle();
    }
    SetFog();
  }

  void DayNightCycle()
  {
    sun.transform.RotateAround(Vector3.zero, Vector3.right, dayNightCycleSpeed * Time.deltaTime);
    sun.transform.LookAt(Vector3.zero);
    float rotationAngle = (sun.transform.rotation.eulerAngles.x % 360f) / 360f;
    sun.color = sunColor.Evaluate(rotationAngle);
    float lerpBound = rotationAngle > 0.25 ? 0.5f : 0f;
    float lerpValue = Mathf.InverseLerp(lerpBound, 0.25f, rotationAngle);
    sun.intensity = Mathf.Lerp(0.1f, sunIntensity, lerpValue);
    RenderSettings.ambientLight = sun.color;
    RenderSettings.ambientIntensity = sun.intensity;
  }

  void SetFog()
  {
    if (playerPosition)
    {
      if (gameCamera.transform.position.y - 0.2 <= waterLevel)
      {
        float fogHeight = Mathf.InverseLerp(startFogHeight, endFogHeight, playerPosition.position.y);
        RenderSettings.fogColor = Color.Lerp(startFogColor, endFogColor, fogHeight) * sun.intensity;
        RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, endFogDensity, fogHeight);
      }
      else
      {
        RenderSettings.fogDensity = 0f;
      }
    }
  }

  // ================================ Getters ================================ //
  public float GetWaterLevel()
  {
    return waterLevel;
  }

  public void SetDayNightCycleSpeed(float newSpeed)
  {
    dayNightCycleSpeed = newSpeed;
  }
}
