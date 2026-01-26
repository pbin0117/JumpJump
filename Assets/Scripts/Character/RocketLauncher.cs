using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("References")]
    public Camera playerCam;
    public GameObject explosionEffect; // Drag a particle prefab here later

    [Header("Settings")]
    public float explosionForce = 20f;
    public float explosionRadius = 5f;
    
    // The "Rocket Jump" magic number. 
    // Higher number = More vertical lift, less horizontal push.
    public float upwardsModifier = 3f; 

    public LayerMask whatIsHittable;

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Default Left Click
        {
            Shoot();
        }
    }

    void Shoot()
    {
        RaycastHit hit;
        // Find the EXACT center of the screen (Position of Crosshair)
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out hit, 100f, whatIsHittable))
        {
            Instantiate(explosionEffect, hit.point, Quaternion.LookRotation(hit.normal));

            // Find everything in range of the explosion
            Collider[] colliders = Physics.OverlapSphere(hit.point, explosionRadius);
            
            foreach (Collider nearbyObject in colliders)
            {
                Rigidbody rb = nearbyObject.GetComponentInParent<Rigidbody>();
                if (rb != null)
                {
                    // 1. Apply Physics Force
                    rb.AddExplosionForce(explosionForce * 10, hit.point, explosionRadius, upwardsModifier, ForceMode.Impulse);

                    // 2. apply blast force
                    RagdollController ragdollController = nearbyObject.GetComponentInParent<RagdollController>();
                    if (ragdollController != null)
                        ragdollController.ApplyBlastForce();
                }
            }
        }
    }
}