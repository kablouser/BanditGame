using UnityEngine;
public class PlayerMovement : MovementController
{
    protected override void Update()
    {
        if (Input.GetButtonDown(InputConstants.JumpKey))
            Jump();
        block = Input.GetButton(InputConstants.Fire1);
        if (Input.GetButtonDown(InputConstants.Fire2))
            LightCut();

        base.Update();
    }

    protected override void FixedUpdate()
    {
        moveDirection = new Vector2(Input.GetAxis(InputConstants.HorizontalAxis), Input.GetAxis(InputConstants.VerticalAxis));

        base.FixedUpdate();
    }
}
