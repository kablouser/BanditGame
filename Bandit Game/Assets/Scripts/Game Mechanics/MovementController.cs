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

    protected Rigidbody rigid;
    [System.Serializable]
    protected enum MovementState { walking, jumping, block, swordAttack, channelSpell };

    [Header("Controls")]
    [SerializeField]
    protected MovementState movementState;
    [SerializeField]
    protected Vector2 moveDirection;

    private Vector3 nextForward;
    private float nextAttack;

    private Collider[] jumpCollisionBuffer;
    private Coroutine regainMovementCoroutine;

    private bool wasWalking;
    private bool wasBlocking;
    private bool wasChannelling;

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
                blockWeapon.GetComponent<Shield>().blockAngle)
                return true;
        }
        blockWeapon = null;
        return false;
    }

    [ContextMenu("Sword Attack")]
    protected void SwordAttack()
    {
        if (characterInventory.GetSwordShield(out Sword sword, out _) && sword
        && (movementState == MovementState.walking || movementState == MovementState.swordAttack)
        && nextAttack < Time.time
        && characterEntity.SpendAttributes(0, 0, sword.staminaCost))
        {
            Sword.SwordAnimationData attackData = sword.attackData;

            movementState = MovementState.swordAttack;            
            nextAttack = Time.time + attackData.duration - attackData.comboWindow;

            characterAnimation.SetTrigger(attackData.animationTrigger);

            StopMovement(attackData.duration);
            sword.StartAttack(() => movementState == MovementState.swordAttack, characterEntity);
        }
    }

    protected void ToggleBlock(bool active)
    {
        if (characterInventory.GetSwordShield(out _, out Shield shield) && shield &&
            (movementState == MovementState.walking || movementState == MovementState.block))
        {
            wasBlocking = characterEntity.SpendAttributesOverTime(new Attribute(0, 0, shield.staminaCost), new Attribute(0, 0, shield.staminaDrain), active, wasBlocking, Time.deltaTime);

            characterAnimation.SetLeftBlock(wasBlocking);
            movementState = wasBlocking ? MovementState.block : MovementState.walking;
        }
    }

    [ContextMenu("Enable Block")]
    protected void EnableBlock()
    {
        ToggleBlock(true);
    }

    [ContextMenu("Disable Block")]
    protected void DisableBlock()
    {
        ToggleBlock(false);
    }

    protected void ChannelSpell(bool channelling)
    {
        if (characterInventory.GetBook(out MagicBook book) && book &&
        (movementState == MovementState.walking || movementState == MovementState.channelSpell))
        {
            wasChannelling = characterEntity.SpendAttributesOverTime(new Attribute(0, 0, book.manaCost), new Attribute(0, 0, book.manaDrain), channelling, wasChannelling, Time.deltaTime);

            characterAnimation.SetLeftBlock(wasBlocking);
            movementState = wasBlocking ? MovementState.block : MovementState.walking;
        }
    }

    protected void StopMovement(float duration)
    {
        rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
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
