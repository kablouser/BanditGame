using UnityEngine;
using System.Collections.Generic;

public class Weapon : Hitbox
{
    public float defence = 1.0f;
    public float attack = 1.0f;

    public BoxCollider attackBox;
    public BoxCollider levelCollider;
    public LayerMask attackMask;

    private bool attacking;
    private float stopAttacking;
    private Collider[] collisionBuffer;
    private List<Hitbox> alreadyHit;

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
            attacking = false;
        }
        else
        {
            print(name + " successfully block!");
        }

        return reboundingForce;
    }

    public void StartAttack(float duration, params Hitbox[] ignoreHits)
    {
        attacking = true;
        stopAttacking = Time.time + duration;
        alreadyHit = new List<Hitbox>(ignoreHits)
        {
            this
        };
    }

    public void EquipTo(Transform parent)
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
    }

    private void Awake()
    {
        collisionBuffer = new Collider[5];
    }

    private void Update()
    {
        if(stopAttacking < Time.time)
        {
            attacking = false;
        }

        if (attacking)
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
                        alreadyHit.Add(hitbox);

                        float reboundingForce = hitbox.Hit(collisionBuffer[i], attack);
                        if (reboundingForce > 0)
                        {
                            print(name + " pinged off!");
                            attacking = false;
                        }
                        else
                        {
                            print(name + " pierced through "+hitbox.name+"!");
                            attacking = false;
                        }
                    }
                }
            }
        }
    }
}