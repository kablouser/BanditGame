using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Hitbox ragdollHitbox;
    public Animator ragdollAnimator;
    public int hitboxLayer = 8;

    [SerializeField]
    private Rigidbody[] allRigidbodies;

    [SerializeField]
    private Collider[] allColliders;

    [ContextMenu("Link Ragdoll")]
    public void LinkRagdoll()
    {
        List<Rigidbody> rigidbodyList = new List<Rigidbody>();
        List<Collider> colliderList = new List<Collider>();
        LinkTransform(transform, rigidbodyList, colliderList);
        allRigidbodies = rigidbodyList.ToArray();
        allColliders = colliderList.ToArray();
    }

    private void LinkTransform(Transform target, List<Rigidbody> rigidbodyList, List<Collider> colliderList)
    {
        if(target.GetComponent<Weapon>())
        {
            return;
        }
        for(int i = 0; i < target.childCount; i++)
        {
            LinkTransform(target.GetChild(i), rigidbodyList, colliderList);
        }
        if(target.GetComponent<Rigidbody>())
        {
            rigidbodyList.Add(target.GetComponent<Rigidbody>());
        }
        if(target.GetComponent<Collider>())
        {
            RagdollHitbox ragdollHitbox = target.GetComponent<RagdollHitbox>();
            if(ragdollHitbox == null)
                ragdollHitbox = target.gameObject.AddComponent<RagdollHitbox>();
            ragdollHitbox.controller = this;

            colliderList.Add(target.GetComponent<Collider>());
        }
    }

    [ContextMenu("Activate Ragdoll")]
    public void ActivateRagdoll()
    {
        ToggleRagdoll(true);
    }

    [ContextMenu("Deactivate Ragdoll")]
    public void DeactivateRagdoll()
    {
        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool enabled)
    {
        ragdollAnimator.enabled = !enabled;
        foreach (Rigidbody rigid in allRigidbodies)
        {
            rigid.isKinematic = !enabled;
        }

        foreach(Collider collider in allColliders)
        {
            collider.gameObject.layer = enabled ? 0 : hitboxLayer;
        }
    }
}
