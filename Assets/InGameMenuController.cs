using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject MainUi;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        bool isActive = pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(!isActive);
        if (MainUi)
            MainUi.SetActive(isActive);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("mainMenu");
    }
}
