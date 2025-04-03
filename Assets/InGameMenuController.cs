using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;

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
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("mainMenu");
    }
}
