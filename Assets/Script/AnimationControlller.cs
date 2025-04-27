using UnityEngine;

public class AnimationControlller : MonoBehaviour
{
    private Animator animator;

    // Animation parameter hashes (more efficient than strings)
    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int AttackHash = Animator.StringToHash("Shoot");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void StartMoving()
    {
        animator.SetBool(MoveHash, true);
    }

    public void StopMoving()
    {
        animator.SetBool(MoveHash, false);
    }

    public void TriggerAttack()
    {
        // Only allow attack if not already attacking
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Shoot"))
        {
            animator.SetTrigger(AttackHash);
        }
    }

    // Called by Animation Event at the end of attack animation
    public void OnAttackComplete()
    {
        // Any cleanup needed after attack
    }
}
