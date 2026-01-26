using UnityEngine;
using System.Collections;

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
    public float jumpForce = 20f;
    public float jumpCooldown;
    public float airMultiplier = 0.4f;
    public float extraGravity = 20f;

    bool readyToJump;

    [Header("Ground Check")]
    public LayerMask whatIsGround;
    public bool isGrounded = false;
    public float characterHeight = 1f; 
    RaycastHit[] raycastHits = new RaycastHit[10];

    [Header("Blast State")]
    public bool blastMode; // The "Ragdoll" state

    // Input
    Vector2 moveInputVector = Vector2.zero;


    // Helper components
    SyncPhysicsObject[] syncPhysicsObjects;
    Transform cameraTransform;

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
        if (moveDir != Vector3.zero)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            mainJoint.targetRotation = Quaternion.Inverse(desiredRotation);
        }

        // --- FORCES ---
        if (blastMode) return; 

        if (isGrounded)
            rb.AddForce(moveDir * moveSpeed * Time.fixedDeltaTime, ForceMode.Force);
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

        float radius = characterHeight * 0.45f;
        float castDist = characterHeight * 0.6f;
        Vector3 origin = transform.position + Vector3.up * (characterHeight * 0.5f);

        int hits = Physics.SphereCastNonAlloc(origin, radius, Vector3.down, raycastHits, castDist, whatIsGround);
        
        for (int i = 0; i < hits; i++)
        {
            if (raycastHits[i].transform.root != transform) 
            {
                isGrounded = true;
                break;
            }
        }
    }

    void ApplyDrag()
    {
        if (isGrounded && !blastMode)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
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
}