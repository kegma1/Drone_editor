using UnityEngine;

public class EditingControlls : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public GameObject pauseMenuPanel;

    private float xRotation = 0f;
    private bool isAiming = false;

    void Start()
    {
        UnlockCursor();
    }

    void Update()
    {
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            UnlockCursor();
            return;
        }

        if (Input.GetMouseButtonDown(1)) {
            isAiming = true;
            LockCursor();
        } else if (Input.GetMouseButtonUp(1)) {
            isAiming = false;
            UnlockCursor();
        }


        if (isAiming) {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
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
