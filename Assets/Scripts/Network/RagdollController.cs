using UnityEngine;
using Unity.Cinemachine;
using System.Linq;

public class RagdollController : MonoBehaviour
{
    // Singleton for easy access (Optional)
    public static RagdollController Local { get; private set; }

    [Header("Physics")]
    [SerializeField] Rigidbody rigidbody3D;
    [SerializeField] ConfigurableJoint mainJoint;

    [Header("Animation")]
    [SerializeField] Animator animator;

    // Input
    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;

    // Settings
    float maxSpeed = 3;
    bool isGrounded = false;
    RaycastHit[] raycastHits = new RaycastHit[10];

    // Helper components
    SyncPhysicsObject[] syncPhysicsObjects;
    CinemachineCamera cinemachineCamera;
    CinemachineBrain cinemachineBrain;

    void Awake()
    {
        Local = this;
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
        
        // Setup Camera automatically on Start
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineBrain = FindFirstObjectByType<CinemachineBrain>();

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = transform;
            cinemachineCamera.LookAt = transform;
            
            // Revert to standard Update if it was set to Manual by the old script
            if (cinemachineBrain != null)
                cinemachineBrain.UpdateMethod = CinemachineBrain.UpdateMethods.SmartUpdate;
        }
    }

    void Update()
    {
        // 1. Collect Input directly
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
            isJumpButtonPressed = true;
    }

    void FixedUpdate()
    {
        // 2. Ground Check
        isGrounded = false;
        int numberOfHits = Physics.SphereCastNonAlloc(rigidbody3D.position, 0.1f, transform.up * -1, raycastHits, 0.5f);
        
        for (int i = 0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform) continue;
            isGrounded = true;
            break;
        }

        // Apply artificial gravity if in air
        if (!isGrounded)
            rigidbody3D.AddForce(Vector3.down * 10);

        // Calculate local velocity for Animator
        Vector3 localVelocifyVsForward = transform.forward * Vector3.Dot(transform.forward, rigidbody3D.linearVelocity);
        float localForwardVelocity = localVelocifyVsForward.magnitude;
        animator.SetFloat("movementSpeed", localForwardVelocity * 0.4f);

        // 3. Movement Logic (Formerly inside GetInput check)
        float inputMagnitude = moveInputVector.magnitude;

        if (inputMagnitude != 0)
        {
            Quaternion desiredDirection = Quaternion.LookRotation(
                new Vector3(moveInputVector.x, 0, moveInputVector.y * -1), // * -1 might be camera dependent
                transform.up
            );

            // Rotate the configurable joint to face direction
            mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Time.fixedDeltaTime * 300);

            if (localForwardVelocity < maxSpeed)
            {
                rigidbody3D.AddForce(transform.forward * inputMagnitude * 30);
            }
        }

        // 4. Jump Logic
        if (isGrounded && isJumpButtonPressed)
        {
            rigidbody3D.AddForce(Vector3.up * 20, ForceMode.Impulse);
            isJumpButtonPressed = false; // Reset immediately after use
        }

        // 5. Update Physics Limbs (Ragdoll Matching)
        // We no longer need to sync over network, just update the joints to match animation
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }

        // Teleport reset (Falling off map)
        if (transform.position.y < -10)
        {
            transform.position = Vector3.zero + Vector3.up * 2;
            rigidbody3D.linearVelocity = Vector3.zero;
        }
    }
}