using UnityEngine;
using UnityEngine.InputSystem;

public class FlightControl : MonoBehaviour
{
    // movement brukt i preview modus
    public float movementSpeed = 10f;
    public float boostMultiplier = 3f;
    public float climbSpeed = 5f;

    public InputActionReference moveAction;
    public InputActionReference climbAction;
    public InputActionReference boostAction;
    public InputActionReference HomeAction;

    void OnEnable()
    {
        moveAction.action.Enable();
        climbAction.action.Enable();
        boostAction.action.Enable();
        HomeAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        climbAction.action.Disable();
        boostAction.action.Disable();
        HomeAction.action.Disable();
    }

    void Update()
    {
        Vector2 controllerMove = moveAction.action.ReadValue<Vector2>();
        float controllerClimb = climbAction.action.ReadValue<float>();
        bool isControllerBoosting = boostAction.action.IsPressed();

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float moveY = 0f;
        if (Input.GetKey(KeyCode.E)) moveY = 1f;
        if (Input.GetKey(KeyCode.Q)) moveY = -1f;

        bool isKeyboardBoosting = Input.GetKey(KeyCode.LeftShift);

        Vector3 combinedMove = new Vector3(
            moveX + controllerMove.x,
            moveY + controllerClimb,
            moveZ + controllerMove.y
        ).normalized;

        bool isBoosting = isKeyboardBoosting || isControllerBoosting;
        float speed = isBoosting ? movementSpeed * boostMultiplier : movementSpeed;

        transform.Translate(combinedMove * speed * Time.deltaTime, Space.Self);

        if (HomeAction.action.IsPressed()) {
            transform.position = new(0, 1.8f, 0);
        }

    }
}
