using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float speed;
    public float jumpHeight;
    public float rotationSpeed;

    private Rigidbody rigid;

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
            rigid.velocity = new Vector3(0, jumpHeight, 0);
        }

        float mouseHorizontal = Input.GetAxis("Mouse X") * rotationSpeed;
        //float mouseVertical = Input.GetAxis("Mouse Y") * rotationSpeed;

        transform.Rotate(0, mouseHorizontal, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal") * speed;
        float vertical = Input.GetAxis("Vertical") * speed;

        Vector3 move = new Vector3(horizontal, rigid.velocity.y, vertical);

        rigid.velocity = move;
    }

    bool IsGrounded()
    {
        if(Physics.Raycast(transform.position, Vector3.down, 1.0f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
