using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("Settings")]
    public float bounceMultiplier = 2.5f;
    public float minJumpForce = 25f; // Increased default so you definitely see it
    public float maxJumpForce = 60f;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("1. Collision Detected with: " + collision.gameObject.name);

        // Find the Rigidbody on the player (or parent)
        Rigidbody rb = collision.gameObject.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            Debug.Log("2. RB Found. Calculating Bounce...");

            // Get the speed at which we hit the trampoline
            // We use the Y component of relative velocity to only care about falling speed
            float fallSpeed = Mathf.Abs(collision.relativeVelocity.y);
            Debug.Log("   > Impact Fall Speed: " + fallSpeed);

            // Calculate Force
            float finalForce = fallSpeed * bounceMultiplier;
            
            // Clamp (Ensure it's at least the minimum, but not crazy high)
            finalForce = Mathf.Clamp(finalForce, minJumpForce, maxJumpForce);
            Debug.Log("   > Final Bounce Force: " + finalForce);

            // FORCE APPLICATION STRATEGY
            // 1. Kill current velocity (stop falling)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // 2. Nudge Up (Crucial!)
            // We move the player 0.2 units up so they are no longer "touching" the collider.
            // This prevents the PlayerMovement script from applying "Ground Drag" next frame.
            rb.position += Vector3.up * 0.2f;

            // 3. Apply the Impulse
            rb.AddForce(Vector3.up * finalForce, ForceMode.Impulse);

            PlayerMovement pm = collision.gameObject.GetComponentInParent<PlayerMovement>();
            if (pm != null)
            {
                Debug.Log("4. Disabling Player Drag via BlastMode");
                pm.ApplyBlastForce(); // Uses the same logic as your Rocket Launcher
            }

            Debug.Log("3. BOUNCE APPLIED!");
        }
        else
        {
            Debug.LogError("X. Object hit trampoline but has NO Rigidbody!");
        }
    }
}