using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float jumpHeight;
    public float rotateSpeed;

    public Camera3rdPerson cameraController;

    public Transform currentForward;
    public Transform playerModel;

    private Rigidbody rigid;
    //private float velocityY;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool isGrounded = IsGrounded();
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigid.velocity = new Vector3(rigid.velocity.x, jumpHeight, rigid.velocity.z);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal") * speed;
        float vertical = Input.GetAxis("Vertical") * speed;

        if (horizontal != 0 || vertical != 0)
        {
            Vector3 forward = cameraController.cameraCenter.forward;
            forward.y = 0;
            currentForward.rotation = Quaternion.LookRotation(forward);
        }

        Vector3 move = new Vector3();
        move.y = rigid.velocity.y;
        move += currentForward.forward * vertical;
        move += currentForward.right * horizontal;

        rigid.AddForce(move - rigid.velocity, ForceMode.Impulse);
        
        //Physics.CheckCapsule()
        //transform.Translate()

        playerModel.rotation = Quaternion.Lerp(playerModel.rotation, currentForward.rotation, rotateSpeed * Time.fixedDeltaTime);
    }

    bool IsGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 1.01f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
