using System.Reflection;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

// Based on this StackOverflow answer: https://stackoverflow.com/a/58446816

[CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
public class ShowIfAttributeDrawer : PropertyDrawer
{

  #region Reflection helpers.

  private static FieldInfo GetField(object target, string fieldName)
  {
    return GetAllFields(target, f => f.Name.Equals(fieldName,
          StringComparison.InvariantCulture)).FirstOrDefault();
  }

  private static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
  {
    List<Type> types = new List<Type>()
            {
                target.GetType()
            };

    while (types.Last().BaseType != null)
    {
      types.Add(types.Last().BaseType);
    }

    for (int i = types.Count - 1; i >= 0; i--)
    {
      IEnumerable<FieldInfo> fieldInfos = types[i]
          .GetFields(BindingFlags.Instance | BindingFlags.Static |
          BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
          .Where(predicate);

      foreach (var fieldInfo in fieldInfos)
      {
        yield return fieldInfo;
      }
    }
  }
  #endregion

  private bool MeetsConditions(SerializedProperty property)
  {
    var showIfAttribute = this.attribute as ShowIfAttribute;
    var target = property.serializedObject.targetObject;
    List<bool> conditionValues = new List<bool>();

    foreach (var condition in showIfAttribute.Conditions)
    {
      FieldInfo conditionField = GetField(target, condition);
      if (conditionField != null && conditionField.FieldType == typeof(bool))
      {
        conditionValues.Add((bool)conditionField.GetValue(target));
      }
    }

    if (conditionValues.Count > 0)
    {
      bool met;
      met = true;
      foreach (var value in conditionValues)
      {
        met = met && value;
      }
      return met;
    }
    else
    {
      return true;
    }
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    // Calcluate the property height, if we don't meet the condition and the draw mode is DontDraw, then height will be 0.
    bool meetsCondition = MeetsConditions(property);
    var showIfAttribute = this.attribute as ShowIfAttribute;

    return meetsCondition ? base.GetPropertyHeight(property, label) : 0;
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    bool meetsCondition = MeetsConditions(property);
    // Early out, if conditions met, draw and go.
    if (meetsCondition)
    {
      EditorGUI.PropertyField(position, property, label, true);
      return;
    }
    return;
  }
}
