using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpiderControl : MonoBehaviour
{
  [Header("Limbs")]
  [SerializeField] ProceduralLimb[] legs = new ProceduralLimb[4];
  [Tooltip("The starting position of the legs in local space of the limb")]
  [SerializeField] Vector3[] legStartingPosition = new Vector3[4];

  private Vector3[] legControls = new Vector3[4]; // World space
  private Vector3[] targetPoints = new Vector3[4]; // Local space

  private Rigidbody rb = null;

  void Start()
  {
    rb = GetComponent<Rigidbody>();
    for (int i = 0; i < legs.Length; i++)
    {
      legs[i].SetControlFromLimbSpace(legStartingPosition[i]);
      legControls[i] = FindGroundElevation(legs[i].controlPoint);
      targetPoints[i] = transform.InverseTransformPoint(legControls[i]);
      legs[i].controlPoint = legControls[i];
    }
  }

  void Update()
  {
    for (int i = 0; i < legs.Length; i++)
    {
      Vector3 target = FrameTarget(i);
      // legControls[i] = FindGroundElevation(legControls[i]);

      if (Vector3.Distance(legControls[i], target) > 1f)
      {
        legControls[i] = target;
      }

      if (Vector3.Distance(legs[i].controlPoint, legControls[i]) > 0.1f)
      {
        // legs[i].controlPoint = legControls[i];
      }
    }

    Vector3 planeVector1 = legs[0].controlPoint - legs[3].controlPoint;
    Vector3 planeVector2 = legs[1].controlPoint - legs[2].controlPoint;
    Vector3 normal = Vector3.Cross(planeVector1, planeVector2).normalized;
    transform.up = normal;

    Move();
  }

  void Move()
  {
    float horizontal = Input.GetAxis("Horizontal");
    float vertical = Input.GetAxis("Vertical");
    Vector3 direction = new Vector3(horizontal, 0, vertical);
    rb.MovePosition(transform.position + direction * 10f * Time.deltaTime);
  }

  Vector3 FindGroundElevation(Vector3 position)
  {
    RaycastHit hit;
    if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out hit, 100f, LayerMask.GetMask("Terrain")))
    {
      return hit.point;
    }
    return position;
  }

  // void MoveLeg(int index, Vector3 target)
  // {
  //   targetPoint[index] = transform.InverseTransformPoint(target);
  // }

  Vector3 FrameTarget(int i)
  {
    Vector3 target = FindGroundElevation(transform.TransformPoint(targetPoints[i]));
    return target;
  }

  void OnDrawGizmosSelected()
  {
    if (!Application.isPlaying)
    {
      return;
    }

    for (int i = 0; i < legs.Length; i++)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(legControls[i], 0.2f);
      Gizmos.color = Color.green;
      Gizmos.DrawSphere(FrameTarget(i), 0.2f);
    }
  }
}
