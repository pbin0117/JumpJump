using UnityEngine;

public class SyncPhysicsObject : MonoBehaviour
{
    Rigidbody rigidbody3D;
    ConfigurableJoint joint;

    [SerializeField]
    Rigidbody animatedRigidbody3D;

    [SerializeField]
    bool syncAnimation = false;

    Quaternion startLocalRoatation;

    void Awake()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        startLocalRoatation = transform.localRotation;
    }

    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation) return;

        if (joint == null)
        {
            Debug.LogError($"[SyncPhysicsObject] ConfigurableJoint missing on {name}", this);
            return;
        }

        if (animatedRigidbody3D == null)
        {
            Debug.LogError($"[SyncPhysicsObject] animatedRigidbody3D not assigned on {name}", this);
            return;
        }

        ConfigurableJointExtensions.SetTargetRotationLocal(
            joint,
            animatedRigidbody3D.transform.localRotation,
            startLocalRoatation
        );
    }

}
