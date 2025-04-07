using UnityEngine;

public class EditingMovement : MonoBehaviour
{
    public Transform CameraTransform;
    public GameObject pauseMenuPanel;
    public float MovementSpeed = 100;

    private bool isFocused = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            isFocused = true;
            return;
        } else {
            isFocused = false;
        }

        if(!isFocused) {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 moveDir = CameraTransform.forward * vertical + CameraTransform.right * horizontal;

            transform.position += moveDir * MovementSpeed * Time.deltaTime;
        }
    }
}
