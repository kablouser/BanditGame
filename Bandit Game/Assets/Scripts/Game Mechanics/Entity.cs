using UnityEngine;
using System.Collections.Generic;
using Attributes;

public class Entity : Hitbox
{
    [Tooltip("If this entity dies, these are deactivated.")]
    public MonoBehaviour[] attachedBehaviours;
    public float defence = 0;
    public RagdollController ragdollController;
    public Collider levelCollider;
    public Rigidbody levelRigidbody;
    public MovementController movementController;

    [Header("Don't change these stats during play.")]
    [SerializeField]
    private RegulatedAttribute entityAttribute;

    public RegulatedAttribute GetAttribute
    {
        get
        {
            return entityAttribute;
        }
    }

    //This tag allows the struct to be editable in Unity editor.
    [System.Serializable]
    private struct PartDamage
    {
        public Collider hitCollider;
        public float damageMultiplier;
    }

    [SerializeField]
    private List<PartDamage> damageAreas;

    public override float Hit(Collider hitCollider, float incomingAttack)
    {
        float damage = CalulateDamage(incomingAttack, defence);

        if (damage > 0)
        {
            PartDamage partDamage = damageAreas.Find(x => x.hitCollider == hitCollider);
            if(partDamage.hitCollider)
                DealDamage(incomingAttack * partDamage.damageMultiplier);

            //Has not found the part
            else
                DealDamage(incomingAttack);           
        }

        return damage;
    }

    public override float HitPosition(Collider hitCollider, float incomingAttack, Weapon useWeapon)
    {
        if (movementController && useWeapon.GetParentMovement() && movementController.BlockAttack(useWeapon.GetParentMovement().transform.position, out Weapon blockWeapon))
        {
            return blockWeapon.HitPosition(blockWeapon.GetComponentInChildren<Collider>(), incomingAttack, useWeapon);
        }
        return base.HitPosition(hitCollider, incomingAttack, useWeapon);
    }

    public void DealDamage(float damage)
    {
        entityAttribute.ClampedAdd(-damage, 0, 0);

        SetAlive(entityAttribute.current.health > 0);
    }

    [ContextMenu("Revive")]
    public void Revive()
    {
        SetAlive(true);
    }

    [ContextMenu("Kill")]
    public void Kill()
    {
        SetAlive(false);
    }

    public bool SpendAttributes(float health, float mana, float stamina)
    {
        return entityAttribute.Spend(health, mana, stamina);
    }

    public bool SpendAttributesOverTime(Attribute activateCost, Attribute drainCost, bool currentlyActive, bool previouslyActive, float timePassed)
    {
        return entityAttribute.SpendOverTime(activateCost, drainCost, currentlyActive, previouslyActive, timePassed);
    }

    private void SetAlive(bool isAlive)
    {
        enabled = isAlive;
        foreach(MonoBehaviour behaviour in attachedBehaviours)
        {
            if(behaviour)
                behaviour.enabled = isAlive;
        }

        SetRagdoll(!isAlive);
    }

    private void SetRagdoll(bool enabled)
    {
        levelRigidbody.isKinematic = enabled;
        levelCollider.enabled = !enabled;
        ragdollController.ToggleRagdoll(enabled);
    }

    private void FixedUpdate()
    {
        entityAttribute.Regen(Time.fixedDeltaTime);
    }
}
