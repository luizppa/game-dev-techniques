using System;
using UnityEngine;

// Based on this StackOverflow answer: https://stackoverflow.com/a/58446816

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ShowIfAttribute : PropertyAttribute
{
  public string[] Conditions { get; private set; }

  public ShowIfAttribute(params string[] conditions)
  {
    Conditions = conditions;
  }
}
