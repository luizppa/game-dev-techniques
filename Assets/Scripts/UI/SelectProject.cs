using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectProject : MonoBehaviour
{
  public void Open(string project)
  {
    SceneManager.LoadScene(project);
  }
}
