using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveAction", menuName = "Actions/Player/Movement")]
public class PlayerMoveAction : PlayerAction
{
    PlayerUnit unit
    {
        get
        {
            return PlayerUnitsController.Instance.selectedPlayerUnit;
        }
    }


    public override void Execute()
    {
        unit.SwitchActionState(PlayerUnit.ActionState.MovePreparation);       
    }
}
