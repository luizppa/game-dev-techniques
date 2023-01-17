using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralLimb)), CanEditMultipleObjects]
class ProceduralLimbEditor : Editor
{
  private bool editingControl = false;

  private void OnLostFocus()
  {
    editingControl = false;
    Tools.hidden = false;
  }

  public override void OnInspectorGUI()
  {
    ProceduralLimb limb = (ProceduralLimb)target;

    base.OnInspectorGUI();

    if (GUILayout.Button(editingControl ? "Finish Edition" : "Edit in Scene"))
    {
      editingControl = !editingControl;
    }
  }

  protected virtual void OnSceneGUI()
  {
    ProceduralLimb segment = (ProceduralLimb)target;

    if (editingControl)
    {
      Tools.hidden = true;
      EditorGUI.BeginChangeCheck();
      Vector3 newControlPosition = Handles.PositionHandle(segment.controlPoint, Quaternion.identity);

      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(segment, "Change Control Point");
        segment.controlPoint = newControlPosition;
      }
    }
    else
    {
      Tools.hidden = false;
    }
  }
}
