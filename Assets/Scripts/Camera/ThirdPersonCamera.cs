using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;

public class ThirdPersonCamera : MonoBehaviour
{   
    [Header("Cinemachine References")]
    public CinemachineCamera explorationCam; // Drag the "Zoomed Out" virtual cam here
    public CinemachineCamera focusCam;       // Drag the "Over Shoulder" virtual cam here
    public Image crosshairUI;               // Drag your Crosshair Image

    [Header("Player References")]
    public RagdollController playerController;
    public Rigidbody playerRb;
    public Transform playerObj;   // The visual mesh inside the player
    public Transform playerCam;   // Drag your MAIN CAMERA here

    [Header("Effects")]
    public CinemachineImpulseSource impulseSource; // Drag Player here (with Impulse Source component)
    public float shakeStrength = 0.5f;

    [Header("Camera Anchor Settings")]
    public float anchorVerticalOffset = 0.1f; 
    public float anchorSmoothTime = 0.1f;    
    public float anchorFallThreshold = 4f;   

    // Internal "Ghost" Object
    private Transform ghostAnchor; 
    private float anchorCurrentY;
    private float anchorYVelocity;

    [Header("Speed Feel")]
    public float baseFOV = 80f;        // Normal view
    public float maxFOV = 100f;        // "Warp Speed" view
    public float zoomSpeed = 5f;       // How fast FOV changes
    public float speedForMaxEffect = 30f; // Velocity needed to reach Max FOV

    [Header("Slow Motion Settings")]
    public float minSlowMo = 0.05f;      // Tiny freeze for weak hits
    public float maxSlowMo = 0.5f;       // Matrix-style freeze for huge hits
    public float impactSpeedMax = 20f;   // The speed required to get the longest freeze
    public float slowMoScale = 0.1f;     // How slow time actually gets (10%)
    
    [Header("Settings")]
    public float rotationSpeed = 7f;
    public float blastShakeDuration = 1f;
    public float autoCamSpeed = 5f;

    // State Variables
    private bool wasAiming = false;
    private bool wasBlasted = false;     // To detect the moment we get hit
    private float disableAimTimer = 0f;  // The actual timer
    private Vector3 smoothedVelocity;


