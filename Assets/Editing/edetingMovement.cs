using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using SimpleFileBrowser;

public class EditingMovement : MonoBehaviour
{
    public Transform CameraTransform;
    public GameObject pauseMenuPanel;
    public float MovementSpeed = 100;

    private bool isFocused = false;

    void Update()
    {
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            isFocused = true;
            return;
        } else {
            isFocused = false;
        }

        if (FileBrowser.IsOpen) {
            isFocused = true;
        }

        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null && selected.GetComponent<TMP_InputField>() != null)
        {
            isFocused = true;
        }


        if(!isFocused) {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 moveDir = CameraTransform.forward * vertical + CameraTransform.right * horizontal;

            transform.position += moveDir * MovementSpeed * Time.deltaTime;
        }
    }
}
