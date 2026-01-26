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
    public Transform player;      // The actual player Transform (Capsule)
    public Transform playerObj;   // The visual mesh inside the player
    public Transform orientation; // The empty object that defines "Forward"
    public Transform playerCam;   // Drag your MAIN CAMERA here
    
    [Header("Settings")]
    public float rotationSpeed = 7f;

    private bool wasAiming = false;

    private void Start()
    {
        // Setup Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure we start in Exploration Mode
        SetCameraMode(false);
    }

    // Update is called once per frame
    private void Update()
    {   
        // --- 1. HANDLE CAMERA SWITCHING ---
        bool isAiming = Input.GetMouseButton(1);
        if (isAiming != wasAiming)
        {
            SetCameraMode(isAiming);
            wasAiming = isAiming;
        }
        

        // --- 2. HANDLE PLAYER ORIENTATION --- 
        Vector3 viewDir = playerCam.forward;
        viewDir.y = 0;
        orientation.forward = viewDir.normalized;

        if (isAiming)
        {
            // AIMING MODE
            RotatePlayerToCrosshair();
        }
        else 
        {   
            // EXPLORATION MODE
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero) 
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }

    void SetCameraMode(bool isAiming)
    {
        if (isAiming)
        {
            focusCam.Priority = 20;       // Focus wins (20 > 10)
            if(crosshairUI) crosshairUI.enabled = true; // Show Crosshair
        }
        else
        {
            focusCam.Priority = 5;        // Exploration wins (10 > 5)
            if(crosshairUI) crosshairUI.enabled = false; // Hide Crosshair
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
        Vector3 aimDir = targetPoint - player.position;
        aimDir.y = 0; // Keep the player upright (don't tilt up/down)

        // Smoothly rotate to face that point
        playerObj.forward = Vector3.Slerp(playerObj.forward, aimDir.normalized, Time.deltaTime * 20f); // Faster speed for aiming
    }
}
