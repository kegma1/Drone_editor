using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void NewShow()
    {
        SceneManager.LoadScene("EditDroneShow");
    }

    public void LoadShow()
    {
        SceneManager.LoadScene("previewDroneShow");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");  //må implementeres
    }
}
