using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
  }

  void Update()
  {
    SetFog();
  }

  void SetFog()
  {
    float fogHeight = Mathf.InverseLerp(startFogHeight, endFogHeight, playerPosition.position.y);
    RenderSettings.fogColor = Color.Lerp(startFogColor, endFogColor, fogHeight);
    RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, endFogDensity, fogHeight);
    Camera.main.backgroundColor = RenderSettings.fogColor;
  }
}
