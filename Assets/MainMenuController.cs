using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void NewShow()
    {
        SceneManager.LoadScene("mainScene");
    }

    public void LoadShow()
    {
        Debug.Log("Load Show"); //må implementeres
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");  //må implementeres
    }
}
