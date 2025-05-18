using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using SimpleFileBrowser;

public class EditingControlls : MonoBehaviour
{
    // mus kontrollen som blir brukt i editoren
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
        // Muse pekeren burde være tilgjengelig i pausemenyen
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            UnlockCursor();
            return;
        }

        // Muse pekeren burde være tilgjengelig hvis brukeren er i en tekst boks
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null && selected.GetComponent<TMP_InputField>() != null)
        {
            UnlockCursor();
            return;
        }

        // Muse pekeren burde være tilgjengelig hvis fil utforskeren er åpen
        if (FileBrowser.IsOpen)
        {
            UnlockCursor();
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            LockCursor();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            UnlockCursor();
        }

        // Hvis høyre museknapp er nede kan brukeren rotere kamera
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
