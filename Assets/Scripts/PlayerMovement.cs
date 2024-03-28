using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private readonly ContactPoint[] tempContacts = new ContactPoint[5];

    [Header("Movement")]
    public float acceleration = 200;
    public float sprintAcceleration = 200;
    public float airAcceleration = 5;
    public float maxMovementSpeed = 5;
    public float maxSprintSpeed = 10;
    public float speedCap = 100;
    public float drag = 0.3f;

    [Header("Mouse")]
    public float mouseSensitivity = 1;

    [Header("Jump")]
    public float jumpCooldown = 0.25f;
    public float jumpHeight = 300;
    public float additionalJumpTime = 0.25f;
    public float additionalJumpHeight = 500;

    public bool IsGrounded { get; private set; }

    private Rigidbody rb;
    private Camera cam;

    private Vector3 movementInput;

    private bool isSpaceUp = false;
    private bool isSpaceDown = false;
    private float lastJumpTime = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.maxLinearVelocity = speedCap;

        cam = GetComponentInChildren<Camera>();
    }

    private void OnApplicationFocus(bool focus)
    {
        // Lock cursor if focused
        Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = focus;
    }

    private void Update()
    {
        // Process key up and key down events in update, since they may be lost
        // in FixedUpdate
        // These flags will be reset in FixedUpdate
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isSpaceDown = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isSpaceUp = true;
        }

        movementInput = new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetKey(KeyCode.LeftShift) ? 1 : 0,
            Input.GetAxis("Vertical")
        );

        // Do this in update to make it more snappy
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        // Invert mouse y or it will be inverted
        float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player transform side to side
        transform.localRotation *= Quaternion.Euler(0, mouseX, 0);

        // Rotate camera up and down or the player "model" will start floating
        Quaternion camRotation = cam.transform.localRotation * Quaternion.Euler(mouseY, 0, 0);

        // Clamp up down look
        float upDownAngle = camRotation.eulerAngles.x;

        // Make angle in [-180, 180] space rather than [0, 360]
        if (upDownAngle > 180)
        {
            upDownAngle -= 360;
        }
        camRotation = Quaternion.Euler(Mathf.Clamp(upDownAngle, -75, 75), 0, 0);
        cam.transform.localRotation = camRotation;
    }

    // Physics should always be handled in fixed update
    private void FixedUpdate()
    {
        // Temp code to "respawn" player
        if (transform.position.y < -5)
        {
            transform.position = new Vector3(0, 2, 0);
        }

        Move();
        Jump();

        // Reset collision flags since OnCollision messages happen after fixed
        // update
        IsGrounded = false;
    }

    private void Move()
    {
        Vector2 vel = new(rb.velocity.x, rb.velocity.z);
        float rot = -transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        // Grab inputs
        // Generally grabbing inputs in FixedUpdates isn't good, but since we
        // aren't grabbing specific keyup or keydown events, we should be ok
        float strafeInput = movementInput.x;
        float forwardInput = movementInput.z;
        bool isSprinting = movementInput.y == 1;
        bool isMovePressed = Mathf.Abs(strafeInput) > 0.001 || Mathf.Abs(forwardInput) > 0.001;

        // Check which accel and max speed to use
        float accel;
        float maxSpeed;
        if (!IsGrounded)
        {
            accel = airAcceleration;
            maxSpeed = maxSprintSpeed;
        }
        else if (isSprinting && forwardInput > 0)
        {
            accel = sprintAcceleration;
            maxSpeed = maxSprintSpeed;
        }
        else
        {
            accel = acceleration;
            maxSpeed = maxMovementSpeed;
        }

        // Apply accel
        vel += Time.fixedDeltaTime * accel
            * new Vector2(
                // Had to look up how to rotate a 2d vector
                strafeInput * Mathf.Cos(rot) - forwardInput * Mathf.Sin(rot),
                strafeInput * Mathf.Sin(rot) + forwardInput * Mathf.Cos(rot)
            );

        // Cap move speed only when trying to move
        if (isMovePressed)
        {
            vel = vel.normalized * Mathf.Min(vel.magnitude, maxSpeed);
        }
        // Slow down if no keys are pressed
        else if (IsGrounded)
        {
            vel *= drag;
        }

        // Set actual velocity to temp vel var
        rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.y);
        Debug.Log(rb.velocity);
    }

    private void Jump()
    {
        // This is code repurposed and translated from one of my other projects
        // https://github.com/WSPTA-Cat-Game/CatGame/blob/master/Assets/Scripts/CharacterControl/CharacterMovement.cs#L204
        if (isSpaceDown)
        {
            if (IsGrounded && Time.time - lastJumpTime > jumpCooldown)
            {
                lastJumpTime = Time.time;

                // Add force rather than changing vel over multiple frames as it is
                // easier logically
                rb.AddForce(jumpHeight * rb.mass * Vector3.up);

                isSpaceUp = false;
            }

            isSpaceDown = false;
        }
        else if (!isSpaceUp && Time.time - lastJumpTime <= additionalJumpTime)
        {
            // Stop jump extension if let go of space
            rb.AddForce(additionalJumpHeight * Time.fixedDeltaTime * rb.mass * Vector3.up);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        int contactCount = collision.GetContacts(tempContacts);
        for (int i = 0; i < contactCount; i++) 
        {
            // If the contact normal is *mostly* facing up, then we're grounded
            if (Vector3.Angle(tempContacts[i].normal, Vector3.up) < 10)
            {
                IsGrounded = true;
                break;
            }
        }
    }
}
