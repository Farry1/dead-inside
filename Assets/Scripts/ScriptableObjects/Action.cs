using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action : ScriptableObject
{
    public string actionName = "Action";

    public virtual void Execute()
    {
        Debug.Log("Base Action performed");
    }
}
