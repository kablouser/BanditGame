using UnityEngine;
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
    public struct AttackTiming
    {
        public float damageDelay, duration, comboWindow;

        public AttackTiming(float damageDelay, float duration, float comboWindow)
        {
            this.damageDelay = damageDelay;
            this.duration = duration;
            this.comboWindow = comboWindow;
        }
    }

    public float currentSpeed = 3f;
    public float maxSpeed = 6f;
    public float maxWalkForce = 60f;

    public float jumpHeight = 1f;
    public float rotateSpeed = 6f;

    public AttackTiming lightCutTiming = new AttackTiming(0.217f, 0.667f, 0.2f);
    public AttackTiming heavyCutTiming = new AttackTiming(0.583f, 1, 0);

    public int leftHandSlot = 0;
    public int rightHandSlot = 1;    

    public Transform playerModel;
    public LayerMask jumpLayerMask;
    public CapsuleCollider levelCollider;

    public Camera3rdPerson cameraController;
    public CharacterAnimation characterAnimation;
    public Inventory characterInventory;
    public Entity characterEntity;
    
    private Rigidbody rigid;
    private Vector3 nextForward;
    private float nextAttack;

    private enum MovementState {walking, jumping, block, lightCut, heavyCut};
    private MovementState movementState;

    private Collider[] jumpCollisionBuffer;
    private Coroutine regainMovementCoroutine;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        UpdateNextForward(Vector2.up);
        jumpCollisionBuffer = new Collider[10];
    }

    void Update()
    {
        if (movementState == MovementState.jumping)
        {
            characterAnimation.SetLeftBlock(false);
        }
        else
        {
            Weapon leftHandWeapon = characterInventory.GetItem(leftHandSlot);
            AttackInputUpdate(MovementState.lightCut, InputConstants.Fire2, characterAnimation.SetLightCut, lightCutTiming, leftHandWeapon);
            AttackInputUpdate(MovementState.heavyCut, InputConstants.Fire3, characterAnimation.SetHeavyCut, heavyCutTiming, leftHandWeapon);

            if (movementState == MovementState.walking && Input.GetButtonDown(InputConstants.JumpKey))
            {
                rigid.velocity = new Vector3(rigid.velocity.x, Mathf.Sqrt(2 * Physics.gravity.magnitude * jumpHeight), rigid.velocity.z);
                characterAnimation.SetJump();
            }

            if (movementState == MovementState.walking || movementState == MovementState.block)
            {
                bool button = Input.GetButton(InputConstants.Fire1);                 
                characterAnimation.SetLeftBlock(button);
                movementState = button ? MovementState.block : MovementState.walking;
            }

            Vector3 horizontalVelocity = rigid.velocity;
            horizontalVelocity.y = 0;
            characterAnimation.SetWalkSpeed(horizontalVelocity.magnitude / maxSpeed);
        }        
    }

    void FixedUpdate()
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
        
        Vector2 inputDirection = new Vector2(Input.GetAxis(InputConstants.HorizontalAxis), Input.GetAxis(InputConstants.VerticalAxis));

        if (inputDirection.sqrMagnitude > 1)
        {
            inputDirection.Normalize();
        }
        inputDirection *= currentSpeed;
        if (inputDirection.sqrMagnitude != 0)
        {
            //Rotates the player model
            UpdateNextForward(inputDirection);
        }

        //Moves the player
        Vector3 move = new Vector3(0, rigid.velocity.y, 0);
        if (movementState == MovementState.walking || movementState == MovementState.jumping)
        {
            move += inputDirection.magnitude * nextForward;
        }
        Vector3 force = (move - rigid.velocity) / Time.fixedDeltaTime;
        if(force.sqrMagnitude > maxWalkForce * maxWalkForce)
        {
            force = force.normalized * maxWalkForce;
        }

        rigid.AddForce(force, ForceMode.Acceleration);

        playerModel.rotation = Quaternion.Lerp(playerModel.rotation, Quaternion.LookRotation(nextForward, Vector3.up), rotateSpeed * Time.fixedDeltaTime);
    }

    bool IsGrounded()
    {
        int results = Physics.OverlapSphereNonAlloc(transform.position + levelCollider.center - Vector3.up * (0.01f + levelCollider.height * 0.51f - levelCollider.radius), levelCollider.radius * 0.98f, jumpCollisionBuffer, jumpLayerMask);
        for(int i = 0; i < results; i++)
        {
            if(jumpCollisionBuffer[i].gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }

    void UpdateNextForward(Vector2 inputDirection)
    {
        if (inputDirection == Vector2.zero)
            inputDirection = Vector2.up;
        
        float angle = Vector2.Angle(Vector2.up, inputDirection);
        if (inputDirection.x <= 0)
            angle = 360 - angle;

        nextForward = Quaternion.Euler(0, angle, 0) * cameraController.cameraCenter.forward;
        nextForward.y = 0;
        nextForward.Normalize();
    }

    void AttackInputUpdate(MovementState attackState, string button, System.Action animationTrigger, AttackTiming attackTiming, Weapon useWeapon)
    {
        if ((movementState == MovementState.walking || movementState == attackState)
        && nextAttack < Time.time && Input.GetButtonDown(button))
        {
            if (regainMovementCoroutine != null)
                StopCoroutine(regainMovementCoroutine);
            regainMovementCoroutine = StartCoroutine(regainMovement(attackTiming.duration));

            movementState = attackState;
            animationTrigger();
            nextAttack = Time.time + attackTiming.duration - attackTiming.comboWindow;

            if (useWeapon)
            {
                StartCoroutine(delayAttack(attackState, attackTiming.damageDelay, attackTiming.duration - attackTiming.damageDelay, useWeapon));
            }
            //make sure velocity is 0.
            rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
        }
    }

    IEnumerator delayAttack(MovementState attackState, float delay, float damageDuration, Weapon useWeapon)
    {
        yield return new WaitForSeconds(delay);
        if(movementState == attackState)
            useWeapon.StartAttack(damageDuration, characterEntity);
    }

    IEnumerator regainMovement(float duration)
    {
        yield return new WaitForSeconds(duration);
        movementState = MovementState.walking;
    }
}
