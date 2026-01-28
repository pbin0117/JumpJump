using UnityEngine;

public class FallSFX : MonoBehaviour
{
    public AudioClip fallClip;

    AudioSource audioSource;
    Rigidbody rb;

    public float fallSpeedThreshold = -6f;
    public float dropDistanceThreshold = 2.5f;
    public float disarmDistance = 2.5f;

    float peakY;
    bool armed;
    float lastVelY;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        peakY = transform.position.y;
    }

    void FixedUpdate()
    {
        lastVelY = rb.linearVelocity.y;

        float y = transform.position.y;

        if (y > peakY) peakY = y;

        if (!armed && (peakY - y) >= dropDistanceThreshold)
            armed = true;

        if (armed && y >= peakY - disarmDistance)
            armed = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!armed) return;
        if (lastVelY >= fallSpeedThreshold) return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                audioSource.PlayOneShot(fallClip);
                armed = false;
                peakY = transform.position.y;
                break;
            }
        }
    }
}
