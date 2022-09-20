using System.Linq;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
  [SerializeField] ComputeShader rayTracingShader = null;

  [Header("Skybox")]
  [SerializeField] Texture2D skyboxTexture = null;
  [SerializeField] Color solidSkyboxColor = Color.black;

  [Header("Anti Aliasing")]
  [SerializeField] bool useAntiAliasing = false;
  [SerializeField][Range(0f, 1f)] float antiAliasingIntensity = .8f;

  [Header("Ray Tracing")]
  [SerializeField] bool useRayTracing = true;
  [SerializeField] int maxBounces = 8;

  // Scene
  private Camera rtxCamera;
  private Light directionalLight;
  private RenderTexture converged;
  private RenderTexture target;

  // Reference values
  private Color previousSkyboxColor;
  private Texture2D previousSkyboxTexture;
  private bool previousUseAntiAliasing;
  private float previousAntiAliasingIntensity;
  private bool previousUseRayTracing;
  private int previousMaxBounces;

  // Anti-aliasing
  private uint currentSample = 0;
  private Material addMaterial;

  // Entities
  private RayTracingSphere[] spheres;
  private ComputeBuffer sphereBuffer;


  void Awake()
  {
    directionalLight = FindObjectsOfType<Light>().First(l => l.type == LightType.Directional);
    rtxCamera = GetComponent<Camera>();
    GetEntities();
  }

  private void OnEnable()
  {
    currentSample = 0;
    SetUpScene();
  }

  void Start()
  {
    UpdateReferenceValues();
  }

  void Update()
  {
    if (skyboxTexture == null || solidSkyboxColor != previousSkyboxColor || skyboxTexture != previousSkyboxTexture)
    {
      UpdateSkybox();
    }

    if (transform.hasChanged || directionalLight.transform.hasChanged || ShouldUpdate())
    {
      currentSample = 0;
      UpdateReferenceValues();
      transform.hasChanged = false;
      directionalLight.transform.hasChanged = false;
    }

    if (ShouldUpdateEntities())
    {
      currentSample = 0;
      SetUpScene();
    }
  }

  private void OnRenderImage(RenderTexture source, RenderTexture destination)
  {
    SetShaderParameters();
    Render(destination);
  }

  void OnDisable()
  {
    sphereBuffer?.Release();
  }

  private void Render(RenderTexture destination)
  {
    // Make sure we have a current render target
    InitRenderTexture();

    // Set the target and dispatch the compute shader
    rayTracingShader.SetTexture(0, "Result", target);
    int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
    int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
    rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

    // Blit the result texture to the screen
    if (addMaterial == null)
    {
      addMaterial = new Material(Shader.Find("Hidden/AddShader"));
    }
    addMaterial.SetFloat("_Sample", currentSample);
    Graphics.Blit(target, converged, addMaterial);
    Graphics.Blit(converged, destination);
    currentSample++;
  }

  private bool ShouldUpdateEntities()
  {
    return spheres.Any(s => s.ShouldUpdate());
  }

  private void SetUpScene()
  {
    Sphere[] spheresData = new Sphere[spheres.Length];
    for (int i = 0; i < spheres.Length; i++)
    {
      spheresData[i] = spheres[i].GetSphere();
    }
    if (sphereBuffer != null)
    {
      sphereBuffer.Release();
    }
    sphereBuffer = new ComputeBuffer(spheres.Length, 40);
    sphereBuffer.SetData(spheresData);
  }

  private void GetEntities()
  {
    spheres = FindObjectsOfType<RayTracingSphere>();
  }

  private bool ShouldUpdate()
  {
    return useRayTracing != previousUseRayTracing || maxBounces != previousMaxBounces || useAntiAliasing != previousUseAntiAliasing || antiAliasingIntensity != previousAntiAliasingIntensity;
  }

  private void UpdateReferenceValues()
  {
    previousUseAntiAliasing = useAntiAliasing;
    previousAntiAliasingIntensity = antiAliasingIntensity;
    previousUseRayTracing = useRayTracing;
    previousMaxBounces = maxBounces;
  }

  private void UpdateSkybox()
  {
    if (skyboxTexture == null)
    {
      skyboxTexture = new Texture2D(Screen.width, Screen.height);
      Color[] pixels = Enumerable.Repeat(solidSkyboxColor, Screen.width * Screen.height).ToArray();
      skyboxTexture.SetPixels(pixels);
      skyboxTexture.Apply();
    }
    previousSkyboxColor = solidSkyboxColor;
    previousSkyboxTexture = skyboxTexture;
    currentSample = 0;
  }

  private void InitRenderTexture()
  {
    if (target == null || target.width != Screen.width || target.height != Screen.height)
    {
      // Release render texture if we already have one
      if (target != null)
      {
        target.Release();
      }

      // Get a render target for Ray Tracing
      target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      target.enableRandomWrite = true;
      target.Create();
    }


    if (converged == null || converged.width != Screen.width || converged.height != Screen.height)
    {
      // Release render texture if we already have one
      if (converged != null)
      {
        converged.Release();
      }

      // Get a render target for Ray Tracing
      converged = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      converged.enableRandomWrite = true;
      converged.Create();
    }
  }

  private void SetShaderParameters()
  {
    if (useAntiAliasing)
    {
      rayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value) * antiAliasingIntensity);
    }
    else
    {
      rayTracingShader.SetVector("_PixelOffset", new Vector2(0.5f, 0.5f));
    }
    Vector3 l = directionalLight.transform.forward;
    rayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, directionalLight.intensity));
    rayTracingShader.SetMatrix("_CameraToWorld", rtxCamera.cameraToWorldMatrix);
    rayTracingShader.SetMatrix("_CameraInverseProjection", rtxCamera.projectionMatrix.inverse);
    rayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);
    rayTracingShader.SetInt("_MaxBounces", maxBounces);
    rayTracingShader.SetBool("_UseRayTracing", useRayTracing);
    rayTracingShader.SetBuffer(0, "_Spheres", sphereBuffer);
    rayTracingShader.SetFloat("_Seed", Random.value);
  }
}
