using UnityEngine;

public class PlayPauseManager : MonoBehaviour
{
    public GameObject playIcon;
    public GameObject pauseIcon;

    private bool _isPaused;
    public bool isPaused {
        get => _isPaused;
        // endrer hvilket ikon som er synelig
        set
        {
            _isPaused = value;
            if (_isPaused)
            {
                playIcon.SetActive(true);
                pauseIcon.SetActive(false);
            }
            else
            {
                playIcon.SetActive(false);
                pauseIcon.SetActive(true);
            }
        }
    }
    
    void Start()
    {
        playIcon.SetActive(true);
        pauseIcon.SetActive(false);
    }
}
