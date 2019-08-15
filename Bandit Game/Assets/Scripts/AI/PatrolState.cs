using System;
using AI;
using UnityEngine;

public class PatrolState : AIState
{
    public RandomRange waitTime = new RandomRange(3f, 2f)
                    , walkRange = new RandomRange(4f, 2f);

    private float nextWalk;
    private bool walking;

    public PatrolState(AIMovement movement) : base(movement)
    {
        this.movement = movement;
    }

    public override Type Tick()
    {
        if(movement.currentTarget)
        {
            return typeof(AttackState);
        }

        if(movement.ReachedDestination())
        {
            if(walking)
            {
                walking = false;
                nextWalk = Time.time + waitTime.GenerateRandom();                
            }
            else if (nextWalk < Time.time)
            {
                walking = true;
                movement.useSpeed = 0.6f;
                movement.agent.SetDestination(
                    AIMovement.RandomNavSphere(
                        movement.transform.position, 
                        walkRange.GenerateRandom(), 
                        -1));
            }
        }

        return GetType();
    }
}
