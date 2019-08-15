using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Weapon : Hitbox
{
    [System.Serializable]
    public struct AttackData
    {
        public float damageDelay, duration, comboWindow;
        public string animationTrigger;
        public AttackData(float damageDelay, float duration, float comboWindow, string animationTrigger)
        {
            this.damageDelay = damageDelay;
            this.duration = duration;
            this.comboWindow = comboWindow;
            this.animationTrigger = animationTrigger;
        }
    }

    public float defence;
    public float blockAngle;
    public float attack;

    public BoxCollider attackBox;
    public BoxCollider levelCollider;
    public LayerMask attackMask;

    public ParticleSystem slashTrail;

    public AttackData attackData = new AttackData(0.217f, 0.667f, 0.2f, "lightCut");

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
    [SerializeField]
    private Hitbox parentedHitbox;

    private float currentAttackDamage;

    public Collider[] orderedCollisions;

    private Rigidbody rigid
    {
        get
        {
            return GetComponent<Rigidbody>();
        }
    }

    public override float Hit(Collider hitCollider, float incomingAttack)
    {
        float damage = CalulateDamage(incomingAttack, defence);
        if(damage == 0)
        {
            //print(name + " successfully block!");
        }
        else
        {
            //interrupt attack
            Attacking = false;
        }

        return damage;
    }

    public void StartAttack(float duration, params Hitbox[] ignoreHits)
    {
        Attacking = true;
        currentAttackDamage = attack;

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
        collisionBuffer = new Collider[10];
        Attacking = false;
    }

    public Collider[] TestShoot(Vector3 position)
    {
        Physics.OverlapBoxNonAlloc(position, attackBox.size / 2.0f, collisionBuffer, attackBox.transform.rotation, attackMask);
        return collisionBuffer;
    }

    private void Update()
    {
        if(stopAttacking < Time.time)
        {
            Attacking = false;
        }

        if (Attacking && currentAttackDamage > 0)
        {           
            int collisions = Physics.OverlapBoxNonAlloc(attackBox.transform.position + attackBox.center, attackBox.size / 2.0f, collisionBuffer, attackBox.transform.rotation, attackMask);
            orderedCollisions = collisionBuffer.OrderBy(collider => collider ? (collider.transform.position - attackBox.transform.position).sqrMagnitude : Mathf.Infinity).ToArray();
            for (int i = 0; i < collisions; i++)
            {
                Hitbox hitbox = null;
                if(orderedCollisions[i].GetComponent<RagdollHitbox>())
                {
                    hitbox = orderedCollisions[i].GetComponent<RagdollHitbox>().GetHitbox;
                }
                else if(orderedCollisions[i].attachedRigidbody)
                {
                    hitbox = orderedCollisions[i].attachedRigidbody.GetComponent<Hitbox>();
                }

                if (hitbox && !alreadyHit.Contains(hitbox))
                {
                    //Ignore the hitbox if its a weapon with the same parent
                    Weapon weapon = hitbox.GetComponent<Weapon>();
                    if (weapon && weapon.CheckParentHitbox(parentedHitbox))
                    {
                        continue;
                    }

                    alreadyHit.Add(hitbox);

                    float damageDealt =
                        hitbox.HitPosition(
                        orderedCollisions[i],
                        currentAttackDamage,
                        this);

                    Attacking = false;
                    break;
                }
            }
        }
    }
}