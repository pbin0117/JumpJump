using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("Settings")]
    public float bounceMultiplier = 2.5f;
    public float minJumpForce = 25f; 
    public float maxJumpForce = 60f; 

    [Header("Cooldown")]
    public float bounceCooldown = 0.5f; // Add this variable
    private float lastBounceTime = -1f; // Add this variable
    
    // How long the "stored speed" is valid. 
    // Prevents storing a speed, walking away, and bouncing 10 mins later.
    public float dataExpiryTime = 0.2f; 

    // Internal State
    private float storedDropSpeed = 0f;
    private float storedTime = -1f;

    // ---------------------------------------------------------
    // 1. THE SENSOR (The Trigger Child calls this)
    // ---------------------------------------------------------
    public void RecordEntrySpeed(float speed)
    {
        // Only record if we are actually falling (Speed > 0)
        // We use Abs() before sending, so logic handles positive values.
        storedDropSpeed = speed;
        storedTime = Time.time;
        
        // Debug.Log($"Sensor Recorded Speed: {storedDropSpeed}");
    }

    // ---------------------------------------------------------
    // 2. THE ACTION (The Solid Base triggers this)
    // ---------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        // 1. STOP MULTIPLE HITS
        // If we bounced less than 0.5 seconds ago, ignore everything else.
        if (Time.time < lastBounceTime + bounceCooldown) return;

        bool hasValidData = (Time.time - storedTime) <= dataExpiryTime;
        RagdollController ragdollController = collision.gameObject.GetComponentInParent<RagdollController>();

        if (ragdollController != null)
        {
            // 2. MARK THE TIME
            // We are committing to this bounce. No more hits allowed.
            lastBounceTime = Time.time;

            float impactSpeed = hasValidData ? storedDropSpeed : Mathf.Abs(collision.relativeVelocity.y);
            float finalForce = impactSpeed * bounceMultiplier;
            finalForce = Mathf.Clamp(finalForce, minJumpForce, maxJumpForce);

            ragdollController.ExternalLaunch(Vector3.up * finalForce);

            Debug.Log($"Trampoline Bounce! Used Sensor? {hasValidData}. Force: {finalForce}");
            
            storedTime = -1f; 
        }
    }
}