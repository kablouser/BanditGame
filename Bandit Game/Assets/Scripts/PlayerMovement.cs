using UnityEngine;
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
    public float currentSpeed = 3f;
    public float maxSpeed = 6f;
    public float maxWalkForce = 60f;

    public float jumpHeight = 1f;
    public float rotateSpeed = 6f;

    public float attackDelay = 0.217f;
    public float attackDuration = 0.667f;
    public float attackComboWindow = 0.2f;

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

    private enum MovementState {walking, jumping, leftBlock, rightSwing};
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
            if ((movementState == MovementState.walking || movementState == MovementState.rightSwing)
                && nextAttack < Time.time)
            {
                if (Input.GetButtonDown(InputConstants.Fire2))
                {
                    if (regainMovementCoroutine != null)
                        StopCoroutine(regainMovementCoroutine);
                    regainMovementCoroutine = StartCoroutine(regainMovement(attackDuration));
                    movementState = MovementState.rightSwing;
                    characterAnimation.SetLightCut();
                    nextAttack = Time.time + attackDuration - attackComboWindow;
                    if (characterInventory.GetItem(rightHandSlot))
                    {
                        StartCoroutine(delayAttack());
                    }
                    //make sure velocity is 0.
                    rigid.velocity = new Vector3(0, rigid.velocity.y, 0);
                }
            }

            if (movementState == MovementState.walking && Input.GetButtonDown(InputConstants.JumpKey))
            {
                rigid.velocity = new Vector3(rigid.velocity.x, Mathf.Sqrt(2 * Physics.gravity.magnitude * jumpHeight), rigid.velocity.z);
                characterAnimation.SetJump();
            }

            if (movementState == MovementState.walking || movementState == MovementState.leftBlock)
            {
                bool button = Input.GetButton(InputConstants.Fire1);                 
                characterAnimation.SetLeftBlock(button);
                movementState = button ? MovementState.leftBlock : MovementState.walking;
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

    IEnumerator delayAttack()
    {
        yield return new WaitForSeconds(attackDelay);
        if(movementState == MovementState.rightSwing)
            characterInventory.GetItem(rightHandSlot).StartAttack(attackDuration - attackDelay, characterEntity);
    }

    IEnumerator regainMovement(float duration)
    {
        yield return new WaitForSeconds(duration);
        movementState = MovementState.walking;
    }
}
