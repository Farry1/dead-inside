using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimation : MonoBehaviour
{
    Animator animator;


    void Start()
    {
        
        animator = GetComponentInChildren<Animator>();
        PlayIdleAnimation();
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

        float randomOffset = Random.Range(0.0f, 1.0f);
        animator.SetFloat("randomOffset", randomOffset);      
        animator.SetFloat("speed", 0);
    }

    public void PlayShootAnimation()
    {
        Debug.Log("Shoot animation!");
        animator.SetTrigger("shoot");
    }

    public IEnumerator TransitionToIdle()
    {
        yield return new WaitForSeconds(0.1f);
        PlayIdleAnimation();
    }
}
