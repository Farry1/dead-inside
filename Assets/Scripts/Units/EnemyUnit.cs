using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class EnemyUnit : Unit
{

    EnemyAI enemyAI;

    public enum AttackType { Normal, Suicide }
    public AttackType attackType;

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
                    //Calculate Recoid Direction and get the node, where the unit would land after a shot
                    Vector3 recoilDirection = Node.GetOppositePlanarDirection(currentNode, targetUnit.currentNode);

                    Node recoilTarget = unitMovement.CalculatePushTarget(equippedRangeWeapon.recoilAmount, recoilDirection, targetUnit.currentNode, false);
                    Vector3 shootDirection = Node.GetPlanarDirection(currentNode, targetUnit.currentNode);

                    Node targetPushbackNode = targetUnit.unitMovement.CalculatePushTarget(equippedRangeWeapon.projectilePushAmount, shootDirection, currentNode, false);

                    ShootProjectile(recoilTarget, recoilDirection, targetUnit.raycastTarget.position, targetUnit.currentNode, targetUnit, shootDirection, targetPushbackNode);
                    transform.rotation = unitMovement.PlanarRotation(targetUnit.transform.position - currentNode.transform.position);
                    yield return new WaitForSeconds(1f);

                    //if Range Attack returns true, the recoil Target is a valid field, else the unit will probably die.
                    //if (RangeAttack(targetUnit))
                    //    yield return new WaitForSeconds(1f);
                    //else
                    //{
                    //    yield return new WaitForSeconds(1f);
                    //    yield break;
                    //}
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
        unitMovement.DestroyCollisionWarning();
        unitMovement.DestroyZeroGravityWarning();
    }

    //bool RangeAttack(Unit targetUnit)
    //{
    //    unitAnimation.PlayShootAnimation();
    //    targetUnit.healthController.Damage(equippedRangeWeapon.damage);
    //    transform.rotation = unitMovement.PlanarRotation(targetUnit.transform.position - currentNode.transform.position);
    //    GameObject shootprojectile = Instantiate(equippedRangeWeapon.shootFX, gunbarrel.position, gunbarrel.rotation);

    //    //Calculate Recoid Direction and get the node, where the unit would land after a shot
    //    Vector3 recoilDirection = Node.GetOppositePlanarDirection(currentNode, targetUnit.currentNode);

    //    Node recoilTarget = unitMovement.CalculatePushTarget(equippedRangeWeapon.recoilAmount, recoilDirection, targetUnit.currentNode);

    //    Vector3 shootDirection = Node.GetPlanarDirection(currentNode, targetUnit.currentNode);

    //    Node targetPushbackNode = targetUnit.unitMovement.CalculatePushTarget(equippedRangeWeapon.projectilePushAmount, shootDirection, currentNode);
    //    if (targetPushbackNode != null)
    //    {
    //        targetUnit.StartCoroutine(targetUnit.unitMovement.MoveWithPush(equippedRangeWeapon.projectilePushAmount, shootDirection));
    //    }
    //    else
    //    {
    //        targetUnit.StartCoroutine(targetUnit.unitMovement.DieLonesomeInSpace(shootDirection));
    //    }

    //    //If the recoil target is valid, move there
    //    if (recoilTarget != null)
    //    {
    //        unitMovement.ResetPreviousStoredValues();
    //        StartCoroutine(unitMovement.MoveWithPush(equippedRangeWeapon.recoilAmount, recoilDirection));
    //        return true;
    //    }
    //    //If the recoil target is not valid, die
    //    else
    //    {
    //        unitMovement.ResetPreviousStoredValues();
    //        StartCoroutine(unitMovement.DieLonesomeInSpace(recoilDirection));
    //        return false;
    //    }
    //}

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
                Die();
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
