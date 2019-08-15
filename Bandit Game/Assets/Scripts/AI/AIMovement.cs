using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AI;

public class AIMovement : MovementController
{
    [Header("AI Settings")]
    public EntityTrigger entityTrigger;
    [Tooltip("Player is Team 0")]
    public int teamNumber = 1;
    public LayerMask entityMask;
    public float fieldOfView = 110f;
    [Range(0,1)]
    public float useSpeed = 1f;

    [Header("Sight")]
    public Entity currentTarget;
    public Vector3 lastSighting;

    [Header("Navigation")]
    public NavMeshAgent agent;

    public AIState currentState;
    public List<AIState> stateMachine;
    public event Action<AIState> OnStateChanged;

    public bool reachedDest;

    protected override void Awake()
    {
        if(!entityTrigger.blackList.Contains(characterEntity))
            entityTrigger.blackList.Add(characterEntity);

        if(agent)
        {
            agent.updatePosition = agent.updateRotation = false;
        }

        stateMachine = new List<AIState>()
        {
            new PatrolState(this),
            new AttackState(this)
        };

        base.Awake();
    }

    protected override void FixedUpdate()
    {
        reachedDest = ReachedDestination();

        if (entityTrigger.updated)
        {
            entityTrigger.updated = false;
        }

        currentTarget = null;
        float currentTargetDistance = 0;
        foreach (Entity entity in entityTrigger.enteredList)
        {
            if (InVision(entity.transform))
            {
                float newDistance = (entity.transform.position - transform.position).sqrMagnitude;
                if (currentTarget == null || newDistance < currentTargetDistance)
                {
                    currentTarget = entity;
                    currentTargetDistance = newDistance;
                }
            }
        }

        if(currentTarget)
        {
            lastSighting = currentTarget.transform.position;
        }

        if(currentState == null)
        {
            if(stateMachine.Count > 0)
                currentState = stateMachine[0];
        }
        if (currentState != null)
        {
            Type nextState = currentState.Tick();
            if (nextState != currentState.GetType())
            {
                OnStateChanged?.Invoke(currentState);
                agent.isStopped = true;
                SetBlock(false);
                currentState = stateMachine.Find(x => x.GetType() == nextState);
            }
        }

        if (ReachedDestination())
        {
            moveDirection = Vector2.zero;
        }
        else
        {
            agent.nextPosition = transform.position;
            Vector3 direction = agent.steeringTarget - transform.position;
            useSpeed = Mathf.Clamp(useSpeed, 0, 1);
            moveDirection = new Vector2(direction.x, direction.z);
            if(moveDirection.sqrMagnitude > 1)
            {
                moveDirection = moveDirection.normalized * useSpeed;
            }
        }

        base.FixedUpdate();
    }

    public bool InVision(Transform target)
    {
        Ray ray = new Ray(transform.position + levelCollider.center, target.position - (transform.position + levelCollider.center));

        if (Vector3.Angle(playerModel.forward, ray.direction) <= fieldOfView && Physics.Raycast(ray, out RaycastHit raycastHit, entityTrigger.GetRange, entityMask))
        {
            return raycastHit.transform == target;
        }
        return false;
    }

    public bool ReachedDestination()
    {
        if (agent.pathPending)
        {
            return false;
        }
        else
        {
            if (agent.hasPath)
            {
                return agent.remainingDistance < 0.05f;
            }
            else
            {
                return true;
            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;

        randomDirection += origin;

        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, distance, layermask);
        return navHit.position;
    }

    public void SetBlock(bool isBlocking)
    {
        block = isBlocking;
    }

    public void SetJump()
    {
        Jump();
    }

    public void SetLightCut()
    {
        LightCut();
    }
}
