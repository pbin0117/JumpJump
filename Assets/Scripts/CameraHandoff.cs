using UnityEngine;
using Unity.Cinemachine;

public class CameraHandoff : MonoBehaviour
{
    private CinemachineOrbitalFollow orbitalFollow;

    void Awake()
    {
        // Find the component that actually holds the rotation angles
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    }

    void OnEnable()
    {
        // 1. Check if we have the component and a Main Camera
        if (Camera.main != null && orbitalFollow != null)
        {
            // 2. Get the current physical rotation of the Main Camera
            Vector3 currentRotation = Camera.main.transform.rotation.eulerAngles;

            // 3. Force the Orbital Follow internal values to match
            // This prevents the camera from "snapping back" to the past
            
            // Horizontal Axis (Y rotation / Yaw)
            orbitalFollow.HorizontalAxis.Value = currentRotation.y;
            
            // Vertical Axis (X rotation / Pitch)
            orbitalFollow.VerticalAxis.Value = NormalizeAngle(currentRotation.x);
        }
    }

    // Helper to handle the 360 wrap-around (e.g. 350 degrees -> -10 degrees)
    float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle > 180) return angle - 360;
        return angle;
    }
}