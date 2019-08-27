using UnityEngine;

public class Weapon : Hitbox
{
    [Header("Weapon")]
    public BoxCollider levelCollider;

    [SerializeField]
    private Hitbox parentedHitbox;
    public Hitbox GetParentedHitbox
    {
        get
        {
            return parentedHitbox;
        }
    }

    private Rigidbody rigid
    {
        get
        {
            return GetComponent<Rigidbody>();
        }
    }

    public override float Hit(Collider hitCollider, float incomingAttack)
    {
        return CalulateDamage(incomingAttack, 0);
    }

    public void EquipTo(Transform parent, Hitbox parentHitbox)
    {
        transform.parent = parent;
        if (parent)
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            rigid.isKinematic = true;
            levelCollider.enabled = false;
        }
        else
        {
            rigid.isKinematic = false;
            levelCollider.enabled = true;
        }
        parentedHitbox = parentHitbox;
    }

    /// <summary>
    /// Checks whether its parented hitbox is the same as the input.
    /// </summary>
    /// <param name="hitbox">input hitbox object</param>
    /// <returns>true if they are the same, false otherwise</returns>
    public bool CheckParentHitbox(Hitbox hitbox)
    {
        return hitbox == parentedHitbox && hitbox != null;
    }

    public MovementController GetParentMovement()
    {
        if (parentedHitbox)
            return parentedHitbox.GetComponent<MovementController>();
        else
            return null;
    }
}