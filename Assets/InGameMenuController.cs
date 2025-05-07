using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class InGameMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject MainUi;
    public InputActionReference toggleMenuAction;

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

    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        ToggleMenu();
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
