using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralSegment)), CanEditMultipleObjects]
class ProceduralSegmentEditor : Editor
{
  private bool editingPivot = false;

  public override void OnInspectorGUI()
  {
    ProceduralSegment segment = (ProceduralSegment)target;

    EditorGUI.BeginChangeCheck();
    Vector3 newControlPoint = EditorGUILayout.Vector3Field("Control", segment.controlPoint);

    if (GUILayout.Button(editingPivot ? "Finish Edition" : "Edit in Scene"))
    {
      editingPivot = !editingPivot;
    }

    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(segment, "Change Control Point");
      segment.controlPoint = newControlPoint;
    }

    if (GUILayout.Button("Reset"))
    {
      segment.controlPoint = Vector3.right;
    }

    EditorGUILayout.Space();
  }

  protected virtual void OnSceneGUI()
  {
    ProceduralSegment segment = (ProceduralSegment)target;

    if (editingPivot)
    {
      Tools.hidden = true;
      EditorGUI.BeginChangeCheck();
      Vector3 newControlPosition = Handles.PositionHandle(segment.transform.TransformPoint(segment.controlPoint), Quaternion.identity);

      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(segment, "Change Control Point");
        segment.controlPoint = segment.transform.InverseTransformPoint(newControlPosition);
      }
    }
    else
    {
      Tools.hidden = false;
    }
  }
}
