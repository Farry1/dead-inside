using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyUnit : Unit
{
    public enum EnemyActionState { Patrolling, Attacking }


    protected override void Start()
    {
        base.Start();
    }


    protected override void Update()
    {

    }

    public override void SwitchUnitState(UnitState state)
    {
        switch (state)
        {
            case UnitState.Dead:
                EnemyUnitsController.Instance.enemyUnits.Remove(this);
                Destroy(this.gameObject);
                break;
        }

        unitState = state;
    }


}
