using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveAction", menuName = "Actions/ShootAction")]
public class PlayerShootAction : Action
{
    // Start is called before the first frame update
    PlayerUnit unit
    {
        get
        {
            return PlayerUnitsController.Instance.selectedPlayerUnit;
        }
    }


    public override void Execute()
    {
        unit.SwitchActionState(PlayerUnit.PlayerActionState.Aiming);
    }
}
