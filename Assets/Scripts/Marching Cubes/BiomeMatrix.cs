using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BiomeCode = System.Collections.Generic.List<int>;

using BiomeNode = System.Tuple<Biome, float>;
using CodeNode = System.Tuple<System.Collections.Generic.List<int>, float>;

class BinaryTree<T>{
  public Node<T> root;

  public BinaryTree(T rootData){
    root = new Node<T> { data = rootData };
  }

  public List<Node<T>> NodesAtLevel(int level){
    List<Node<T>> nodes = new List<Node<T>>();
    Queue<Node<T>> queue = new Queue<Node<T>>();
    queue.Enqueue(root);
    int currentLevel = 0;
    while(queue.Count > 0){
      Node<T> node = queue.Dequeue();
      if(currentLevel == level){
        nodes.Add(node);
      }
      else{
        if(node.left != null){
          queue.Enqueue(node.left);
        }
        if(node.right != null){
          queue.Enqueue(node.right);
        }
      }
    }
    return nodes;
  }
}

class Node<T>
{
  public Node<T> parent { get; set; } = null;
  public Node<T> left { get; set; } = null;
  public Node<T> right { get; set; } = null;
  public T data { get; set; }
}

[System.Serializable]
class BiomeData
{
  
  private const float HIGH_FEATURE_LEVEL = 0.54f;
  private const float LOW_FEATURE_LEVEL = 0.46f;

