using UnityEngine;
public class CharacterAnimation : MonoBehaviour
{
    private Animator animator;

    [Header("Animation Parameter Variables")]
    public string speedPercent = "speedPercent";
    public string idlePosition = "idlePosition";
    public string isGrounded = "isGrounded";
    public string leftBlock = "leftBlock";
    public string rightSwing = "rightSwing";

    [Header("Settings")]
    public float idlePositionDampTime = 0.5f;
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
        animator.SetFloat(idlePosition, value, idlePositionDampTime, Time.deltaTime);
    }

    private void UpdateOffGuard()
    {
        nextOffGuard = Time.time + onGuardDuration;
    }

    public bool IsOnGuard()
    {
        return Time.time < nextOffGuard || currentlyBlocking;
    }

    public void SetSpeedPercent(float speedPercent)
    {
        animator.SetFloat(this.speedPercent, speedPercent, speedDampTime, Time.deltaTime);
    }    

    public void SetIsGrounded(bool isGrounded)
    {
        animator.SetBool(this.isGrounded, isGrounded);
    }

    public void SetLeftBlock(bool blocking)
    {
        if (currentlyBlocking && !blocking)
            UpdateOffGuard();

        currentlyBlocking = blocking;
        animator.SetBool(leftBlock, blocking);
    }

    public void SetRightSwing()
    {
        UpdateOffGuard();
        animator.SetTrigger(rightSwing);
    }
}
