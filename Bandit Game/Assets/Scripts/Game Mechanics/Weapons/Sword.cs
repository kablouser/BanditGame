using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Sword : Weapon
{
    [System.Serializable]
    public struct SwordAnimationData
    {
        public float damageDelay, duration, comboWindow;
        public string animationTrigger;
        public SwordAnimationData(float damageDelay, float duration, float comboWindow, string animationTrigger)
        {
            this.damageDelay = damageDelay;
            this.duration = duration;
            this.comboWindow = comboWindow;
            this.animationTrigger = animationTrigger;
        }
    }

    [Header("Sword")]
    public BoxCollider attackBox;
    public LayerMask attackMask;

    public ParticleSystem slashTrail;

    public SwordAnimationData attackData = new SwordAnimationData(0.217f, 0.667f, 0.2f, "lightCut");

    public float attack;
    public float staminaCost;

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

    private Rigidbody rigid
    {
        get
        {
            return GetComponent<Rigidbody>();
        }
    }

    public void StartAttack(System.Func<bool> finalCheck, params Entity[] ignoreEntities)
    {
        StartCoroutine(AttackRoutine(finalCheck, ignoreEntities));
    }

    protected virtual void ToggleSlashTrails(bool enabled)
    {
        if (slashTrail)
        {
            if (enabled)
                slashTrail.Play(true);
            else
                slashTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    protected virtual void Awake()
    {
        collisionBuffer = new Collider[10];
        Attacking = false;
    }

    protected virtual void Update()
    {
        if (stopAttacking < Time.time)
        {
            Attacking = false;
        }

        if (Attacking && attackBox)
        {
            int collisions = Physics.OverlapBoxNonAlloc(attackBox.transform.position + attackBox.center, attackBox.size / 2.0f, collisionBuffer, attackBox.transform.rotation, attackMask);

            Collider[] orderedCollisions = 
                collisionBuffer.OrderBy
                (
                    collider => collider 
                    ? 
                    (collider.transform.position - attackBox.transform.position).sqrMagnitude 
                    : 
                    Mathf.Infinity
                ).ToArray();

            for (int i = 0; i < collisions; i++)
            {
                Hitbox hitbox = null;
                if (orderedCollisions[i].GetComponent<RagdollHitbox>())
                {
                    hitbox = orderedCollisions[i].GetComponent<RagdollHitbox>().GetHitbox;
                }
                else if (orderedCollisions[i].attachedRigidbody)
                {
                    hitbox = orderedCollisions[i].attachedRigidbody.GetComponent<Hitbox>();
                }

                if (hitbox && !alreadyHit.Contains(hitbox))
                {
                    //Ignore the hitbox if its a weapon with the same parent
                    Weapon weapon = hitbox.GetComponent<Weapon>();
                    if (weapon && weapon.CheckParentHitbox(GetParentedHitbox))
                    {
                        continue;
                    }

                    alreadyHit.Add(hitbox);

                    float damageDealt =
                        hitbox.HitPosition(
                        orderedCollisions[i],
                        attack,
                        this);

                    Attacking = false;
                    break;
                }
            }
        }
    }
    IEnumerator AttackRoutine(System.Func<bool> finalCheck, params Entity[] ignoreEntities)
    {
        yield return new WaitForSeconds(attackData.damageDelay);
        if (finalCheck())
        {
            Attacking = true;

            stopAttacking = Time.time + attackData.duration;
            alreadyHit = new List<Hitbox>(ignoreEntities)
            {
                this
            };
        }
    }
}