  private static Biome placeholderBiome = new Biome {
    name = "Stone Valley",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  private static Biome deepSea = new Biome {
    name = "Deep Sea",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  private static Biome caverns = new Biome {
    name = "Caverns",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  private static Biome rockyMeadows = new Biome {
    name = "Rocky Meadows",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.06f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.15f
  };

  private static Biome serenityFields = new Biome {
    name = "Serenity Fields",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  private static Biome canyons = new Biome {
    name = "Canyons",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  private static Biome steamingValley = new Biome {
    name = "Steaming Valley",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  private static Biome wasteland = new Biome {
    name = "Wasteland",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  private static Biome bloomingHills = new Biome {
    name = "Blooming Hills",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  private static Biome underwaterTundra = new Biome {
    name = "Underwater Tundra",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  private static Biome tropicalIslands = new Biome {
    name = "Tropical Islands",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.05f
  };

  private static Biome deadLands = new Biome {
    name = "Dead Lands",
    shallowColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    deepColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    startFogColor = new Color(0.168f, 0.325f, 0.952f, 1f),
    startFogDensity = 0.03f,
    endFogColor = new Color(0.015f, 0.063f, 0.254f, 1f),
    endFogDensity = 0.03f
  };

  public Biome[] matrix = new Biome[]{
    deepSea         , placeholderBiome, placeholderBiome, caverns,
    placeholderBiome, rockyMeadows    , serenityFields  , canyons,
    placeholderBiome, placeholderBiome, steamingValley  , wasteland,
    bloomingHills   , underwaterTundra, tropicalIslands , deadLands
  };

  private List<Tuple<BiomeCode, float>> GetBiomeCodes(float[] features){
    List<Tuple<BiomeCode, float>> codes = new List<Tuple<BiomeCode, float>>
    {
      new Tuple<BiomeCode, float>(new BiomeCode(), 0f)
    };
    
    for(int i = 0; i < features.Length; i++){
      if(features[i] < 0.46f){
        foreach(Tuple<BiomeCode, float> code in codes){
          code.Item1.Add(0);
        }
      }
      else if(features[i] > 0.54f){
        foreach(Tuple<BiomeCode, float> code in codes){
          code.Item1.Add(1);
        }
      }
      else{
        float interp = Mathf.InverseLerp(0.46f, 0.54f, features[i]);
        List<Tuple<BiomeCode, float>> newCodes = new List<Tuple<BiomeCode, float>>();
        foreach(Tuple<BiomeCode, float> code in codes){
          newCodes.Add(new Tuple<BiomeCode, float>(new BiomeCode(code.Item1), interp));
          code.Item1.Add(0);
        }
        foreach(Tuple<BiomeCode, float> code in codes){
          newCodes.Add(new Tuple<BiomeCode, float>(new BiomeCode(code.Item1), 1f-interp));
          code.Item1.Add(1);
        }
        codes.AddRange(newCodes);
      }
    }

    return codes;
  }
  private Biome GetByCode(BiomeCode code){
    int index = 0;
    for(int i = 0; i < code.Count; i++){
      index += code[i] * (int)Mathf.Pow(2, i);
    }
    return matrix[index];
  }

  private List<Tuple<Biome, float>> GetBiomes(float[] features){
    List<Tuple<BiomeCode, float>> codes = GetBiomeCodes(features);
    List<Tuple<Biome, float>> biomes = new List<Tuple<Biome, float>>();
    foreach(Tuple<BiomeCode, float> code in codes){
      Biome biome = GetByCode(code.Item1);
      biomes.Add(new Tuple<Biome, float>(biome, code.Item2));
    }
    return biomes;
  }

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

  private void FillCodeTree(ref BinaryTree<CodeNode> tree, float[] features, int depth = 0){
    if(depth == features.Length){
      return;
    }

    List<Node<CodeNode>> nodes = tree.NodesAtLevel(depth);

    if(features[depth] < LOW_FEATURE_LEVEL){
      // add node to the left with code = [...parent.code, 0] and t = parent.t
      foreach(Node<CodeNode> node in nodes){
        BiomeCode newCode = new BiomeCode(node.data.Item1);
        newCode.Add(0); 
        node.left = new Node<CodeNode>{
          parent = node,
          data = new CodeNode(newCode, node.data.Item2)
        };
      }
    }
    else if(features[depth] > HIGH_FEATURE_LEVEL){
      // add node to the right with code = [...parent.code, 1] and t = parent.t
      foreach(Node<CodeNode> node in nodes){
        BiomeCode newCode = new BiomeCode(node.data.Item1);
        newCode.Add(1); 
        node.right = new Node<CodeNode>{
          parent = node,
          data = new CodeNode(newCode, node.data.Item2)
        };
      }
    }
    else{
      // calculate t with the inverse lerp of LOW_FEATURE_LEVEL and HIGH_FEATURE_LEVEL
      // add node to the left with code = [...parent.code, 0] and t = parent.t
      // add node to the right with code = [...parent.code, 1] and t = t
      float t = Mathf.InverseLerp(LOW_FEATURE_LEVEL, HIGH_FEATURE_LEVEL, features[depth]);
      foreach(Node<CodeNode> node in nodes){
        BiomeCode newCodeLeft = new BiomeCode(node.data.Item1);
        newCodeLeft.Add(0); 
        node.left = new Node<CodeNode>{
          parent = node,
          data = new CodeNode(newCodeLeft, node.data.Item2)
        };
        BiomeCode newCodeRight = new BiomeCode(node.data.Item1);
        newCodeRight.Add(1);
        node.right = new Node<CodeNode>{
          parent = node,
          data = new CodeNode(newCodeRight, t)
        };
      }
    }

    FillCodeTree(ref tree, features, depth+1);
  }

  private BiomeNode BiomeForNode(Node<CodeNode> node){
    if(node.left == null && node.right == null){
      Biome biome = GetByCode(node.data.Item1);
      return new BiomeNode(biome, node.data.Item2);
    }
    if(node.left == null){
      return BiomeForNode(node.right);
    }
    if(node.right == null){
      return BiomeForNode(node.left);
    }
    else{
      BiomeNode biomeLeft = BiomeForNode(node.left);
      BiomeNode biomeRight = BiomeForNode(node.right);

      Biome biome = InterpolateBiomes(biomeLeft.Item1, biomeRight.Item1, biomeRight.Item2);
      return new BiomeNode(biome, biomeLeft.Item2);
    }
  }
  
  public Biome GetBiome(float[] features){
    CodeNode rootData = new CodeNode(new BiomeCode(), 1f);
    BinaryTree<CodeNode> tree = new BinaryTree<CodeNode>(rootData);

    FillCodeTree(ref tree, features);
    Biome biome = BiomeForNode(tree.root).Item1;
    return biome;
  }
}

