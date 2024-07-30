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
  private GPUChunk playerChunk = null;
  private Biome biome = BiomeData.placeholderBiome;

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
    UpdateBiomeFog();
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
  
  float[] ResolveBiomeValues(){
    const float LOW_FEATURE_LEVEL = .40f;
    const float HIGH_FEATURE_LEVEL = .60f;
    int biomeCount = (int)Mathf.Pow(biomeMaps.Count, 2);
    float[] values = new float[biomeCount];
    float[] features = new float[biomeMaps.Count];
    values[0] = 1.0f;

    for(int i = 0; i < biomeMaps.Count; i++){
      features[i] = biomeMaps[i].GetPixelBilinear(playerPosition.position.x, playerPosition.position.z).r;
    }

    for (int depth = 0; depth < biomeMaps.Count; depth++){
      for(int index = 0; index < biomeCount; index = (int)(index + biomeCount / Mathf.Pow(2, depth))){
        float t = values[index];

        int slice = (int)(biomeCount / Mathf.Pow(2, depth));
        int leftIndex = index;
        int rightIndex = index + (slice / 2); 

        float feature = features[depth];
        // Debug.Log(index);
        // Debug.Log(feature);
        if (feature <= LOW_FEATURE_LEVEL){
          // create the left child with t = 1.0 * parent.t and the right child with t = 0.0
          values[leftIndex] = 1.0f * t;
          values[rightIndex] = 0.0f;
        } else if (feature >= HIGH_FEATURE_LEVEL){
          // create the left child with 0 and the right child with t = 1.0 * parent.t
          values[leftIndex] = 0.0f;
          values[rightIndex] = 1.0f * t;
        } else {
          // calculate t with inverse lerp and create the left and right children
          float leftT = Mathf.InverseLerp(HIGH_FEATURE_LEVEL, LOW_FEATURE_LEVEL, feature);
          values[leftIndex] = leftT * t;
          values[rightIndex] = (1.0f - leftT) * t;
        }
      }
      // Debug.Log(string.Join(", ", values));
    }

    Debug.Log(string.Join(", ", features));
    // Debug.Log(string.Join(", ", values));

    return values;
  }

  void UpdateBiomeFog(){
    GPUChunk currentChunk = SurfaceManager.Instance.GetPlayerChunk();
    if(currentChunk == null) return;
    int chunkSize = SurfaceManager.Instance.GetChunkSize();
    playerChunk = currentChunk;
    // Texture2D biomeValues1 = new Texture2D(chunkSize, chunkSize);
    // Texture2D biomeValues2 = new Texture2D(chunkSize, chunkSize);
    // Texture2D biomeValues3 = new Texture2D(chunkSize, chunkSize);
    // Texture2D biomeValues4 = new Texture2D(chunkSize, chunkSize);
    
    // RenderTexture.active = playerChunk.biomeOutput1;
    // biomeValues1.ReadPixels(new Rect(0, 0, chunkSize, chunkSize), 0, 0);
    // biomeValues1.Apply();
    // RenderTexture.active = playerChunk.biomeOutput2;
    // biomeValues2.ReadPixels(new Rect(0, 0, chunkSize, chunkSize), 0, 0);
    // biomeValues2.Apply();
    // RenderTexture.active = playerChunk.biomeOutput3;
    // biomeValues3.ReadPixels(new Rect(0, 0, chunkSize, chunkSize), 0, 0);
    // biomeValues3.Apply();
    // RenderTexture.active = playerChunk.biomeOutput4;
    // biomeValues4.ReadPixels(new Rect(0, 0, chunkSize, chunkSize), 0, 0);
    // biomeValues4.Apply();
    // RenderTexture.active = null;

    // Color[] biomeTextures = {
    //   biomeValues1.GetPixelBilinear(playerPosition.position.x, playerPosition.position.z),
    //   biomeValues2.GetPixelBilinear(playerPosition.position.x, playerPosition.position.z),
    //   biomeValues3.GetPixelBilinear(playerPosition.position.x, playerPosition.position.z),
    //   biomeValues4.GetPixelBilinear(playerPosition.position.x, playerPosition.position.z)
    // };
    // float[] biomeValues = {
    //   biomeTextures[0].r, biomeTextures[0].g, biomeTextures[0].b, biomeTextures[0].a,
    //   biomeTextures[1].r, biomeTextures[1].g, biomeTextures[1].b, biomeTextures[1].a,
    //   biomeTextures[2].r, biomeTextures[2].g, biomeTextures[2].b, biomeTextures[2].a,
    //   biomeTextures[3].r, biomeTextures[3].g, biomeTextures[3].b, biomeTextures[3].a
    // };

    float[] biomeValues = ResolveBiomeValues();

    biome = biomeData.GetBiome(biomeValues);
  }

  void SetFog()
  {
    if (playerPosition == null)
    {
      return;
    }
    // Biome biome = GetBiomeAtPosition(playerPosition.position);
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
}
