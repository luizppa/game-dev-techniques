using UnityEngine;

public class Boid : MonoBehaviour
{
  [SerializeField] Bounds navigationBounds;
  [SerializeField] Bounds spawnBounds;
  [SerializeField] uint units;
  [SerializeField] GameObject unitPrefab;

  private BoidUnit[] boidUnits;
  private float[,] adjacencyMatrix;
  private Vector3[] neighbourDirections;

  void Start()
  {
    SpawnUnits();
  }

  void Update()
  {
    CalculateDistances();
  }

  void SpawnUnits()
  {
    boidUnits = new BoidUnit[units];
    adjacencyMatrix = new float[units, units];
    neighbourDirections = new Vector3[units];

    for (int i = 0; i < units; i++)
    {
      Vector3 position = new Vector3(
        Random.Range(spawnBounds.min.x, spawnBounds.max.x),
        Random.Range(spawnBounds.min.y, spawnBounds.max.y),
        Random.Range(spawnBounds.min.z, spawnBounds.max.z)
      );

      GameObject unit = Instantiate(unitPrefab, position, Quaternion.identity, gameObject.transform);
      BoidUnit unitComponent;
      if (!unit.TryGetComponent(out unitComponent))
      {
        unitComponent = unit.AddComponent<BoidUnit>();
      }

      boidUnits[i] = unitComponent;

      unitComponent.SetBounds(navigationBounds);
      unitComponent.SetId(i);
      unitComponent.SetManager(this);
    }

    CalculateDistances();
  }

  void CalculateDistances()
  {
    for (int i = 0; i < units; i++)
    {
      int neighbourCount = 0;
      Vector3 neighbourDirection = Vector3.zero;
      for (int j = 0; j < units; j++)
      {
        if (i > j)
        {
          float distance = Vector3.Distance(boidUnits[i].transform.position, boidUnits[j].transform.position);
          adjacencyMatrix[i, j] = distance;
          adjacencyMatrix[j, i] = distance;
        }
        if (i != j)
        {
          if (adjacencyMatrix[i, j] <= boidUnits[i].GetUnitAwareness())
          {
            neighbourCount++;
            neighbourDirection += boidUnits[j].GetDirection();
          }
        }
      }
      neighbourDirections[i] = (neighbourDirection / neighbourCount).normalized;
    }
  }

  public Vector3 GetNeighboursDirection(int id)
  {
    return neighbourDirections[id];
  }

  public void SetUnitPrefab(GameObject prefab)
  {
    unitPrefab = prefab;
  }

  public void SetUnits(uint units)
  {
    this.units = units;
  }

  public void SetNavigationBounds(Bounds bounds)
  {
    navigationBounds = bounds;
  }

  public void SetSpawnBounds(Bounds bounds)
  {
    spawnBounds = bounds;
  }

  void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.white;
    Gizmos.DrawWireCube(navigationBounds.center, navigationBounds.size);
    Gizmos.color = Color.green;
    Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);

    if (!Application.isPlaying)
    {
      return;
    }

    for (int i = 0; i < units; i++)
    {
      for (int j = i + 1; j < units; j++)
      {
        if (adjacencyMatrix[i, j] <= boidUnits[i].GetUnitAwareness())
        {
          Gizmos.color = Color.blue;
          Gizmos.DrawLine(boidUnits[i].transform.position, boidUnits[j].transform.position);
        }
      }
    }
  }
}
