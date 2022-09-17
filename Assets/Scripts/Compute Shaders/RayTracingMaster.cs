using System.Linq;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
  [SerializeField] ComputeShader rayTracingShader = null;
  [SerializeField] Texture2D skyboxTexture = null;
  [SerializeField] Color solidSkyboxColor = Color.black;

  private Camera _camera;
  private RenderTexture _target;

  void Awake()
  {
    _camera = GetComponent<Camera>();
  }

  void Start()
  {
    if (skyboxTexture == null)
    {
      skyboxTexture = new Texture2D(Screen.width, Screen.height);
      Color[] pixels = Enumerable.Repeat(solidSkyboxColor, Screen.width * Screen.height).ToArray();
      skyboxTexture.SetPixels(pixels);
      skyboxTexture.Apply();
    }
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
    Graphics.Blit(_target, destination);
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
    rayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
    rayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
    rayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);
  }
}
