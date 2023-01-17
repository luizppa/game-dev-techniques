using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ProceduralLimb : MonoBehaviour
{
  // Position of _controlPoint is relative to body
  [SerializeField] Vector3 _controlPoint = Vector3.right;
  [SerializeField] bool usePole = false;
  [SerializeField] Vector3 _pole = Vector3.up + (Vector3.right / 2f);
  [SerializeField] int boneCount = 3;
  [SerializeField] public Transform body = null;

  [Header("Inverse Kinematics")]
  [SerializeField] int solverIterations = 1;
  [SerializeField] float solverTolerance = 0.01f;

  // Position of controlPoint is relative to world
  public Vector3 controlPoint
  {
    get
    {
      if (body == null)
      {
        return _controlPoint;
      }
      return body.TransformPoint(_controlPoint);
    }
    set
    {
      _controlPoint = body.InverseTransformPoint(value);
    }
  }

  public Vector3 pole
  {
    get
    {
      if (body == null)
      {
        return _pole;
      }
      return body.TransformPoint(_pole);
    }
    set
    {
      _pole = body.InverseTransformPoint(value);
    }
  }

  private Transform[] bones;
  private Vector3[] bonePositions;
  private float[] boneLengths;
  private float limbLength = 0f;

  // private Vector3[] startDirectionSuccessive;
  // private Quaternion[] startRotationBone;
  // private Quaternion startRotationTarget;
  // private Quaternion startRotationRoot;

  bool isControlReachable
  {
    get
    {
      return _controlPoint.sqrMagnitude < (limbLength * limbLength);
    }
  }


  void Awake()
  {
    Init();
  }

  void Update()
  {

  }

  void LateUpdate()
  {
    ResolveKinematics();
  }

  void Init()
  {
    bones = new Transform[boneCount + 1];
    bonePositions = new Vector3[boneCount + 1];
    boneLengths = new float[boneCount];
    // startDirectionSuccessive = new Vector3[boneCount + 1];
    // startRotationBone = new Quaternion[boneCount + 1];
    limbLength = 0f;

    Transform currentBone = transform;
    for (int i = boneCount; i >= 0 && currentBone != null; i--, currentBone = currentBone.parent)
    {
      bones[i] = currentBone;
      if (i < boneCount)
      {
        boneLengths[i] = Vector3.Distance(bones[i + 1].position, bones[i].position);
        limbLength += boneLengths[i];
      }
    }

    if (bones[0] != body)
    {
      Debug.LogWarning("Warning: Body is not a parent of the last bone");
    }
  }

  void ResolveKinematics()
  {
    if (boneLengths.Length != boneCount)
    {
      Init();
    }

    CopyPositions();
    if (isControlReachable)
    {
      InverseKinematics();
    }
    else
    {
      Vector3 direction = _controlPoint.normalized;
      for (int i = 1; i <= boneCount; i++)
      {
        bonePositions[i] = bonePositions[i - 1] + direction * boneLengths[i - 1];
      }
    }
    SetPositions();
    RotateSegments();
  }

  void InverseKinematics()
  {
    for (int i = 0; i < solverIterations; i++)
    {
      BackwardKinematics();
      ForwardKinematics();
      if ((_controlPoint - bonePositions[boneCount]).sqrMagnitude < solverTolerance * solverTolerance)
      {
        break;
      }
    }

    if (usePole)
    {
      PoleVector();
    }
  }

  void ForwardKinematics()
  {
    for (int i = 0; i < boneCount; i++)
    {
      Vector3 direction = (bonePositions[i + 1] - bonePositions[i]).normalized;
      Vector3 target = bonePositions[i] + (direction * boneLengths[i]);

      bonePositions[i + 1] = target;
    }
  }

  void BackwardKinematics()
  {
    Vector3 target = controlPoint;
    for (int i = boneCount; i > 0; i--)
    {
      bonePositions[i] = target;
      Vector3 direction = (bonePositions[i - 1] - bonePositions[i]).normalized;
      target = bonePositions[i] + (direction * boneLengths[i - 1]);
    }
  }

  void PoleVector()
  {
    for (int i = 1; i < boneCount; i++)
    {
      Plane plane = new Plane(bonePositions[i + 1] - bonePositions[i - 1], bonePositions[i - 1]);
      Vector3 projectedPole = plane.ClosestPointOnPlane(pole);
      Vector3 projectedBone = plane.ClosestPointOnPlane(bonePositions[i]);

      if (Vector3.Distance(projectedPole, bonePositions[i - 1]) < 0.1f || Vector3.Distance(projectedBone, bonePositions[i - 1]) < 0.1f)
      {
        continue;
      }

      float angle = Vector3.SignedAngle(projectedBone - bonePositions[i - 1], projectedPole - bonePositions[i - 1], plane.normal);
      bonePositions[i] = Quaternion.AngleAxis(angle, plane.normal) * (bonePositions[i] - bonePositions[i - 1]) + bonePositions[i - 1];
    }
  }

  void CopyPositions()
  {
    for (int i = 0; i <= boneCount; i++)
    {
      bonePositions[i] = bones[i].position;
    }
  }

  void SetPositions()
  {
    for (int i = 0; i <= boneCount; i++)
    {
      bones[i].position = bonePositions[i];
    }
  }

  void RotateSegments()
  {
    for (int i = 1; i <= boneCount; i++)
    {
      bones[i].right = (bonePositions[i - 1] - bonePositions[i]).normalized;
    }
  }

  void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawSphere(controlPoint, 0.05f);
    Gizmos.color = Color.blue;
    Gizmos.DrawSphere(pole, 0.05f);
    Handles.color = Color.red;

    Transform current = transform;
    for (int i = 0; i < boneCount && current != null && current.parent != null; i++, current = current.parent)
    {
      float scale = Vector3.Distance(current.position, current.parent.position) * .1f;
      Matrix4x4 transformMatrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.position, current.parent.position), scale));
      Handles.matrix = transformMatrix;

      Handles.DrawWireCube(Vector3.up * .5f, Vector3.one);
    }

    Handles.matrix = Matrix4x4.identity;
    Gizmos.color = Color.red;
    for (int i = 0; i <= boneCount; i++)
    {
      Handles.Label(bones[i].position, "Bone " + i.ToString() + ": " + bones[i].name);
      Gizmos.DrawCube(bonePositions[i], Vector3.one * .1f);
      if (i < boneCount)
      {
        Gizmos.DrawLine(bonePositions[i], bonePositions[i + 1]);
      }
    }
  }
}
