using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class EnemyUnit : Unit
{
    public enum EnemyActionState { Patrolling, Attacking }

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
        while (currentActionPoints > 0)
        {
            //If a Player Unit is in Shoot Range, Shoot!
            PlayerUnit targetUnit = (PlayerUnit)GetPlayerUnitInShootRange();

            if (targetUnit != null)
            {
                Shoot(targetUnit);
                yield return new WaitForSeconds(1f);
            }

            //If not, find closest Player Unit
            else
            {
                Unit closestPlayerUnit = FindClosestPlayerUnit();

                //If we find a Player Unit, Set it as target and move towards it as close as possible
                if (closestPlayerUnit != null)
                {                    
                    currentPath = Dijkstra.Instance.GeneratePathTo(currentNode, closestPlayerUnit.currentNode, maxSteps);
                    yield return StartCoroutine(Move());
                }
            }

            //Decrement Action Points
            currentActionPoints--;
        }

        yield return new WaitForSeconds(1.5f);

        Debug.Log(transform.name + " ends move");
    }

    void Shoot(Unit targetUnit)
    {
        unitAnimation.PlayShootAnimation();
        targetUnit.healthController.Damage(equippedRangeWeapon.damage);
    }

    PlayerUnit FindClosestPlayerUnit()
    {
        int pathLength = 255;
        List<Node> path = new List<Node>();
        PlayerUnit closestUnit = null;

        Dijkstra.Instance.Clear();

        foreach (PlayerUnit playerUnit in PlayerUnitsController.Instance.units)
        {
            if (Dijkstra.Instance.GeneratePathTo(currentNode, playerUnit.currentNode, 255).Count < pathLength)
            {
                path = Dijkstra.Instance.GeneratePathTo(currentNode, playerUnit.currentNode, 255);
                pathLength = path.Count;
                closestUnit = playerUnit;
            }
        }

        return closestUnit;
    }


    IEnumerator Move()
    {
        unitAnimation.PlayMoveAnimation();   
        while (currentPath != null && currentPath.Count() > 1)
        {
            yield return StartCoroutine(MoveToNextTile());
        }
        unitAnimation.PlayIdleAnimation();
    }

    IEnumerator MoveToNextTile()
    {
        Vector3 direction = currentPath[1].transform.position - currentPath[0].transform.position;
        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, currentPath[1].transform.up);


        Debug.Log("Move To Next Tile");
        currentPath.RemoveAt(0);

        

        transform.rotation = currentPath[0].transform.rotation;
        transform.rotation = Quaternion.LookRotation(planarDirection, currentPath[0].transform.up);

        SetMoveDestination(currentPath[0].transform.position, 0.45f);

        

        currentNode.unitOnTile = null;
        currentNode = currentPath[0];
        currentNode.unitOnTile = this;

        if (currentPath.Count == 1)
        {            
            currentPath = null;
        }

        yield return new WaitForSeconds(0.5f);
    }

    Unit GetPlayerUnitInShootRange()
    {
        foreach (Unit playerUnit in PlayerUnitsController.Instance.units)
        {
            Vector3 direction = playerUnit.raycastTarget.position - gunbarrel.position;
            RaycastHit shootHit;
            Ray shootRay = new Ray(gunbarrel.position, direction);

            Debug.DrawRay(shootRay.origin, shootRay.direction * 10, Color.red, 2f);

            if (Physics.SphereCast(shootRay, 0.1f, out shootHit))
            {
                if (shootHit.collider.tag == "Player")
                {
                    Debug.Log("Player Hit!");
                    return shootHit.collider.GetComponent<Unit>();
                }
            }
        }
        return null;
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
                Destroy(this.gameObject);
                break;
        }

        unitState = state;
    }
}
