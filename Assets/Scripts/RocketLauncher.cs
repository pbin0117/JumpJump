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
        Debug.DrawRay(playerCam.transform.position, playerCam.transform.forward * 100f, Color.red, 2f);
        
        // Shoot a ray from the center of the camera
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, 100f, whatIsHittable))
        {   
            Debug.Log("Hit: " + hit.collider.name);

            // 1. Create visual explosion (Optional)
            if (explosionEffect != null)
                Instantiate(explosionEffect, hit.point, Quaternion.identity);

            // 2. Find everything in range of the explosion
            Collider[] colliders = Physics.OverlapSphere(hit.point, explosionRadius);
            
            foreach (Collider nearbyObject in colliders)
    {
            Rigidbody rb = nearbyObject.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                // 1. Apply Physics Force
                rb.AddExplosionForce(explosionForce * 10, hit.point, explosionRadius, upwardsModifier, ForceMode.Impulse);

                // 2. NEW: Disable Speed Limits on the Player
                PlayerMovement pm = nearbyObject.GetComponentInParent<PlayerMovement>();
                if (pm != null)
                    pm.ApplyBlastForce();
            }
    }
        }
        else
        {
            Debug.Log("Raycast hit NOTHING. Check your LayerMask!");
        }

    }
}