using UnityEngine;

public class VolcanoLamp : MonoBehaviour
{
    [Header("Eruption Settings")]
    public float launchForce = 100f; 
    [Header("Horizontal Settings")]
    // (1, 0, 0) = East, (-1, 0, 0) = West, (0, 0, 1) = North
    public Vector3 launchDirection = new Vector3(1f, 0f, 0f); 
    public float horizontalSpeed = 1f; // How hard to push in that direction

    [Header("Effects")]
    public ParticleSystem lavaParticles;
    public AudioSource eruptionSound;

    private void OnCollisionEnter(Collision collision)
    {
        RagdollController ragdollController = collision.gameObject.GetComponentInParent<RagdollController>();
        Rigidbody rb = collision.gameObject.GetComponentInParent<Rigidbody>();

        if (ragdollController != null && rb != null)
        {
            // --- VISUALS ---
            if (lavaParticles != null) lavaParticles.Play();
            if (eruptionSound != null) eruptionSound.Play();

            // --- PHYSICS ---
            // 1. Reset Velocity so the launch is consistent every time
            rb.linearVelocity = Vector3.zero;
            rb.position += Vector3.up * 0.5f; // Unstick from ground

            // 2. Calculate the Constant Horizontal Force
            // .normalized ensures the direction is pure (length of 1)
            // We flatten y to 0 just in case you accidentally typed a Y value in the inspector
            Vector3 flatDirection = new Vector3(launchDirection.x, 0f, launchDirection.z).normalized;
            Vector3 sidewaysForce = flatDirection * horizontalSpeed;

            // 3. Combine Upward Force + Sideways Force
            Vector3 finalForce = (Vector3.up * launchForce) + sidewaysForce;

            // 4. Apply Force
            rb.AddForce(finalForce, ForceMode.Impulse);

            // --- STATE ---
            ragdollController.ApplyBlastForce();
            
            // Debug: Show the arrow in the Scene view so you can see where it aims
            Debug.DrawRay(transform.position, finalForce.normalized * 5f, Color.red, 2f);
        }
    }
}