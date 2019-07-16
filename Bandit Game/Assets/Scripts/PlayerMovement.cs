using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public float currentSpeed = 3f;
    public float maxSpeed = 6f;
    public float maxWalkForce = 60f;

    public float jumpSpeed = 5f;
    public float rotateSpeed = 6f;

    public float attackDuration = 1f;

    public Transform playerModel;
    public LayerMask jumpLayerMask;

    public Camera3rdPerson cameraController;
    public CharacterAnimation characterAnimation;
    
    private Rigidbody rigid;
    private Vector3 nextForward;
    private float nextAttack;

    private enum MovementState {walking, jumping, leftBlock, rightSwing};
    private MovementState movementState;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        UpdateNextForward(Vector2.up);
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
                movementState = MovementState.walking;
                if (Input.GetButtonDown(InputConstants.Fire2))
                {
                    movementState = MovementState.rightSwing;
                    characterAnimation.SetRightSwing();
                    nextAttack = Time.time + attackDuration;
                }
            }

            if (movementState == MovementState.walking && Input.GetButtonDown(InputConstants.JumpKey))
            {
                rigid.velocity = new Vector3(rigid.velocity.x, jumpSpeed, rigid.velocity.z);
            }

            if (movementState == MovementState.walking || movementState == MovementState.leftBlock)
            {
                bool button = Input.GetButton(InputConstants.Fire1);                 
                characterAnimation.SetLeftBlock(button);
                movementState = button ? MovementState.leftBlock : MovementState.walking;
            }

            Vector3 horizontalVelocity = rigid.velocity;
            horizontalVelocity.y = 0;
            characterAnimation.SetSpeedPercent(horizontalVelocity.magnitude / maxSpeed);
        }        
    }

    void FixedUpdate()
    {
        if(IsGrounded())
        {
            if (movementState == MovementState.jumping)
            {
                movementState = MovementState.walking;
                characterAnimation.SetIsGrounded(true);
            }
        }
        else
        {
            movementState = MovementState.jumping;
            characterAnimation.SetIsGrounded(false);
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
        if (Physics.Raycast(transform.position, Vector3.down, 1.01f, jumpLayerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
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
}
