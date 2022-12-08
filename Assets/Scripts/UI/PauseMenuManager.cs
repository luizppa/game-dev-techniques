using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuManager : SingletonMonoBehaviour<PauseMenuManager>
{
  [SerializeField] GameObject pauseMenuCanvas;
  private GameObject pauseMenuInstance;

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7))
    {
      TogglePauseMenu();
    }
  }

  public void TogglePauseMenu()
  {
    if (pauseMenuInstance == null)
    {
      pauseMenuInstance = Instantiate(pauseMenuCanvas);
    }
    else
    {
      Destroy(pauseMenuInstance);
    }
  }
}
