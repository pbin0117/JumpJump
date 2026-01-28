using UnityEngine;

public class RollImpulseReceiver : MonoBehaviour
{
    public float hardImpactSpeed = 8f;
    public float upVelChange = 0.6f;
    public float sideVelChange = 1.2f;
    public float torqueVelChange = 2.0f;
    public float cooldown = 0.5f;

    Rigidbody rb;
    float lastTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Trigger(Vector3 normal, float impactSpeed)
    {
        if (Time.time - lastTime < cooldown) return;
        if (normal.y <= 0.5f) return;
        if (impactSpeed < hardImpactSpeed) return;

        float t = Mathf.InverseLerp(hardImpactSpeed, hardImpactSpeed * 2f, impactSpeed);
        t = Mathf.Clamp01(t);

        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.right;
        dir.Normalize();

        Vector3 axis = Vector3.Cross(Vector3.up, dir).normalized;

        rb.AddForce(Vector3.up * (upVelChange * (0.6f + t)), ForceMode.VelocityChange);
        rb.AddForce(dir * (sideVelChange * (0.6f + t)), ForceMode.VelocityChange);
        rb.AddTorque(axis * (torqueVelChange * (0.6f + t)), ForceMode.VelocityChange);

        lastTime = Time.time;
    }
}
