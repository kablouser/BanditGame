using UnityEngine;
public class CharacterAnimation : MonoBehaviour
{
    private Animator animator;

    [Header("Animation Parameter Variables")]
    public string walkSpeed = "walkSpeed";
    public string dampWalkSpeed = "dampWalkSpeed";
    public string idlePose = "idlePose";
    public string grounded = "grounded";
    public string block = "block";
    public string lightCut = "lightCut";
    public string heavyCut = "heavyCut";
    public string jump = "jump";

    [Header("Settings")]
    public float idlePoseDamp = 0.5f;
    public float onGuardDuration = 3.0f;
    public float speedDampTime = 0.5f;

    private float nextOffGuard;
    private bool currentlyBlocking;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float value = IsOnGuard() ? 1 : 0;
        animator.SetFloat(idlePose, value, idlePoseDamp, Time.deltaTime);
    }

    private void UpdateOffGuard()
    {
        nextOffGuard = Time.time + onGuardDuration;
    }

    public bool IsOnGuard()
    {
        return Time.time < nextOffGuard || currentlyBlocking;
    }

    public void SetWalkSpeed(float speed)
    {
        animator.SetFloat(walkSpeed, speed);
        animator.SetFloat(dampWalkSpeed, speed, speedDampTime, Time.deltaTime);
    }

    public void SetGrounded(bool isGrounded)
    {
        animator.SetBool(grounded, isGrounded);
    }

    public void SetLeftBlock(bool blocking)
    {
        if (currentlyBlocking && !blocking)
            UpdateOffGuard();

        currentlyBlocking = blocking;
        animator.SetBool(block, blocking);
    }

    public void SetLightCut()
    {
        UpdateOffGuard();
        animator.SetTrigger(lightCut);
    }

    public void SetHeavyCut()
    {
        UpdateOffGuard();
        animator.SetTrigger(heavyCut);
    }

    public void SetJump()
    {
        animator.SetTrigger(jump);
    }

    public void SetTrigger(string trigger)
    {
        UpdateOffGuard();
        animator.SetTrigger(trigger);
    }
}
