using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveAction", menuName = "Actions/Player/Movement")]
public class PlayerMoveAction : Action
{
    PlayerUnit unit
    {
        get
        {
            return UnitsManager.Instance.selectedUnit.GetComponent<PlayerUnit>();
        }
    }


    public override void Execute()
    {
        unit.SwitchActionState(PlayerUnit.ActionState.MovePreparation);
    }
}
