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
    public PlayerMovement playerMovement; 
    public Rigidbody playerRb;
    public Transform playerObj;   // The visual mesh inside the player
    public Transform orientation; // The empty object that defines "Forward"
    public Transform playerCam;   // Drag your MAIN CAMERA here

    [Header("Effects")]
    public CinemachineImpulseSource impulseSource; // Drag Player here (with Impulse Source component)
    public float shakeStrength = 0.5f;

    [Header("Speed Feel")]
    public float baseFOV = 80f;        // Normal view
    public float maxFOV = 100f;        // "Warp Speed" view
    public float zoomSpeed = 5f;       // How fast FOV changes
    public float speedForMaxEffect = 30f; // Velocity needed to reach Max FOV
    
    [Header("Settings")]
    public float rotationSpeed = 7f;
    public float blastShakeDuration = 1f;
    public float autoCamSpeed = 5f;

    // State Variables
    private bool wasAiming = false;
    private bool wasBlasted = false;     // To detect the moment we get hit
    private float disableAimTimer = 0f;  // The actual timer
    private Vector3 smoothedVelocity;

    private CinemachineBasicMultiChannelPerlin perlinNoise;

    private void Start()
    {
        // Setup Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure we start in Exploration Mode
        SetCameraMode(false);

        perlinNoise = explorationCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    private void Update()
    {      
        // --- 1. HANDLE BLAST TIMER ---
        bool isBlasted = playerMovement.blastMode;

        if (isBlasted && !wasBlasted) {
            disableAimTimer = blastShakeDuration; 

            if (impulseSource != null)
            {
                impulseSource.GenerateImpulse(Vector3.up * shakeStrength);
            }
        }

        if (disableAimTimer > 0)
        {
            disableAimTimer -= Time.deltaTime;
            AlignCameraWithVelocity();
        }
            

        wasBlasted = isBlasted;

        // --- 2. Dynamic FOV & Particles ---
        HandleSpeedEffects();
        
        // --- 2. HANDLE CAMERA SWITCHING ---
        bool holdingAimButton = Input.GetMouseButton(1);
        bool isAiming = holdingAimButton && (disableAimTimer <= 0);

        if (isAiming != wasAiming)
        {
            SetCameraMode(isAiming);
            wasAiming = isAiming;
        }
        

        // --- 3. HANDLE PLAYER ORIENTATION --- 
        Vector3 viewDir = playerCam.forward;
        viewDir.y = 0;
        orientation.forward = viewDir.normalized;

        if (isAiming)
        {
            // AIMING MODE
            RotatePlayerToCrosshair();
        }
        else if (!isBlasted)
        {   
            // EXPLORATION MODE
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero) 
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
        // else if blasted and not aiming, do nothing 
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

    void RotatePlayerToCrosshair()
    {
        // Raycast from center of screen to find what we are looking at
        Ray ray = playerCam.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        Vector3 targetPoint;

        // Did we hit a wall/enemy?
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            targetPoint = hit.point;
        }
        else
        {
            // Hit nothing (Sky)? Aim at a point far in the distance
            targetPoint = ray.GetPoint(1000f);
        }

        // Calculate direction to that point
        Vector3 aimDir = targetPoint - playerMovement.transform.position;
        aimDir.y = 0; // Keep the player upright (don't tilt up/down)

        // Smoothly rotate to face that point
        playerObj.forward = Vector3.Slerp(playerObj.forward, aimDir.normalized, Time.deltaTime * 20f); // Faster speed for aiming
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
}
