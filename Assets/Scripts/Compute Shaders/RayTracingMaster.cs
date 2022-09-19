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

  private Camera _camera;
  private Light _directionalLight;
  private RenderTexture _target;

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
    _directionalLight = FindObjectsOfType<Light>().First(l => l.type == LightType.Directional);
    _camera = GetComponent<Camera>();
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

    if (transform.hasChanged || _directionalLight.transform.hasChanged || ShouldUpdate())
    {
      currentSample = 0;
      UpdateReferenceValues();
      transform.hasChanged = false;
      _directionalLight.transform.hasChanged = false;
    }
  }

  void OnDisable()
  {
    sphereBuffer?.Release();
  }

  private void SetUpScene()
  {
    Sphere[] spheresData = new Sphere[spheres.Length];
    for (int i = 0; i < spheres.Length; i++)
    {
      spheresData[i] = spheres[i].GetSphere();
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

  private void OnRenderImage(RenderTexture source, RenderTexture destination)
  {
    SetShaderParameters();
    Render(destination);
  }

  private void Render(RenderTexture destination)
  {
    // Make sure we have a current render target
    InitRenderTexture();

    // Set the target and dispatch the compute shader
    rayTracingShader.SetTexture(0, "Result", _target);
    int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
    int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
    rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

    // Blit the result texture to the screen
    if (addMaterial == null)
    {
      addMaterial = new Material(Shader.Find("Hidden/AddShader"));
    }
    addMaterial.SetFloat("_Sample", currentSample);
    Graphics.Blit(_target, destination, addMaterial);
    currentSample++;
  }

  private void InitRenderTexture()
  {
    if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
    {
      // Release render texture if we already have one
      if (_target != null)
      {
        _target.Release();
      }

      // Get a render target for Ray Tracing
      _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      _target.enableRandomWrite = true;
      _target.Create();
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
    Vector3 l = _directionalLight.transform.forward;
    rayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, _directionalLight.intensity));
    rayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
    rayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
    rayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);
    rayTracingShader.SetInt("_MaxBounces", maxBounces);
    rayTracingShader.SetBool("_UseRayTracing", useRayTracing);
    rayTracingShader.SetBuffer(0, "_Spheres", sphereBuffer);
  }
}
