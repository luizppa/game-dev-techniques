using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralLimb)), CanEditMultipleObjects]
class ProceduralLimbEditor : Editor
{
  private bool editingPivots = false;

  void OnDestroy()
  {
    Tools.hidden = false;
  }

  public override void OnInspectorGUI()
  {
    ProceduralLimb limb = (ProceduralLimb)target;

    base.OnInspectorGUI();

    if (GUILayout.Button(editingPivots ? "Finish Edition" : "Edit in Scene"))
    {
      editingPivots = !editingPivots;
    }
  }

  protected virtual void OnSceneGUI()
  {
    ProceduralLimb segment = (ProceduralLimb)target;

    if (editingPivots)
    {
      Tools.hidden = true;

      EditorGUI.BeginChangeCheck();
      Vector3 newControlPosition = Handles.PositionHandle(segment.controlPoint, Quaternion.identity);
      Vector3 newPolePosition = Handles.PositionHandle(segment.pole, Quaternion.identity);

      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(segment, "Change Kinematics Points");
        segment.controlPoint = newControlPosition;
        segment.pole = newPolePosition;
      }
    }
    else
    {
      Tools.hidden = false;
    }
  }
}
