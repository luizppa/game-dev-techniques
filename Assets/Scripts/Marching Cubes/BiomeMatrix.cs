using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class BiomeData
{
  public static Biome blankBiome = new Biome {
    name = "Blank",
    shallowColor = new Color(0f, 0f, 0f, 0f),
    deepColor = new Color(0f, 0f, 0f, 0f),
    startFogColor = new Color(0f, 0f, 0f, 0f),
    startFogDensity = 0f,
    endFogColor = new Color(0f, 0f, 0f, 0f),
    endFogDensity = 0f
  };

  public static Biome placeholderBiome = new Biome {
    name = "Stone Valley",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public static Biome deepSea = new Biome {
    name = "Deep Sea",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public static Biome caverns = new Biome {
    name = "Caverns",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public static Biome rockyMeadows = new Biome {
    name = "Rocky Meadows",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.06f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.15f
  };

  public static Biome serenityFields = new Biome {
    name = "Serenity Fields",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public static Biome canyons = new Biome {
    name = "Canyons",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public static Biome steamingValley = new Biome {
    name = "Steaming Valley",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public static Biome wasteland = new Biome {
    name = "Wasteland",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public static Biome bloomingHills = new Biome {
    name = "Blooming Hills",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public static Biome underwaterTundra = new Biome {
    name = "Underwater Tundra",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public static Biome tropicalIslands = new Biome {
    name = "Tropical Islands",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.05f
  };

  public static Biome deadLands = new Biome {
    name = "Dead Lands",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };
  
  public static Biome stoneValley = new Biome {
    name = "Stone Valley",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };
  
  public static Biome acidPlateau = new Biome {
    name = "Acid Plateau",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };
  
  public static Biome darkDeeps = new Biome {
    name = "Dark Deeps",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };
  
  public static Biome nowhere = new Biome {
    name = "Nowhere",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public Biome[] biomes = new Biome[]{
    rockyMeadows    , deepSea         , caverns         , serenityFields,
    canyons         , steamingValley  , wasteland       , bloomingHills,
    underwaterTundra, tropicalIslands , deadLands       , stoneValley,
    acidPlateau     , darkDeeps       , nowhere         , rockyMeadows
  };

  public Biome InterpolateBiomes(Biome a, Biome b, float t = 0.5f){
    return new Biome{
      name = "("+a.name+","+b.name+")",
      startFogColor = Color.Lerp(a.startFogColor, b.startFogColor, t),
      startFogDensity = Mathf.Lerp(a.startFogDensity, b.startFogDensity, 0.5f),
      endFogColor = Color.Lerp(a.endFogColor, b.endFogColor, t),
      endFogDensity = Mathf.Lerp(a.endFogDensity, b.endFogDensity, t),
      shallowColor = Color.Lerp(a.shallowColor, b.shallowColor, t),
      deepColor = Color.Lerp(a.deepColor, b.deepColor, t)
    };
  }

  public Biome AddBiomeFraction(Biome target, Biome source, float t = 0.5f){
    target.startFogColor += source.startFogColor * t;
    target.startFogDensity += source.startFogDensity * t;
    target.endFogColor += source.endFogColor * t;
    target.endFogDensity += source.endFogDensity * t;
    target.shallowColor += source.shallowColor * t;
    target.deepColor += source.deepColor * t;
    return target;
  }
  
  public Biome GetBiome(float[] features){
    int interpolations = Math.Min(features.Length, biomes.Length);
    // Debug.Log(features);
    Biome biome = blankBiome;
    for(int i = 0; i < interpolations; i++){
      biome = AddBiomeFraction(biome, biomes[i], features[i]);
    }
    return biome;
  }
}

