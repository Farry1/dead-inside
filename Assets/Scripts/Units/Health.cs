using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int health;
    Unit unit;
    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<Unit>();   
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Damage(int amount)
    {
        health -= amount;
        CheckLifeSigns();
    }

    public void Add(int amount)
    {
        health += amount;
    }

  

    void CheckLifeSigns()
    {
        if (health <= 0)
        {
            unit.SwitchUnitState(Unit.UnitState.Dead);
        }
    }
}
