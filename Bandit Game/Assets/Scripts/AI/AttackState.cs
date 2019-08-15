using UnityEngine;
using AI;
using System;

public class AttackState : AIState
{
    public float attackRange = 1.8f;
    public RandomRange blockTime = new RandomRange(2.5f, 1f);
    public RandomRange attackTime = new RandomRange(1f, 1f);

    private bool attacking;
    private float nextSwitch;

    public AttackState (AIMovement movement) : base(movement)
    {
        this.movement = movement;
        attacking = UnityEngine.Random.Range(0, 1) > 0.5f;
    }

    public override Type Tick()
    {
        if(!movement.currentTarget)
        {
            //return investigate state
            return null;
        }
        
        movement.agent.SetDestination(movement.currentTarget.transform.position);
        if (movement.agent.remainingDistance < attackRange)
        {
            if(nextSwitch < Time.time)
            {
                if(attacking)
                {
                    nextSwitch = Time.time + blockTime.GenerateRandom();
                }
                else
                {
                    nextSwitch = Time.time + attackTime.GenerateRandom();
                    movement.SetBlock(false);
                }
                attacking = !attacking;
            }

            if (attacking)
            {                
                movement.SetLightCut();
            }
            else
            {                
                movement.SetBlock(true);
            }
        }

        return GetType();
    }
}
