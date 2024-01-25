using UnityEngine;
using Unity.Mathematics;

public enum NOISE_TYPE
{
  PERLIN,
  CELLULAR
}

// TODO: A nice speed up would be to generate the noise maps in a GPU kernel
public static class NoiseUtils
{
  public static Texture2D GenerateNoiseMap(Vector2Int resolution, float scale, int seed, NOISE_TYPE noiseType = NOISE_TYPE.PERLIN)
  {
    Texture2D noiseMap = new Texture2D(resolution.x, resolution.y);
    for (int x = 0; x < resolution.x; x++)
    {
      for (int y = 0; y < resolution.y; y++)
      {
        Color color;
        switch(noiseType){
          case NOISE_TYPE.PERLIN:
            color = CalculatePerlinNoiseColor(x, y, resolution, scale, seed);
            break;
          case NOISE_TYPE.CELLULAR:
            color = GenerateCellularNoiseMap(x, y, resolution, scale, seed);
            break;
          default:
            color = CalculatePerlinNoiseColor(x, y, resolution, scale, seed);
            break;
        }
        noiseMap.SetPixel(x, y, color);
      }
    }
    noiseMap.Apply();
    return noiseMap;
  }

  public static Color GenerateCellularNoiseMap(int x, int y, Vector2Int resolution, float scale, int seed)
  {
      float2 noiseCoord = new float2((x + seed) * scale, (y + seed) * scale);
      float sample = noise.cellular2x2(noiseCoord).x;
      Debug.Log(sample);
      return new Color(sample, sample, sample, 1f);
  }

  private static Color CalculatePerlinNoiseColor(int x, int y, Vector2Int resolution, float scale, int seed)
  {
    float xCord = (float)x / resolution.x * scale;
    float yCord = (float)y / resolution.y * scale;

    float sample = Mathf.PerlinNoise(seed + xCord, seed + yCord);
    return new Color(sample, sample, sample);
  }
}
