using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveAction", menuName = "Actions/Movement")]
public class PlayerMoveAction : Action
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
        unit.SwitchActionState(PlayerUnit.PlayerActionState.MoveSelection);
    }
}
