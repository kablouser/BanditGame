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

        UpdateNextForward();
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
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis(InputConstants.HorizontalAxis) * currentSpeed;
        float vertical = Input.GetAxis(InputConstants.VerticalAxis) * currentSpeed;

        if (horizontal != 0 || vertical != 0)
        {
            UpdateNextForward();
        }

        Vector3 move = new Vector3(0, rigid.velocity.y, 0);
        move += nextForward * vertical;
        move += Vector3.Cross(Vector3.up, nextForward) * horizontal; //cross(up, forward) = right

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

    void UpdateNextForward()
    {
        nextForward = cameraController.cameraCenter.forward;
        nextForward.y = 0;
        nextForward.Normalize();
    }
}
