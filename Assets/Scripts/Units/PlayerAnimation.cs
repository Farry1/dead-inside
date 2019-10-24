using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator animator;


    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }


    void Update()
    {

    }

    public void PlayMoveAnimation()
    {
        animator.SetFloat("speed", 1);
    }

    public void PlayIdleAnimation()
    {
        animator.SetFloat("speed", 0);
    }

    public void PlayShootAnimation()
    {
        Debug.Log("Shoot animation!");
        animator.SetTrigger("shoot");
    }

    public IEnumerator TransitionToIdle()
    {
        yield return new WaitForSeconds(0.25f);
        PlayIdleAnimation();
    }
}
