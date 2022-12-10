using UnityEngine;

public class PauseMenuManager : SingletonMonoBehaviour<PauseMenuManager>
{
  [SerializeField] GameObject pauseMenuCanvas;
  private GameObject pauseMenuInstance;

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
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
