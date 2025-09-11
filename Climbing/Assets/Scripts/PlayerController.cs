using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;
    private PlayerControls controls;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        controls = new PlayerControls();

        // Link the Move action to our callback method
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 target = rb.position + move * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(target);

        //make player face the direction of movement
        if (move != Vector3.zero)
        {
            Quaternion newRot = Quaternion.LookRotation(move, Vector3.up);
            rb.MoveRotation(newRot);
        }
    }
}
