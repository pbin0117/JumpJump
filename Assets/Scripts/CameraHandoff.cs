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

    public void Sync()
    {   
        // 1. Check if we have the component and a Main Camera
        if (Camera.main != null && orbitalFollow != null)
        {
            // 2. Get the current physical rotation of the Main Camera
            Vector3 currentRotation = Camera.main.transform.rotation.eulerAngles;

            // 3. Normalize BOTH angles to be between -180 and 180
            // (This fixes the "always pointing same direction" bug if you turn past 180)
            float safeYaw = NormalizeAngle(currentRotation.y);
            float safePitch = NormalizeAngle(currentRotation.x);

            // 3. Inject values
            orbitalFollow.HorizontalAxis.Value = safeYaw;
            orbitalFollow.VerticalAxis.Value = safePitch;

            // Debug.Log($"[Handoff] Synced {gameObject.name} to Yaw: {safeYaw}, Pitch: {safePitch}");
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