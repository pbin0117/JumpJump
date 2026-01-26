using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("Settings")]
    public float bounceMultiplier = 2.5f;
    public float minJumpForce = 25f; 
    public float maxJumpForce = 60f; 
    
    // Threshold: Impacts slower than this are ignored
    public float velocityThreshold = 1.5f; 

    [Header("Cooldown")]
    public float bounceCooldown = 0.2f; 
    private float lastBounceTime = -1f; 

    // CHANGED: From OnCollisionEnter to OnTriggerEnter
    // Parameter is now 'Collider', not 'Collision'
    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < lastBounceTime + bounceCooldown) return;

        RagdollController ragdollController = other.GetComponentInParent<RagdollController>();

        if (ragdollController != null)
        {
            // 1. Calculate Force (Same logic as before)
            Rigidbody rootRb = ragdollController.GetComponent<Rigidbody>();
            float trueFallSpeed = Mathf.Abs(rootRb.linearVelocity.y);

            if (trueFallSpeed < velocityThreshold) return;

            lastBounceTime = Time.time;

            float finalForce = trueFallSpeed * bounceMultiplier;
            finalForce = Mathf.Clamp(finalForce, minJumpForce, maxJumpForce);

            // 2. THE CLEAN CALL
            ragdollController.ExternalLaunch(Vector3.up * finalForce);

            Debug.Log($"Trampoline Clean Launch: {finalForce}");
        }
    }
}