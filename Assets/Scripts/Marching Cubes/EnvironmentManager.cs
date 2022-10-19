using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvironmentManager : MonoBehaviour
{
  [SerializeField] Transform playerPosition = null;

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
    if (FindObjectsOfType<EnvironmentManager>().Length > 1)
    {
      Destroy(gameObject);
    }
    else
    {
      DontDestroyOnLoad(gameObject);
    }
    if (gameCamera == null)
    {
      gameCamera = Camera.main;
    }
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
      if (gameCamera.transform.position.y - 0.2 <= waterLevel)
      {
        float fogHeight = Mathf.InverseLerp(startFogHeight, endFogHeight, playerPosition.position.y);
        RenderSettings.fogColor = Color.Lerp(startFogColor, endFogColor, fogHeight);
        RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, endFogDensity, fogHeight);
        gameCamera.backgroundColor = RenderSettings.fogColor;
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
      }
      else
      {
        RenderSettings.fogColor = Color.white;
        RenderSettings.fogDensity = 0f;
        gameCamera.backgroundColor = Color.white;
        gameCamera.clearFlags = CameraClearFlags.Skybox;
      }
    }
  }

  // ================================ Getters ================================ //
  public float GetWaterLevel()
  {
    return waterLevel;
  }
}
