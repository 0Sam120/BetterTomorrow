using System;
using System.Collections;
using UnityEngine;

public class AnimatorProxy
{
    private readonly Animator animator;
    private readonly MonoBehaviour coroutineRunner;

    public AnimatorProxy(Animator animator, MonoBehaviour coroutineRunner)
    {
        this.animator = animator ?? throw new ArgumentNullException(nameof(animator));
        this.coroutineRunner = coroutineRunner ?? throw new ArgumentNullException(nameof(coroutineRunner));
    }

    public void WaitUntilAnimationStops(Action callback)
    {
        coroutineRunner.StartCoroutine(WaitRoutine(callback));
    }

    private IEnumerator WaitRoutine(Action callback)
    {
        // Wait until we enter a non-Idle state
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            yield return null;
        }

        // Get the current state hash
        int animHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

        // Wait until that state is no longer active
        while (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == animHash)
        {
            yield return null;
        }

        callback?.Invoke();
    }
}
