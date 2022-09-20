using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Sphere
{
  public Vector3 position;
  public float radius;
  public Vector3 albedo;
  public Vector3 specular;
  public float smoothness;
  public Vector3 emission;
};

public class RayTracingSphere : MonoBehaviour
{
  private Material material;

  void Awake()
  {
    material = GetComponent<Renderer>().material;
  }

  public Sphere GetSphere()
  {
    Color albedo = material.GetColor("_Albedo");
    Color specular = material.GetColor("_Specular");
    Color emission = material.GetColor("_Emission");
    float smoothness = material.GetFloat("_Smoothness");

    return new Sphere
    {
      position = transform.position,
      radius = transform.localScale.x / 2f,
      albedo = new Vector3(albedo.r, albedo.g, albedo.b),
      specular = new Vector3(specular.r, specular.g, specular.b),
      smoothness = smoothness,
      emission = new Vector3(emission.r, emission.g, emission.b)
    };
  }

  public bool ShouldUpdate()
  {
    if (transform.hasChanged)
    {
      transform.hasChanged = false;
      return true;
    }
    return false;
  }
}
