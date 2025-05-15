using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class InGameMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject MainUi;
    public InputActionReference toggleMenuAction;
    public Transform playerTransform;

    void OnEnable()
    {
        toggleMenuAction.action.Enable();
        toggleMenuAction.action.performed += OnToggleMenu;
    }

    void OnDisable()
    {
        toggleMenuAction.action.performed -= OnToggleMenu;
        toggleMenuAction.action.Disable();
    }

    void Update()
    {
        if (FileBrowser.IsOpen) {
            toggleMenuAction.action.Disable();
        } else {
            toggleMenuAction.action.Enable();
        }
    }

    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        ToggleMenu();
    }

    public void OnGoHome()
    {
        playerTransform.position = new(0f, 1.8f, 0f);
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

    public void PickDiffrentShow()
    {
        var currenSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currenSceneName);
    }
}
