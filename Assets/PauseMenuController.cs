using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{

    public GameObject pausePanel;
    public Button mainMenuButton;
    public Button quitButton;

    bool isOpen;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SetPausePanelVisibility(!isOpen);
        }
    }

    public void SetPausePanelVisibility(bool value)
    {
        if(value)
        {
            Cursor.lockState = CursorLockMode.None;
            pausePanel.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            pausePanel.SetActive(false);
        }
    }

    public void MainMenu()
    {
        if(!ReplayManager.Instance.isWatchingReplay)
        {
            ReplayManager.Instance.SaveReplayToFile();
        }
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
