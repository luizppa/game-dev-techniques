using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct Biome {
  [SerializeField] public string name;

  [Header("Fog Settings")]
  [SerializeField] public Color startFogColor;
  [SerializeField] public float startFogDensity;
  [SerializeField] public Color endFogColor;
  [SerializeField] public float endFogDensity;
}

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
  [SerializeField] GameObject waterSurface = null;
  [SerializeField] Texture2D reflectionMap = null;

  [Header("Fog Settings")]
  [SerializeField] float dryLandFogDensity = .1f;
  [SerializeField] Color dryLandFogColor = Color.white;
  [SerializeField] float startFogHeight = 35f;
  [SerializeField] float endFogHeight = 5f;


  [Header("Biome Settings")]
  [SerializeField] Texture2D biomeMap = null;
  [SerializeField] int biomesSeed = 0;
  [SerializeField] float biomesScale = 1f;
  [SerializeField] int biomesSize = 256;
  [SerializeField] List<Biome> biomes = new List<Biome>{
    new Biome {
      name = "Warped Meadows",
      startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
      startFogDensity = 0.03f,
      endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
      endFogDensity = 0.05f
    },
    new Biome {
      name = "Stone Fields",
      startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
      startFogDensity = 0.06f,
      endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
      endFogDensity = 0.15f
    }
  };

  private Camera gameCamera = null;

  override protected void Awake()
  {
    base.Awake();
    if(!biomeMap)
    {
      biomeMap = NoiseUtils.GenerateNoiseMap(new Vector2Int(biomesSize, biomesSize), biomesScale, biomesSeed);
      // biomeMap = Texture2D.redTexture;
    }
    if(!reflectionMap)
    {
      reflectionMap = NoiseUtils.GenerateNoiseMap(new Vector2Int(1024, 1024), 0.05f, 1236, NOISE_TYPE.CELLULAR);
    }
  }

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
    if (waterSurface)
    {
      waterSurface.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_ReflectionNoiseTexture", reflectionMap);
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
      if (gameCamera.transform.position.y - waterLevel <= 0.2)
      {
        Biome biome = GetBiomeAtPosition(playerPosition.position);
        float fogHeight = Mathf.InverseLerp(startFogHeight, endFogHeight, playerPosition.position.y);
        RenderSettings.fogColor = Color.Lerp(biome.startFogColor, biome.endFogColor, fogHeight) * sun.intensity;
        RenderSettings.fogDensity = Mathf.Lerp(biome.startFogDensity, biome.endFogDensity, fogHeight);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
      }
      else
      {
        RenderSettings.fogColor = dryLandFogColor;
        RenderSettings.fogDensity = dryLandFogDensity;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
      }
    }
  }

  public void SetDayNightCycleSpeed(float newSpeed)
  {
    dayNightCycleSpeed = newSpeed;
  }

  // ================================ Getters ================================ //
  public float GetWaterLevel()
  {
    return waterLevel;
  }

  public Texture2D GetBiomeMap(){
    return biomeMap;
  }

  public Biome GetBiomeAtPosition(Vector3 position){
    float biomeValue = biomeMap.GetPixelBilinear(position.x * 0.0005f, position.z * 0.0005f).r;
    if(biomeValue < 0.46f){
      return biomes[0];
    }
    else if(biomeValue > 0.54f){
      return biomes[1];
    }
    else{
      Biome transitionBiome = new Biome{
        name = "Transition Biome",
        startFogColor = Color.Lerp(biomes[0].startFogColor, biomes[1].startFogColor, Mathf.InverseLerp(0.46f, 0.54f, biomeValue)),
        startFogDensity = Mathf.Lerp(biomes[0].startFogDensity, biomes[1].startFogDensity, Mathf.InverseLerp(0.46f, 0.54f, biomeValue)),
        endFogColor = Color.Lerp(biomes[0].endFogColor, biomes[1].endFogColor, Mathf.InverseLerp(0.46f, 0.54f, biomeValue)),
        endFogDensity = Mathf.Lerp(biomes[0].endFogDensity, biomes[1].endFogDensity, Mathf.InverseLerp(0.46f, 0.54f, biomeValue))
      };
      return transitionBiome;
    }
  }
}
