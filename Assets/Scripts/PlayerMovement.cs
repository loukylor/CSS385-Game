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
    public float maxAirSpeed = 3;
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

    private bool isSprint = false;
    private Vector2 movementInput;
    private Vector2 rotatedMovementInput;

    private bool isSpaceUp = false;
    private bool isSpaceDown = false;
    private float lastJumpTime = 0;
    private Vector2 lastJumpDir;
    private bool canDoubleJump = false;

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

        isSprint = Input.GetKey(KeyCode.LeftShift);
        movementInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
        rotatedMovementInput = movementInput.Rotate(-transform.rotation.eulerAngles.y);

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

        // Grab inputs
        // Generally grabbing inputs in FixedUpdates isn't good, but since we
        // aren't grabbing specific keyup or keydown events, we should be ok
        bool isMovePressed = movementInput.magnitude > 0.001;

        // Check which accel and max speed to use
        float accel;
        float maxSpeed;
        if (!IsGrounded)
        {
            // Lessen accel if trying to move in same direction as when jump
            // This is so you don't suddenly accelerate forward when you jump
            // from non sprinting speed
            accel = airAcceleration;
            accel *= Mathf.Clamp(Vector2.Angle(lastJumpDir, movementInput) / 180, 0.1f, 1);

            maxSpeed = maxAirSpeed;
        }
        else if (isSprint && movementInput.y > 0)
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
        vel += Time.fixedDeltaTime * accel * rotatedMovementInput;

        // Cap move speed only when trying to move, or, if not grounded, then 
        // only cap speed when vel is less than max speed + 1
        if (isMovePressed && (IsGrounded || vel.magnitude < maxSpeed + 1))
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
    }

    private void Jump()
    {
        // This is code repurposed and translated from one of my other projects
        // https://github.com/WSPTA-Cat-Game/CatGame/blob/master/Assets/Scripts/CharacterControl/CharacterMovement.cs#L204
        if (isSpaceDown)
        {
            if ((IsGrounded || canDoubleJump) && Time.time - lastJumpTime > jumpCooldown)
            {
                lastJumpDir = movementInput.magnitude < 0.001 ? Vector2.up : movementInput;
                lastJumpTime = Time.time;

                // Reset y velocity so it doesn't hinder the jump
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                
                // Add force rather than changing vel over multiple frames as it is
                // easier logically
                if (!IsGrounded)
                {
                    canDoubleJump = false;
                    // Rotate velocity to give more air control on double jump
                    Vector3 newVelDir = new(rotatedMovementInput.x, 0, rotatedMovementInput.y);
                    float amountRotated = 1 - Vector3.Angle(rb.velocity, newVelDir) / 180;
                    Vector3 newVel = Vector3.RotateTowards(rb.velocity, newVelDir, 10000, 0) * amountRotated;

                    // Make new vel have a minimum speed
                    if (newVel.magnitude < maxAirSpeed)
                    {
                        newVel = 1.5f * maxAirSpeed * newVelDir.normalized;
                    }
                    rb.velocity = newVel;
                    rb.AddForce(jumpHeight * 0.8f * rb.mass * Vector3.up);
                }
                else
                {
                    rb.AddForce(jumpHeight * rb.mass * Vector3.up);
                }

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
                canDoubleJump = true;
                break;
            }
        }
    }
}
