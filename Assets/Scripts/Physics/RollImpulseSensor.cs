using UnityEngine;

public class RollImpulseSensor : MonoBehaviour
{
    [SerializeField] RollImpulseReceiver receiver;

    void Awake()
    {
        if (receiver == null)
            receiver = GetComponentInParent<RollImpulseReceiver>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (receiver == null) return;
        if (collision.contactCount == 0) return;

        Vector3 normal = collision.GetContact(0).normal;

        float downImpact = Mathf.Max(0f, -Vector3.Dot(collision.relativeVelocity, Vector3.up));
        float sideImpact = Vector3.ProjectOnPlane(collision.relativeVelocity, Vector3.up).magnitude;
        float impactSpeed = Mathf.Max(downImpact, sideImpact);

        receiver.Trigger(normal, impactSpeed);
    }
}
