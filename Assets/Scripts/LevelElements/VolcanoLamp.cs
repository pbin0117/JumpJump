using UnityEngine;

public class VolcanoLamp : MonoBehaviour
{
    [Header("Eruption Settings")]
    public float launchForce = 100f; 
    public float horizontalVariance = 10f; // NEW: How much "drift" to add (0 = none, 20 = chaotic)

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
            rb.linearVelocity = Vector3.zero;
            rb.position += Vector3.up * 0.5f;

            // 1. Generate Random Drift
            // 'insideUnitCircle' gives us a random point inside a circle (X, Y)
            Vector2 randomCircle = Random.insideUnitCircle * horizontalVariance;

            // 2. Convert to 3D Vector (X, 0, Z)
            // We map the circle's Y to the world's Z axis
            Vector3 randomDrift = new Vector3(randomCircle.x, 0f, randomCircle.y);

            // 3. Combine Upward Force + Random Drift
            Vector3 finalForceDirection = (Vector3.up * launchForce) + randomDrift;

            // 4. Apply Force
            rb.AddForce(finalForceDirection, ForceMode.Impulse);

            // --- STATE ---
            ragdollController.ApplyBlastForce();
        }
    }
}