using UnityEngine;

public class DevTeleport : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode cheatKey = KeyCode.T; // Press 'T' to Teleport
    public Transform targetLocation;     // Drag your "Finish Line" object here
    
    [Header("Player Reference")]
    public GameObject player;            // Drag your Player here

    void Update()
    {
        // 1. Listen for the key press
        if (Input.GetKeyDown(cheatKey))
        {
            TeleportPlayer();
        }
    }

    void TeleportPlayer()
    {
        if (player == null || targetLocation == null) 
        {
            Debug.LogWarning("DevTeleport: Missing Player or Target!");
            return;
        }

        // 2. Reset Physics (Crucial!)
        // If we don't do this, and you teleport while falling fast, 
        // you might smash into the ground at the new location.
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;       // Stop moving
            rb.angularVelocity = 0f;          // Stop spinning
        }

        // 3. Move the Player
        player.transform.position = targetLocation.position;

        Debug.Log("Cheat Activated: Teleported to " + targetLocation.name);
    }
}