using UnityEngine;

public class TrampolineSensor : MonoBehaviour
{
    // Drag the Parent's script here in the Inspector
    public Trampoline mainScript;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Find the Player's Rigidbody
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            // 2. Get the pure FALLING speed
            // (We only care if they are moving DOWN)
            if (rb.linearVelocity.y < 0)
            {
                float fallSpeed = Mathf.Abs(rb.linearVelocity.y);
                
                // 3. Send it to the main brain
                mainScript.RecordEntrySpeed(fallSpeed);
            }
        }
    }
}