    private void Start()
    {
        // Setup Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure we start in Exploration Mode
        SetCameraMode(false);

        // auto-align cameras
        if (playerController != null)
        {
            // 1. Create a hidden object in the scene
            GameObject anchorObj = new GameObject("GhostCameraAnchor");
            ghostAnchor = anchorObj.transform;

            // 2. Snap it to the player's initial position
            anchorCurrentY = playerController.transform.position.y + anchorVerticalOffset;
            ghostAnchor.position = new Vector3(playerController.transform.position.x, anchorCurrentY, playerController.transform.position.z);

            // 3. Tell Cinemachine to follow THIS instead of the player
            if (explorationCam != null) 
            {
                explorationCam.Follow = ghostAnchor;
                explorationCam.LookAt = ghostAnchor;
            }
            if (focusCam != null) 
            {
                focusCam.Follow = ghostAnchor;
                focusCam.LookAt = ghostAnchor;
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {      
        // --- 1. HANDLE BLAST TIMER ---
        bool isBlasted = playerController.blastMode;

        if (isBlasted && !wasBlasted) {
            disableAimTimer = blastShakeDuration; 

            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse(Vector3.up * shakeStrength);
            }

            float impactSpeed = playerRb.linearVelocity.magnitude;
            float t = Mathf.InverseLerp(0, impactSpeedMax, impactSpeed);

            float curvedT = t * t;
            float dynamicDuration = Mathf.Lerp(minSlowMo, maxSlowMo, curvedT);

            StartCoroutine(TriggerSlowMo(dynamicDuration));
        }

        if (disableAimTimer > 0)
        {
            disableAimTimer -= Time.deltaTime;
            AlignCameraWithVelocity();
        }
            

        wasBlasted = isBlasted;

        // --- 2. Dynamic FOV & Particles ---
        HandleSpeedEffects();
        
        // --- 3. HANDLE CAMERA SWITCHING ---
        bool holdingAimButton = Input.GetMouseButton(1);
        bool isAiming = holdingAimButton && (disableAimTimer <= 0);

        if (isAiming != wasAiming)
        {
            SetCameraMode(isAiming);
            wasAiming = isAiming;
        }

        if (playerController != null)
        {
            playerController.IsAiming = isAiming;
        }
    }

    private void LateUpdate()
    {
        if (playerController == null || ghostAnchor == null) return;

        Vector3 playerPos = playerController.transform.position;
        float targetY = playerPos.y + anchorVerticalOffset;

        // LOGIC: Hybrid Following
        // 1. If we are in the AIR (Jumping/Falling), snap INSTANTLY.
        //    This ensures the camera moves with the player, preventing "panning" (tilting).
        if (!playerController.isGrounded)
        {
            anchorCurrentY = targetY; 
            anchorYVelocity = 0f; // Reset velocity so it doesn't drift when we land
        }
        // 2. If we are GROUNDED (Walking), use SmoothDamp.
        //    This filters out the jittery "head bob" from the physics animations.
        else
        {
            anchorCurrentY = Mathf.SmoothDamp(anchorCurrentY, targetY, ref anchorYVelocity, anchorSmoothTime);
        }

        // Apply Position: Lock X/Z to player, Hybrid Y
        ghostAnchor.position = new Vector3(playerPos.x, anchorCurrentY, playerPos.z);
        
        // Match rotation (Keep this so aiming works)
        ghostAnchor.rotation = playerController.transform.rotation;
    }

    void SetCameraMode(bool isAiming)
    {
        if (isAiming)
        {
            focusCam.Priority = 20;       // Focus wins (20 > 10)
            if(crosshairUI) crosshairUI.enabled = true; // Show Crosshair

            focusCam.GetComponent<CameraHandoff>().Sync();
        }
        else
        {
            focusCam.Priority = 5;        // Exploration wins (10 > 5)
            if(crosshairUI) crosshairUI.enabled = false; // Hide Crosshair

            explorationCam.GetComponent<CameraHandoff>().Sync();

            explorationCam.Lens.FieldOfView = baseFOV;
        }
    }

    void AlignCameraWithVelocity()
    {
        // 1. Get the raw velocity (Ignore Up/Down)
        Vector3 rawFlatVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);

        // 2. Filter the noise!
        // Instead of using raw velocity immediately, we "Lerp" the vector.
        // This removes micro-jitters from physics collisions.
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, rawFlatVel, Time.deltaTime * 10f);

        // 3. Only rotate if moving fast enough
        if (smoothedVelocity.magnitude > 2f) 
        {
            // Calculate angle from the SMOOTHED velocity
            float targetAngle = Mathf.Atan2(smoothedVelocity.x, smoothedVelocity.z) * Mathf.Rad2Deg;

            var orbital = explorationCam.GetComponent<CinemachineOrbitalFollow>();
            
            if (orbital != null)
            {
                float currentAngle = orbital.HorizontalAxis.Value;
                
                // 4. Smoothly rotate the camera (Damping)
                // We use a lower speed here (e.g. 5f) for the camera swing to feel heavy/cinematic
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * autoCamSpeed);
                
                orbital.HorizontalAxis.Value = newAngle;
            }
        }
    }

    void HandleSpeedEffects()
    {
        float currentSpeed = playerRb.linearVelocity.magnitude;

        // Calculate Intensity 
        float t = Mathf.InverseLerp(0, speedForMaxEffect, currentSpeed);

        // Calculate and Apply FOV
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, t);
        explorationCam.Lens.FieldOfView = Mathf.Lerp(explorationCam.Lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    IEnumerator TriggerSlowMo(float duration)
    {
        Time.timeScale = slowMoScale; 
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Use the calculated duration here
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
