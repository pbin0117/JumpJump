using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;

public class HandGrabHandler : MonoBehaviour
{
    [SerializeField] Animator animator;

    FixedJoint fixedJoint;

    Rigidbody rigidbody3D;

    NetworkPlayer networkPlayer;

    void Awake()
    {
        // Get references
        networkPlayer = transform.root.GetComponent<NetworkPlayer>();
        rigidbody3D = GetComponent<Rigidbody>();

        rigidbody3D.solverIterations = 100;
    }

    bool TryCarryObject(Collision collision)
    {
        // Check if we are allowed to carry objects, only state authority is allowed.
        if (!networkPlayer.Object.HasStateAuthority)
            return false;

        // Check that we are not in active ragdoll mode
        // if (networkPlayer.IsActiveRagdoll)
        //     return false;
            
        // Check if we are already carrying another object
        if (fixedJoint != null)
            return false;

        // Avoid trying to grab yourself
        if (collision.transform.root == networkPlayer.transform)
            return false;

        // Get the other rigidbody if there is one
        if (!collision.collider.TryGetComponent(out Rigidbody otherObjectRigidbody))
            return false;

        // Add a fixed joint
        fixedJoint = transform.gameObject.AddComponent<FixedJoint>();

        // Connect the joint to the other object's rigidbody
        fixedJoint.connectedBody = otherObjectRigidbody;

        // We'll take care of the anchor point on our own
        fixedJoint.autoConfigureConnectedAnchor = false;

        // Transform the collision point from world to local space
        fixedJoint.connectedAnchor = collision.transform.InverseTransformPoint(
            collision.GetContact(0).point
        );

        // Set animator to carrying
        animator.SetBool("isCarrying", true);

        return true;

    }

    void OnCollisionEnter(Collision collision)
    {
        // Attempt to carry the other object
        TryCarryObject(collision);
    }

}
