using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidUnit : MonoBehaviour
{
  [SerializeField] float speed = 0.3f;
  [SerializeField] float deviationMargin = 1f;
  [SerializeField][Range(0f, 1f)] float neighbourInfuence = 0.2f;
  [SerializeField] float viewDistance = 1.0f;
  [SerializeField] float unitAwareness = 20f;
  [SerializeField] LayerMask layerMask;

  private int id = -1;
  private Boid manager = null;
  private Bounds bounds;
  private Vector3 direction = Vector3.forward;

  private const int baseFps = 60;

  void Update()
  {
    if(Time.timeScale == 0){
      return;
    }
    Move();
  }

  void Move()
  {
    direction = ApplyNeightbourInfluence();
    direction = ApplyDeviation();

    direction = AvoidBounds();
    direction = AvoidObstacles();

    transform.rotation = Quaternion.LookRotation(direction);
    transform.position += transform.forward * speed * Time.deltaTime * baseFps;
  }

  Vector3 AvoidBounds()
  {
    Vector3 futurePosition = transform.position + (direction * speed * Time.deltaTime * baseFps * viewDistance);
    if (!bounds.Contains(futurePosition))
    {
      Vector3 closestPoint = bounds.ClosestPoint(futurePosition);
      Vector3 safeDirection = closestPoint + Vector3.Reflect(direction, closestPoint - futurePosition);
      return (safeDirection - transform.position).normalized;
    }
    return direction;
  }

  Vector3 ApplyNeightbourInfluence()
  {
    return Vector3.Lerp(direction, manager.GetNeighboursDirection(id), neighbourInfuence).normalized;
  }

  Vector3 ApplyDeviation()
  {
    Vector3 deviation = new Vector3(GetDeviation(), GetDeviation(), GetDeviation());
    return (direction + (deviation * Time.deltaTime * baseFps)).normalized;
  }

  Vector3 AvoidObstacles()
  {
    Ray ray = new Ray(transform.position, direction);
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit, viewDistance, layerMask))
    {
      Vector3 safeDirection = hit.point + Vector3.Reflect(direction, hit.normal);
      return (safeDirection - transform.position).normalized;
    }
    return direction;
  }

  float GetDeviation()
  {
    return Random.Range(-deviationMargin, deviationMargin);
  }

  public float GetViewDistance()
  {
    return viewDistance;
  }

  public float GetUnitAwareness()
  {
    return unitAwareness;
  }

  public Vector3 GetDirection()
  {
    return direction;
  }

  public void SetBounds(Bounds bounds)
  {
    this.bounds = bounds;
  }

  public void SetId(int id)
  {
    this.id = id;
  }

  public void SetManager(Boid manager)
  {
    this.manager = manager;
  }

  void OnDrawGizmosSelected()
  {
    // Gizmos.color = Color.white;
    // Gizmos.DrawRay(transform.position, direction * viewDistance);
    // Gizmos.DrawWireSphere(transform.position, unitAwareness);
  }
}
