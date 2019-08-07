using System.Collections;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float currentSpeed = 3f;
    public float maxSpeed = 6f;
    public float maxWalkForce = 60f;

    public float jumpHeight = 1f;
    public float rotateSpeed = 6f;

    public Transform playerModel;
    public LayerMask jumpLayerMask;
    public CapsuleCollider levelCollider;

    public Transform moveRelativeTo;
    public CharacterAnimation characterAnimation;
    public Inventory characterInventory;
    public Entity characterEntity;

    protected Rigidbody rigid;
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

            Vector3 horizontalVelocity = rigid.velocity;
            horizontalVelocity.y = 0;
            characterAnimation.SetWalkSpeed(horizontalVelocity.magnitude / maxSpeed);
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
        Vector3 force = (move - rigid.velocity) / Time.fixedDeltaTime;
        if (force.sqrMagnitude > maxWalkForce * maxWalkForce)
        {
            force = force.normalized * maxWalkForce;
        }

        rigid.AddForce(force, ForceMode.Acceleration);

        playerModel.rotation = Quaternion.Lerp(playerModel.rotation, Quaternion.LookRotation(nextForward, Vector3.up), rotateSpeed * Time.fixedDeltaTime);
    }

    protected void WeaponCut(MovementState attackState, int weaponSlot)
    {
        Weapon useWeapon = characterInventory.GetItem(weaponSlot);
        if (useWeapon
        && (movementState == MovementState.walking || movementState == attackState)
        && nextAttack < Time.time)
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
            animationToggle(active);
            movementState = active ? MovementState.block : MovementState.walking;
        }
    }

    protected void StopMovement(float duration)
    {
        if (regainMovementCoroutine != null)
            StopCoroutine(regainMovementCoroutine);
        regainMovementCoroutine = StartCoroutine(RegainMovement(duration));
    }

    [ContextMenu("Actions/Jump")]
    protected void Jump()
    {
        if (movementState == MovementState.walking)
        {
            rigid.velocity = new Vector3(rigid.velocity.x, Mathf.Sqrt(2 * Physics.gravity.magnitude * jumpHeight), rigid.velocity.z);
            characterAnimation.SetJump();
        }
    }

    [ContextMenu("Actions/Light Cut")]
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
