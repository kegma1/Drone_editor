using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControlls : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public GameObject pauseMenuPanel;
    public InputActionReference lookAction;

    private float xRotation = 0f;

    void OnEnable()
    {
        lookAction.action.Enable();
    }

    void OnDisable()
    {
        lookAction.action.Disable();
    }

    void Update()
    {
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            UnlockCursor();
            return;
        }

        LockCursor();

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector2 controllerLook = lookAction.action.ReadValue<Vector2>();

        float finalX = (mouseX + controllerLook.x) * mouseSensitivity * Time.deltaTime;
        float finalY = (mouseY + controllerLook.y) * mouseSensitivity * Time.deltaTime;

        xRotation -= finalY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * finalX);
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
