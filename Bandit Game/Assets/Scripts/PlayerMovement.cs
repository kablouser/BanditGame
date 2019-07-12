using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public float currentSpeed = 3f;
    public float maxSpeed = 6f;
    public float maxWalkForce = 60f;

    public float jumpSpeed = 5f;
    public float rotateSpeed = 6f;
    public float animationSmooth = 0.1f;

    public string animationSpeedPercent = "speedPercent";
    public string animationGrounded = "isGrounded";

    public Camera3rdPerson cameraController;
    public Transform playerModel;
    public LayerMask jumpLayerMask;
    public Animator animator;

    private Rigidbody rigid;
    private Vector3 nextForward;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();

        UpdateNextForward(Vector2.up);
    }

    void Update()
    {
        bool isGrounded = IsGrounded();
        if (Input.GetButtonDown(InputConstants.JumpKey) && isGrounded)
        {
            rigid.velocity = new Vector3(rigid.velocity.x, jumpSpeed, rigid.velocity.z);
        }

        Vector3 horizontalVelocity = rigid.velocity;
        horizontalVelocity.y = 0;
        float speedPercent = horizontalVelocity.magnitude / maxSpeed;
        animator.SetFloat(animationSpeedPercent, speedPercent, animationSmooth, Time.deltaTime);
        animator.SetBool(animationGrounded, isGrounded);

        if (Input.GetButtonDown("Fire1"))
        {
            animator.SetTrigger("RightSwing");
        }

        if (Input.GetButtonDown("Fire2"))
        {
            animator.SetBool("LeftBlock", true);
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            animator.SetBool("LeftBlock", false);
        }
    }

    void FixedUpdate()
    {
        Vector2 inputDirection = new Vector2(Input.GetAxis(InputConstants.HorizontalAxis), Input.GetAxis(InputConstants.VerticalAxis));
        if (inputDirection.sqrMagnitude > 1)
        {
            inputDirection.Normalize();
        }
        inputDirection *= currentSpeed;

        if (inputDirection.sqrMagnitude != 0)
        {
            UpdateNextForward(inputDirection);
        }

        Vector3 move = new Vector3(0, rigid.velocity.y, 0);
        move += inputDirection.magnitude * nextForward;

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
