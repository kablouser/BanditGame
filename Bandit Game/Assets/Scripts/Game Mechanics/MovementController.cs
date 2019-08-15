using System.Collections;
using UnityEngine;
using Attributes;

public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float currentSpeed = 6f;
    public float maxSpeed = 6f;
    public float maxWalkForce = 40f;

    public float jumpHeight = 1f;
    public float rotateSpeed = 6f;

    public Transform playerModel;
    public LayerMask jumpLayerMask;
    public CapsuleCollider levelCollider;

    public Transform moveRelativeTo;
    public CharacterAnimation characterAnimation;
    public Inventory characterInventory;
    public Entity characterEntity;

    [Header("Movement Stamina Costs")]    
    public float walkDrain = 0.5f;
    public float walkCost = 0f;
    public float jumpCost = 2f; //
    public float blockDrain = 1.4f;
    public float blockCost = 1.4f;
    public float attackCost = 2f; //

    protected Rigidbody rigid;
    [System.Serializable]
    protected enum MovementState { walking, jumping, block, lightCut, heavyCut };

    [Header("Controls")]
    [SerializeField]
    protected MovementState movementState;
    [SerializeField]
    protected Vector2 moveDirection;
    [SerializeField]
    protected bool block;

    private Vector3 nextForward;
    private float nextAttack;

    private Collider[] jumpCollisionBuffer;
    private Coroutine regainMovementCoroutine;

    private bool wasWalking;
    private bool wasBlocking;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        UpdateNextForward(Vector2.up);
        jumpCollisionBuffer = new Collider[10];
    }

    protected virtual void Update()
    {
        if (movementState == MovementState.jumping)
        {
            characterAnimation.SetLeftBlock(false);
        }
        else
        {
            ActionToggle(MovementState.block, block, characterAnimation.SetLeftBlock);

            if (movementState == MovementState.walking)
            {
                Vector3 horizontalVelocity = rigid.velocity;
                horizontalVelocity.y = 0;
                characterAnimation.SetWalkSpeed(horizontalVelocity.magnitude / maxSpeed);
            }
            else
            {
                characterAnimation.SetWalkSpeed(0);
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (IsGrounded())
        {
            if (movementState == MovementState.jumping)
            {
                movementState = MovementState.walking;
                characterAnimation.SetGrounded(true);
            }
        }
        else
        {
            movementState = MovementState.jumping;
            characterAnimation.SetGrounded(false);
        }

        if (moveDirection.sqrMagnitude > 1)
        {
            moveDirection.Normalize();
        }
        moveDirection *= currentSpeed;
        if (moveDirection.sqrMagnitude != 0)
        {
            //Rotates the model
            UpdateNextForward(moveDirection);
        }

        //Moves the object
        Vector3 move = new Vector3(0, rigid.velocity.y, 0);
        if (movementState == MovementState.walking || movementState == MovementState.jumping)
        {
            move += moveDirection.magnitude * nextForward;
        }

        wasWalking = characterEntity.SpendAttributesOverTime(new Attribute(0, 0, walkCost), new Attribute(0, 0, walkDrain), (move.x != 0 || move.z != 0), wasWalking, Time.fixedDeltaTime);
        if (!wasWalking)
        {
            move.x = move.z = 0;
        }

        Vector3 force = (move - rigid.velocity) / Time.fixedDeltaTime;
        if (force.sqrMagnitude > maxWalkForce * maxWalkForce)
        {
            force = force.normalized * maxWalkForce;
        }

        rigid.AddForce(force, ForceMode.Acceleration);

        playerModel.rotation = Quaternion.Lerp(playerModel.rotation, Quaternion.LookRotation(nextForward, Vector3.up), rotateSpeed * Time.fixedDeltaTime);
    }

    public bool BlockAttack(Vector3 attackerPosition, out Weapon blockWeapon)
    {
        if(movementState == MovementState.block)
        {
            blockWeapon = characterInventory.GetItem(0);
            print(Vector3.Angle(playerModel.forward, attackerPosition - transform.position));
            if(blockWeapon && 
                Vector3.Angle(playerModel.forward, attackerPosition - transform.position) 
                < 
                blockWeapon.blockAngle)                
                return true;
        }
        blockWeapon = null;
        return false;
    }

    protected void WeaponCut(MovementState attackState, int weaponSlot)
    {
        Weapon useWeapon = characterInventory.GetItem(weaponSlot);
        if (useWeapon
        && (movementState == MovementState.walking || movementState == attackState)
        && nextAttack < Time.time
        && characterEntity.SpendAttributes(0, 0, attackCost))
        {
            Weapon.AttackData attackData = useWeapon.attackData;

            movementState = attackState;            
            nextAttack = Time.time + attackData.duration - attackData.comboWindow;

            characterAnimation.SetTrigger(attackData.animationTrigger);

            rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
            StopMovement(attackData.duration);
            StartCoroutine(DelayAttack(attackState, attackData.damageDelay, attackData.duration - attackData.damageDelay, useWeapon));
        }
    }

    protected void ActionToggle(MovementState actionState, bool active, System.Action<bool> animationToggle)
    {
        if (movementState == MovementState.walking || movementState == actionState)
        {
            wasBlocking = characterEntity.SpendAttributesOverTime(new Attribute(0, 0, blockCost), new Attribute(0, 0, blockDrain), active, wasBlocking, Time.deltaTime);

            animationToggle(wasBlocking);
            movementState = wasBlocking ? MovementState.block : MovementState.walking;
        }
    }

    protected void StopMovement(float duration)
    {
        if (regainMovementCoroutine != null)
            StopCoroutine(regainMovementCoroutine);
        regainMovementCoroutine = StartCoroutine(RegainMovement(duration));
    }

    [ContextMenu("Jump")]
    protected void Jump()
    {
        if (movementState == MovementState.walking && characterEntity.SpendAttributes(0,0,jumpCost))
        {
            rigid.velocity = new Vector3(rigid.velocity.x, Mathf.Sqrt(2 * Physics.gravity.magnitude * jumpHeight), rigid.velocity.z);
            characterAnimation.SetJump();
        }
    }

    [ContextMenu("Light Cut")]
    protected void LightCut()
    {
        WeaponCut(MovementState.lightCut, 1);
    }

    IEnumerator DelayAttack(MovementState attackState, float delay, float damageDuration, Weapon useWeapon)
    {
        yield return new WaitForSeconds(delay);
        if (movementState == attackState)
            useWeapon.StartAttack(damageDuration, characterEntity);
    }

    IEnumerator RegainMovement(float duration)
    {
        yield return new WaitForSeconds(duration);
        movementState = MovementState.walking;
    }

    bool IsGrounded()
    {
        int results = 
            Physics.OverlapSphereNonAlloc(
            transform.position + levelCollider.center - Vector3.up * (0.01f + levelCollider.height * 0.51f - levelCollider.radius), 
            levelCollider.radius * 0.98f, jumpCollisionBuffer, jumpLayerMask);
        for (int i = 0; i < results; i++)
        {
            if (jumpCollisionBuffer[i].gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }

    void UpdateNextForward(Vector2 direction)
    {
        if (direction == Vector2.zero)
            direction = Vector2.up;

        float angle = Vector2.Angle(Vector2.up, direction);
        if (direction.x <= 0)
            angle = 360 - angle;

        if (moveRelativeTo)
            nextForward = Quaternion.Euler(0, angle, 0) * moveRelativeTo.forward;
        else
            nextForward = Quaternion.Euler(0, angle, 0) * Vector3.forward;

        nextForward.y = 0;
        nextForward.Normalize();
    }
}
