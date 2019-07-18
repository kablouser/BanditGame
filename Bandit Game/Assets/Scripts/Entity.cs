using UnityEngine;
using System;
using System.Collections.Generic;

public class Entity : Hitbox
{
    [Header("If this entity dies, these are deactivated.")]
    public MonoBehaviour[] attachedBehaviours;
    public float defence = 1.0f;

    //This tag allows the struct to be editable in Unity editor.
    [System.Serializable]
    private struct RegeneratingStat
    {
        public float maximum;
        public float current;
        public float regenRate;

        public void Regen(float timePassed)
        {
            if (current < maximum)
            {
                current += regenRate * timePassed;
                if (current > maximum)
                    current = maximum;
            }
        }
    }

    [Header("Don't change these stats during play.")]
    [SerializeField]
    private RegeneratingStat health;

    //This tag allows the struct to be editable in Unity editor.
    [System.Serializable]
    private struct PartDamage
    {
        public Collider hitCollider;
        public float damageMultiplier;
    }

    [SerializeField]
    private List<PartDamage> damageAreas;

    public override float Hit(Collider hitCollider, float incomingForce)
    {
        float reboundForce = CalculateReboundForce(incomingForce, defence);

        if (reboundForce < 0)
        {
            try
            {
                PartDamage partDamage = damageAreas.Find(x => x.hitCollider == hitCollider);
                DealDamage(incomingForce * partDamage.damageMultiplier);
            }
            catch (ArgumentNullException)
            {
                //Has not found the part
                DealDamage(incomingForce);
            }
        }

        return reboundForce;
    }

    public void DealDamage(float damage)
    {
        print(name + " took damage!");

        health.current -= damage;
        if (health.current < 0)
            health.current = 0;

        SetAlive(health.current > 0);
    }

    private void SetAlive(bool isAlive)
    {
        if(isAlive == false)
            print(name + " died!");

        enabled = isAlive;
        foreach(MonoBehaviour behaviour in attachedBehaviours)
        {
            if(behaviour)
                behaviour.enabled = isAlive;
        }
    }

    private void FixedUpdate()
    {
        health.Regen(Time.fixedDeltaTime);
    }
}
