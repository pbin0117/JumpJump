using UnityEngine;

public class RisingObject : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minSpeed = 5f;
    public float maxSpeed = 10f;
    private float speed;

    [Header("Lifetime")]
    public float lifetime = 5f; // Destroy after 5 seconds

    void Start()
    {
        // Pick a random speed for variety
        speed = Random.Range(minSpeed, maxSpeed);
        
        // Randomize size slightly for variety
        float scale = Random.Range(0.8f, 1.2f);
        transform.localScale = transform.localScale * scale;

        // Auto-destroy so we don't clog up memory
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move straight up
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
}