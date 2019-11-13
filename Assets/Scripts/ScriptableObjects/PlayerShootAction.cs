using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveAction", menuName = "Actions/ShootAction")]
public class PlayerShootAction : Action
{    
    Unit unit
    {
        get
        {
            if (StageManager.Instance.stageState == StageManager.StageState.PlayerTurn)
                return PlayerUnitsController.Instance.selectedUnit;
            else
                return null;
        }
    }


    public override void Execute()
    {
        if (unit != null)
            unit.SwitchActionState(Unit.ActionState.PreparingRangeAttack);        
    }
}
