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

    public IEnumerator MakeTurn()
    {
        Debug.Log(transform.name + " is doing a move!");

        while (currentActionPoints > 0)
        {
            PlayerUnit targetUnit = (PlayerUnit)GetPlayerUnitInShootRange();

            if (targetUnit != null)
            {
                Shoot(targetUnit);
                yield return new WaitForSeconds(1f);
            }
            else
            {
                Unit closestPlayerUnit = FindClosestPlayerUnit();

                if (closestPlayerUnit != null)
                {
                    Debug.Log("Set up Path");
                    currentPath = Dijkstra.Instance.GeneratePathTo(currentNode, closestPlayerUnit.currentNode, maxSteps);
                    yield return StartCoroutine(Move());
                    Debug.Log("Movement Done!");
                }

                yield return new WaitForSeconds(1f);
            }

            currentActionPoints--;
        }

        yield return new WaitForSeconds(1.5f);

        Debug.Log(transform.name + " ends move");
    }

    void Shoot(Unit targetUnit)
    {
        equippedRangeWeapon.Fire(currentNode, targetUnit.currentNode);
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
        while (currentPath != null && currentPath.Count() > 1)
        {
            yield return StartCoroutine(MoveToNextTile());
        }
    }

    IEnumerator MoveToNextTile()
    {
        Debug.Log("Move To Next Tile");
        currentPath.RemoveAt(0);

        SetMoveDestination(currentPath[0].transform.position, 0.45f);


        //transform.position = currentPath[0].transform.position;
        transform.rotation = currentPath[0].transform.rotation;

        currentNode.unitOnTile = null;
        currentNode = currentPath[0];
        currentNode.unitOnTile = this;

        if (currentPath.Count == 1)
        {
            //Next thingy in path would be our ultimate goal and we're standing on it. So make the path null to end this
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
}
