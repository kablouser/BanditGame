using UnityEngine;
using System.Collections.Generic;

public class Weapon : Hitbox
{
    public float defence = 1.0f;
    public float attack = 1.0f;

    public BoxCollider attackBox;
    public BoxCollider levelCollider;
    public LayerMask attackMask;

    public ParticleSystem slashTrail;

    private bool isAttacking;
    private bool Attacking
    {
        get
        {
            return isAttacking;
        }
        set
        {
            isAttacking = value;
            ToggleSlashTrails(value);
        }
    }
    private float stopAttacking;
    private Collider[] collisionBuffer;
    private List<Hitbox> alreadyHit;
    private Hitbox parentedHitbox;

    private Rigidbody rigid
    {
        get
        {
            return GetComponent<Rigidbody>();
        }
    }

    public override float Hit(Collider hitCollider, float incomingForce)
    {
        float reboundingForce = CalculateReboundForce(incomingForce, defence);
        if(reboundingForce < 0)
        {
            print(name + " failed block!");
            //interrupt attack
            Attacking = false;
        }
        else
        {
            print(name + " successfully block!");
        }

        return reboundingForce;
    }

    public void StartAttack(float duration, params Hitbox[] ignoreHits)
    {
        print("starting attacking");
        Attacking = true;
        stopAttacking = Time.time + duration;
        alreadyHit = new List<Hitbox>(ignoreHits)
        {
            this
        };
    }

    public void EquipTo(Transform parent, Hitbox parentHitbox)
    {
        transform.parent = parent;
        if (parent)
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            rigid.detectCollisions = false;
            rigid.isKinematic = true;
            levelCollider.enabled = false;
        }
        else
        {
            rigid.detectCollisions = true;
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
        return hitbox == parentedHitbox;
    }

    private void ToggleSlashTrails(bool enabled)
    {
        if(slashTrail)
        {
            if(enabled)
                slashTrail.Play(true);
            else
                slashTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void Awake()
    {
        collisionBuffer = new Collider[5];
        Attacking = false;
    }

    private void Update()
    {
        if(stopAttacking < Time.time)
        {
            Attacking = false;
        }

        if (Attacking)
        {
            int collisions = Physics.OverlapBoxNonAlloc(attackBox.transform.position + attackBox.center, attackBox.size / 2.0f, collisionBuffer, transform.rotation, attackMask);
            for (int i = 0; i < collisions; i++)
            {
                if (collisionBuffer[i].attachedRigidbody &&
                    collisionBuffer[i].attachedRigidbody.GetComponent<Hitbox>())
                {
                    Hitbox hitbox = collisionBuffer[i].attachedRigidbody.GetComponent<Hitbox>();
                    if (!alreadyHit.Contains(hitbox))
                    {
                        //Ignore the hitbox if its a weapon with the same parent
                        if (hitbox.GetComponent<Weapon>() && hitbox.GetComponent<Weapon>().CheckParentHitbox(parentedHitbox))
                            continue;

                        alreadyHit.Add(hitbox);

                        float reboundingForce = 
                            hitbox.HitPosition(
                            collisionBuffer[i], 
                            attack, 
                            collisionBuffer[i].ClosestPointOnBounds(attackBox.transform.position), 
                            collisionBuffer[i].transform);
                        
                        if (reboundingForce > 0)
                        {
                            print(name + " pinged off!");
                            Attacking = false;
                        }
                        else
                        {
                            print(name + " pierced through "+hitbox.name+"!");
                            Attacking = false;
                        }                        
                    }
                }
            }
        }
    }
}