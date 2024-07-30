using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct Biome {
  [SerializeField] public string name;

  [Header("Water Settings")]
  [SerializeField] public Color shallowColor;
  [SerializeField] public Color deepColor;

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
  [SerializeField] List<string> biomeFeatures = new List<string>{
    "Temperature",
    "Seismic Activity",
    "Erosion",
    "Precipitation"
  };
  [SerializeField] List<Texture2D> biomeMaps = new List<Texture2D>();
  [SerializeField] int biomesSeed = 0;
  [SerializeField] float biomesScale = 1f;
  [SerializeField] int biomesSize = 256;
  [SerializeField] BiomeData biomeData = new BiomeData();

  private Camera gameCamera = null;
  private Material waterMaterial = null;

  override protected void Awake()
  {
    base.Awake();

    while(biomeMaps.Count < biomeFeatures.Count)
    {
      biomeMaps.Add(NoiseUtils.GenerateNoiseMap(new Vector2Int(biomesSize, biomesSize), biomesScale, biomesSeed));
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
      waterMaterial = waterSurface.GetComponent<MeshRenderer>().material;
      waterMaterial.SetTexture("_ReflectionNoiseTexture", reflectionMap);
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
      Biome biome = GetBiomeAtPosition(playerPosition.position);
      if (gameCamera.transform.position.y - waterLevel <= 0.2)
      {
        float fogHeight = Mathf.InverseLerp(startFogHeight, endFogHeight, playerPosition.position.y);
        Color fogColor = Color.Lerp(biome.startFogColor, biome.endFogColor, fogHeight) * sun.intensity;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = Mathf.Lerp(biome.startFogDensity, biome.endFogDensity, fogHeight);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
      }
      else
      {
        RenderSettings.fogColor = dryLandFogColor;
        RenderSettings.skybox.SetColor("_EquatorColor", biome.deepColor);
        RenderSettings.skybox.SetColor("_GroundColor", biome.deepColor);
        RenderSettings.fogDensity = dryLandFogDensity;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        waterMaterial.SetColor("_ShallowColor", biome.shallowColor);
        waterMaterial.SetColor("_DeepColor", biome.deepColor);
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

  public Texture2D GetBiomeMap(string feature = "Temperature"){
    return biomeMaps[biomeFeatures.IndexOf(feature)];
  }

  public Biome GetBiomeAtPosition(Vector3 position){
    float[] features = new float[biomeFeatures.Count];
    for(int i = 0; i < biomeFeatures.Count; i++){
      features[i] = biomeMaps[i].GetPixelBilinear(position.x * 0.0005f, position.z * 0.0005f).r;
    }
    return biomeData.GetBiome(features);
  }
}
