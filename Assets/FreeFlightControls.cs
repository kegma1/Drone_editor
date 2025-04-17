using UnityEngine;

public class FlightControl : MonoBehaviour
{
    public float movementSpeed = 10f;
    public float boostMultiplier = 3f;
    public float climbSpeed = 5f; // Controls the up/down speed (climb)
    
    private float horizontalSpeed;
    private Vector3 moveDirection;

    void Update()
    {
        // Handle horizontal movement (WASD)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Handle vertical movement (E/Q for up/down)
        float moveY = 0f;
        if (Input.GetKey(KeyCode.E)) // move up
            moveY = 1f;
        if (Input.GetKey(KeyCode.Q)) // move down
            moveY = -1f;

        moveDirection = new Vector3(moveX, moveY, moveZ).normalized;

        // Speed multiplier for boost (when holding shift)
        horizontalSpeed = Input.GetKey(KeyCode.LeftShift) ? movementSpeed * boostMultiplier : movementSpeed;

        // Apply movement based on current speed and delta time
        transform.Translate(moveDirection * horizontalSpeed * Time.deltaTime, Space.Self);

        // Apply climb speed for vertical (up/down) movement
        transform.Translate(Vector3.up * climbSpeed * moveY * Time.deltaTime, Space.World);
    }
}
