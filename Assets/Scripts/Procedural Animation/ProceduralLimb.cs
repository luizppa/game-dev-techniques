using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ProceduralLimb : MonoBehaviour
{
  // Position of _controlPoint is relative to body
  [SerializeField] Vector3 _controlPoint = Vector3.right;
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
      else
      {
        return body.TransformPoint(_controlPoint);
      }
    }
    set
    {
      _controlPoint = body.InverseTransformPoint(value);
    }
  }

  private Transform[] bones;
  private Vector3[] bonePositions;
  private float[] boneLengths;
  private float limbLength = 0f;

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

    // Debug.Log("Control is reachable: " + isControlReachable);
    // Debug.Log("Bone 0 position: " + bones[boneCount].position);
    // Debug.Log("Body position: " + body.position);

    // Do stuff
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
    for (int i = 0; i < boneCount; i++)
    {
      Gizmos.DrawLine(bones[i].position, bones[i + 1].position);
    }

    for (int i = 0; i <= boneCount; i++)
    {
      Handles.Label(bones[i].position, "Bone " + i.ToString() + ": " + bones[i].name);
      Gizmos.DrawCube(bones[i].position, Vector3.one * .1f);
    }

    Handles.color = Color.blue;
    Gizmos.color = Color.blue;
    for (int i = 0; i < boneCount; i++)
    {
      Gizmos.DrawLine(bonePositions[i], bonePositions[i + 1]);
    }

    for (int i = 0; i <= boneCount; i++)
    {
      Gizmos.DrawCube(bonePositions[i], Vector3.one * .1f);
    }
  }
}
