using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float runSpeed;
    public float jumpHeight;

    public float rotateSpeed;

    public Camera3rdPerson cameraController;

    public Transform playerModel;

    public LayerMask jumpLayerMask;

    private Rigidbody rigid;
    //private float velocityY;
    private Vector3 nextForward;

    private Animator animator;

    public float animatonSmooth = 0.1f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        UpdateNextForward();
    }

    void Update()
    {
        bool isGrounded = IsGrounded();
        if (Input.GetButtonDown(InputConstants.JumpKey) && isGrounded)
        {
            rigid.velocity = new Vector3(rigid.velocity.x, jumpHeight, rigid.velocity.z);
        }

        float speedPercent = rigid.velocity.magnitude / runSpeed;
        animator.SetFloat("speedPercent", speedPercent, animatonSmooth, Time.deltaTime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float horizontal = Input.GetAxis(InputConstants.HorizontalAxis) * speed;
        float vertical = Input.GetAxis(InputConstants.VerticalAxis) * speed;

        if (horizontal != 0 || vertical != 0)
        {
            UpdateNextForward();
        }

        Vector3 move = new Vector3(0, rigid.velocity.y, 0);
        move += nextForward * vertical;
        move += Vector3.Cross(Vector3.up, nextForward) * horizontal; //cross(up, forward) = right

        rigid.AddForce(move - rigid.velocity, ForceMode.Impulse);
        
        //Physics.CheckCapsule()
        //transform.Translate()

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
