using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{   
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;

    public Transform orientation;

    [Header("Blast State")]
    public bool blastMode; // Check this in Inspector to debug


    float horizontalInput;
    float verticalInput;
    public bool grounded;

    Vector3 moveDirection;

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
    }

    // Update is called once per frame
    void Update()
    {   
        PlayerInput();
        SpeedControl();

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        if (grounded)
            rb.linearDamping = groundDrag;
        else 
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();   
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // On ground - Normal movement
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // In air - ONLY allow control if NOT in blast mode
        else if (!grounded)
        {
            if (!blastMode) 
            {
                // Normal air control
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            }
            else
            {
                // BLAST MODE: Ragdoll-like flight. 
                // We add ZERO force here, so the player is at the mercy of the physics engine.
                // (Optional: You can add a tiny amount of influence if you want slight steering)
            }
        }
    }

    private void SpeedControl()
    {
        // If we were just blasted, DO NOT LIMIT SPEED. Let them fly!
        if (blastMode) return;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    public void ApplyBlastForce()
    {
        StopAllCoroutines(); // Safety: Cancel any old blasts if we get hit twice
        StartCoroutine(BlastRoutine());
    }

    private IEnumerator BlastRoutine()
    {
        blastMode = true;

        // CRITICAL STEP: The "Takeoff" Buffer
        // We wait 0.1 seconds to allow the explosion to physically lift us off the floor.
        // If we check for 'grounded' immediately, the script will think we haven't left the floor yet
        // and give control back instantly.
        yield return new WaitForSeconds(0.1f);

        // Wait until the player is in the air (Safety check)
        yield return new WaitUntil(() => !grounded);

        // Now wait until we hit the ground again
        yield return new WaitUntil(() => grounded);

        // Landing detected: Restore control
        blastMode = false;
    }
}
