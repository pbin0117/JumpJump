using UnityEngine;

public class BackgroundSpawner : MonoBehaviour
{
    [Header("What to Spawn")]
    public GameObject[] prefabs; 

    [Header("Spawn Settings")]
    public float spawnInterval = 0.2f; 
    public float width = 10f;          

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    void SpawnObject()
    {
        if (prefabs.Length == 0) return;

        // 1. Pick a random prefab
        int randomIndex = Random.Range(0, prefabs.Length);
        GameObject prefabToSpawn = prefabs[randomIndex];

        // 2. Pick a random X position
        float randomX = Random.Range(-width, width);
        Vector3 spawnPos = new Vector3(transform.position.x + randomX, transform.position.y, 0);

        // 3. GENERATE RANDOM ROTATION
        // "Random.rotation" gives a completely random orientation in 3D (X, Y, and Z)
        Quaternion randomRot = Random.rotation;

        // *Alternative for 2D Sprites*: If you only want it to spin like a clock (Z-axis only):
        // Quaternion randomRot = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        // 4. Spawn it with that rotation
        Instantiate(prefabToSpawn, spawnPos, randomRot);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position - Vector3.right * width, transform.position + Vector3.right * width);
    }
}