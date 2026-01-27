using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class RagdollController : MonoBehaviour
{
    // Singleton for easy access (Optional)
    public static RagdollController Local { get; private set; }

    [Header("References")]

    [SerializeField] Rigidbody rb;
    [SerializeField] ConfigurableJoint mainJoint;
    [SerializeField] Animator animator;

    [Header("Movement Stats")]
    public float moveSpeed = 4500f; // Higher values needed for Ragdolls compared to standard players
    public float currentMaxSpeed = 10f; 
    public float groundDrag = 5f;
    public float stoppingDrag = 20f; 
    public float jumpForce = 20f;
    public float jumpCooldown;
    public float airMultiplier = 0.4f;
    public float extraGravity = 20f;

    bool readyToJump;

    [Header("Ground Check")]
    public LayerMask whatIsGround;
    public bool isGrounded = false;
    public float characterHeight = 1f; 

    [Header("Blast State")]
    public bool blastMode; // The "Ragdoll" state

    // Input
    Vector2 moveInputVector = Vector2.zero;

    // Helper components
    SyncPhysicsObject[] syncPhysicsObjects;
    Transform cameraTransform;
    private float externalLaunchTimer = 0f;
    public bool IsAiming { get; set; }

    void Awake()
    {
        Local = this;
        rb = GetComponent<Rigidbody>();

        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();

        if(mainJoint != null) mainJoint.targetRotation = Quaternion.identity;

        readyToJump = true;

        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
        else
        {
            // Fallback if Camera.main is null (e.g., missing tag)
            Camera foundCam = FindFirstObjectByType<Camera>();
            if (foundCam) cameraTransform = foundCam.transform;
            else Debug.LogError("[RagdollController] No Camera found in scene!");
        }
    }

    void Update()
    {
        // 1. Collect Input directly
        PlayerInput();

        // 2. Speed Control & Drag
        SpeedControl();
    }

    void FixedUpdate()
    {      
        if (externalLaunchTimer > 0) externalLaunchTimer -= Time.fixedDeltaTime;

        CheckGround();

        ApplyDrag();

        if (!isGrounded) 
            rb.AddForce(Vector3.down * extraGravity);
        
        MovePlayer();
        UpdateAnimator();
        UpdateLimbs();
        CheckRespawn();
        
    }

    void PlayerInput()
    {
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void MovePlayer()
    {   
        // Orientate and Move 
        // 1. Get Camera Vectors
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // 2. Flatten them (Ignore looking up/down so we don't fly into the ground)
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 3. Calculate Move Direction based on Input + Camera Angle
        Vector3 moveDir = camForward * moveInputVector.y + camRight * moveInputVector.x;
        moveDir.Normalize();

        // --- ROTATION ---
        Quaternion desiredRotation = Quaternion.identity;

        if (IsAiming)
        {
            // AIMING MODE
            if (camForward != Vector3.zero) 
                desiredRotation = Quaternion.LookRotation(camForward, Vector3.up);
        }
        else
        {      
            // FREE MOVE MODE
            if (moveDir != Vector3.zero) 
                desiredRotation = Quaternion.LookRotation(moveDir, Vector3.up);
        }

        if (desiredRotation != Quaternion.identity)
            mainJoint.targetRotation = Quaternion.Inverse(desiredRotation);

        // --- FORCES ---
        if (blastMode) return; 

        if (isGrounded){
            float startUpMultiplier = (rb.linearVelocity.magnitude < 2f) ? 2f : 1f;
            rb.AddForce(moveDir * moveSpeed * startUpMultiplier * Time.fixedDeltaTime, ForceMode.Force);
        }
        else
            rb.AddForce(moveDir * moveSpeed * airMultiplier * Time.fixedDeltaTime, ForceMode.Force);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    void CheckGround()
    {   
        isGrounded = false;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float distance = characterHeight + 0.2f;

        RaycastHit hit;
        
        if (Physics.Raycast(origin, Vector3.down, out hit, distance, whatIsGround))
            isGrounded = true;
    }

    void ApplyDrag()
    {   
        // 1. Trampoline / Launch Override
        // If we were just launched by a trampoline, cut all drag so we fly up.
        if (externalLaunchTimer > 0)
        {
            rb.linearDamping = 0f;
            return;
        }

        // 2. Air / Ragdoll Override
        // If we are falling or dead, we shouldn't have "ground friction".
        if (!isGrounded || blastMode)
        {
            rb.linearDamping = 0f;
            return;
        }

        // 3. Ground Movement Logic
        // Check if we are trying to move
        bool isInputActive = moveInputVector.magnitude > 0.1f;

        if (isInputActive)
        {
            // We are running -> Low Drag (allows speed)
            rb.linearDamping = groundDrag; 
        }
        else
        {
            // We released controls -> High Drag (brakes instantly)
            rb.linearDamping = stoppingDrag; 
        }
    }

    void SpeedControl()
    {
        if (blastMode) return;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        
        if (flatVel.magnitude > currentMaxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMaxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    void UpdateAnimator()
    {
        float rawSpeed = rb.linearVelocity.magnitude; 
        animator.SetFloat("movementSpeed", rawSpeed * 0.5f);
    }

    void UpdateLimbs()
    {
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }
    }

    public void ApplyBlastForce()
    {
        StopAllCoroutines();
        StartCoroutine(BlastRoutine());
    }

    IEnumerator BlastRoutine()
    {
        blastMode = true;
        yield return new WaitForSeconds(0.1f);
        float timeOut = 0.5f; 
        while (isGrounded && timeOut > 0)
        {
            timeOut -= Time.deltaTime;
            yield return null;
        }
        if (!isGrounded)
        {
            yield return new WaitUntil(() => isGrounded);
        }
        blastMode = false;
    }

    void CheckRespawn()
    {
        if (transform.position.y < -20)
        {
            rb.linearVelocity = Vector3.zero;
            transform.position = Vector3.up * 5;
        }
    }

    public void ExternalLaunch(Vector3 forceVector)
    {
        // 1. Reset Vertical Velocity (Clean slate)
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        
        // 2. Add the Force
        rb.AddForce(forceVector, ForceMode.Impulse);

        // 3. Set the "Ignore Drag" timer for 0.2 seconds
        // This gives the physics engine enough time to lift the player 
        // off the ground before 'ApplyDrag' kicks in again.
        externalLaunchTimer = 0.2f; 
        
        // Optional: Force state to prevent other logic interfering
        isGrounded = false; 
    }
}