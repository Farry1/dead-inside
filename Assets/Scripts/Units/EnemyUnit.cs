using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class EnemyUnit : Unit
{

    public enum EnemyActionState { Patrolling, Attacking };

    EnemyAI enemyAI;


    protected override void Start()
    {
        base.Start();
        enemyAI = GetComponent<EnemyAI>();
    }


    protected override void Update()
    {
        base.Update();
    }

    /*
     * 
     * ENEMY AI
     * 
     */

    public IEnumerator MakeTurn()
    {
        Debug.Log(transform.name + " is doing a move!");

        //If we have Action Points Left
        while (currentActionPoints > 0 && enemyAI.AreActionsAvailable())
        {
            //If a Player Unit is in Shoot Range, Shoot!
            if (enemyAI.IsActionAvailable("Shoot"))
            {
                PlayerUnit targetUnit = (PlayerUnit)enemyAI.GetPlayerUnitInShootRange();

                if (targetUnit != null)
                {
                    //if Range Attack returns true, the recoil Target is a valid field, else the unit will probably die.
                    if (RangeAttack(targetUnit))
                        yield return new WaitForSeconds(1f);
                    else
                    {
                        yield return new WaitForSeconds(1f);
                        yield break;
                    }

                }
                //If not, find closest Player Unit
                else
                {
                    Unit closestPlayerUnit = enemyAI.FindClosestPlayerUnit();

                    //If we find a Player Unit, Set it as target and move towards it as close as possible
                    if (closestPlayerUnit != null)
                    {
                        currentPath = Dijkstra.Instance.GeneratePathTo(currentNode, closestPlayerUnit.currentNode, maxSteps);

                        yield return StartCoroutine(unitMovement.MoveCoroutine());
                    }
                }

                //Decrement Action Points
                currentActionPoints--;
            }
        }

        Debug.Log(transform.name + " is done with move");
    }

    bool RangeAttack(Unit targetUnit)
    {
        unitAnimation.PlayShootAnimation();
        targetUnit.healthController.Damage(equippedRangeWeapon.damage);
        transform.rotation = unitMovement.PlanarRotation(targetUnit.transform.position - currentNode.transform.position);
        GameObject shootprojectile = Instantiate(equippedRangeWeapon.projectile, gunbarrel.position, gunbarrel.rotation);

        //Calculate Recoid Direction and get the node, where the unit would land after a shot
        Vector3 recoilDirection = Node.GetOppositePlanarDirection(currentNode, targetUnit.currentNode);

        Node recoilTarget = unitMovement.CalculateRecoilTarget(equippedRangeWeapon.recoil, recoilDirection);

        if (recoilTarget != null)
        {
            unitMovement.SetMoveDestination(recoilTarget.transform.position, 0.45f);
            currentNode.unitOnTile = null;
            currentNode = recoilTarget;
            currentNode.unitOnTile = this;
            return true;
        }
        else
        {
            StartCoroutine(DieLonesomeInSpace(recoilDirection));
            return false;
        }
    }

    IEnumerator DieLonesomeInSpace(Vector3 direction)
    {
        //Todo: Change this to some shot and then die animation. But for now just normal recoil state.
        SwitchActionState(ActionState.Recoil);
        unitMovement.SetMoveDestination(direction * 5f, 2f);
        yield return new WaitForSeconds(2f);
        SwitchUnitState(UnitState.Dead);
    }


    /*
     * 
     * UNIT STATES
     * 
     */
    public override void SwitchUnitState(UnitState state)
    {
        switch (state)
        {
            case UnitState.Dead:
                EnemyUnitsController.Instance.enemyUnits.Remove(this);
                Destroy(projectedUnitUI);
                Destroy(this.gameObject);
                break;
        }

        unitState = state;
    }

    public override void SwitchActionState(ActionState a)
    {
        switch (a)
        {
            case ActionState.None:
                unitAnimation.PlayIdleAnimation();
                break;

            case ActionState.Moving:
                unitAnimation.PlayMoveAnimation();
                break;
        }
    }
}
