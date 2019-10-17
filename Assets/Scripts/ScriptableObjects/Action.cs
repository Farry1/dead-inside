using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action : ScriptableObject
{
    public string actionName = "Action";
    public float cooldown = 1f;
    public float AttackRange = 1f;

    public virtual void Execute()
    {
        Debug.Log("Base Action performed");
    }
}
