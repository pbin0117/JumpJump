using UnityEngine;

public class HandGrabHandler : MonoBehaviour
{
    [SerializeField] Animator animator;
    FixedJoint fixedJoint;
    Rigidbody rigidbody3D;
    
    // Changed reference type
    RagdollController playerController; 

    void Awake()
    {
        // Changed GetComponent
        playerController = transform.root.GetComponent<RagdollController>();
        rigidbody3D = GetComponent<Rigidbody>();
        rigidbody3D.solverIterations = 100;
    }

    bool TryCarryObject(Collision collision)
    {
        // REMOVED: if (!networkPlayer.Object.HasStateAuthority) return false;

        if (fixedJoint != null) return false;
        if (collision.transform.root == playerController.transform) return false;

        if (!collision.collider.TryGetComponent(out Rigidbody otherObjectRigidbody))
            return false;

        fixedJoint = transform.gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = otherObjectRigidbody;
        fixedJoint.autoConfigureConnectedAnchor = false;
        fixedJoint.connectedAnchor = collision.transform.InverseTransformPoint(collision.GetContact(0).point);

        animator.SetBool("isCarrying", true);

        return true;
    }

    void OnCollisionEnter(Collision collision)
    {
        TryCarryObject(collision);
    }
}