﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyAI : MonoBehaviour
{
    EnemyUnit enemyUnit;
    // Start is called before the first frame update
    void Start()
    {
        enemyUnit = GetComponent<EnemyUnit>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public PlayerUnit FindClosestPlayerUnit()
    {
        int pathLength = 255;
        List<Node> path = new List<Node>();
        PlayerUnit closestUnit = null;

        Dijkstra.Instance.Clear();

        foreach (PlayerUnit playerUnit in PlayerUnitsController.Instance.units)
        {
            if (Dijkstra.Instance.GeneratePathTo(enemyUnit.currentNode, playerUnit.currentNode, 255).Count < pathLength)
            {
                path = Dijkstra.Instance.GeneratePathTo(enemyUnit.currentNode, playerUnit.currentNode, 255);
                pathLength = path.Count;
                closestUnit = playerUnit;
            }
        }

        return closestUnit;
    }

    public Unit GetPlayerUnitInShootRange()
    {
        foreach (Unit playerUnit in PlayerUnitsController.Instance.units)
        {
            Vector3 direction = playerUnit.raycastTarget.position - enemyUnit.gunbarrel.position;
            RaycastHit shootHit;
            Ray shootRay = new Ray(enemyUnit.gunbarrel.position, direction);

            Debug.DrawRay(shootRay.origin, shootRay.direction * 10, Color.red, 2f);

            RaycastHit[] hits = Physics.RaycastAll(shootRay, enemyUnit.equippedRangeWeapon.range).OrderBy(h => h.distance).ToArray();
            for (int j = 0; j < hits.Length; j++)
            {
                RaycastHit hit = hits[j];

                //If We hit something that stops the projectile, quit
                if (
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("Tile") ||
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle")
                    )
                {
                    return null;
                }

                //If We hit the player return it
                if (hit.collider.gameObject.tag == "Player")
                {
                    return hit.collider.GetComponent<Unit>();
                }
            }
        }
        return null;
    }

    public bool IsActionAvailable(string actionName)
    {
        List<Action> a = enemyUnit.actions.Where(h => h.actionName == actionName).ToList();
        return a.Count > 0;
    }

    public bool AreActionsAvailable()
    {
        return enemyUnit.actions.Count > 0;
    }



}
