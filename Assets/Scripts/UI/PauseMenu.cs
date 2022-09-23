using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
  private PauseMenuManager pauseMenuManager;

  // Start is called before the first frame update
  void Start()
  {
    pauseMenuManager = FindObjectOfType<PauseMenuManager>();
  }

  // Update is called once per frame
  void Update()
  {

  }

  void OnEnable()
  {
    Cursor.lockState = CursorLockMode.None;
    Time.timeScale = 0;
  }

  void OnDisable()
  {
    Time.timeScale = 1;
  }

  public void Resume()
  {
    if (pauseMenuManager != null)
    {
      pauseMenuManager.TogglePauseMenu();
    }
  }

  public void Quit()
  {
    SceneManager.LoadScene("Menu");
  }
}
