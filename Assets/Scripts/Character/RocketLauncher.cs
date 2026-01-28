using UnityEngine;
using UnityEngine.UI;

public class RocketLauncher : MonoBehaviour
{
    [Header("References")]
    public Camera playerCam;
    public GameObject explosionEffect; // Drag a particle prefab here later
    public Image cooldownImage;

    [Header("Settings")]
    public float explosionForce = 20f;
    public float explosionRadius = 5f;
    
    // The "Rocket Jump" magic number. 
    // Higher number = More vertical lift, less horizontal push.
    public float upwardsModifier = 3f; 

    public LayerMask whatIsHittable;

    [Header("Cooldown")]
    public float cooldownTime = 1.5f; // Seconds between shots
    private float nextFireTime = 0f;


    [Header("Sound")]
    public AudioClip explosionSound;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {   
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        
        UpdateCooldownUI();

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime) // Default Left Click
        {
            Shoot();
        }
    }

    void Shoot()
    {   
        nextFireTime = Time.time + cooldownTime;

        RaycastHit hit;
        // Find the EXACT center of the screen (Position of Crosshair)
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out hit, 100f, whatIsHittable))
        {
            Instantiate(explosionEffect, hit.point, Quaternion.LookRotation(hit.normal));

            audioSource.PlayOneShot(explosionSound);

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
                    
                    GameManager.Instance.AddJump();
                }
            }
        }
    }

    void UpdateCooldownUI()
    {
        if (cooldownImage == null) return;

        // 1. Calculate Fill
        float timePassed = cooldownTime - (nextFireTime - Time.time);
        float fillPercent = Mathf.Clamp01(timePassed / cooldownTime);
        cooldownImage.fillAmount = fillPercent;

        // 2. Handle Fading
        Color currentColor = cooldownImage.color;

        if (fillPercent >= 1f)
        {
            // If fully charged, fade out smoothly
            // "Time.deltaTime * 5f" controls the fade speed (higher = faster)
            currentColor.a = Mathf.MoveTowards(currentColor.a, 0f, Time.deltaTime * 5f);
        }
        else
        {
            // If currently reloading, snap to fully visible immediately
            currentColor.a = 1f;
        }

        cooldownImage.color = currentColor;
    }
}