﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnit : Unit
{
    [HideInInspector] public GameObject relatedUIPanel;

    bool isPlayerTurn;



    protected override void Start()
    {
        base.Start();

    }

    protected override void Update()
    {
        base.Update();

        bool rightMouseUp = Input.GetMouseButtonUp(1);
        bool leftMouseUp = Input.GetMouseButtonUp(1);

        if (isPlayerTurn)
        {
            relatedUIPanel.transform.Find("TurnsText").GetComponent<Text>().text = currentActionPoints.ToString();
        }


        switch (unitState)
        {
            case UnitState.Idle:
                break;

            case UnitState.Selected:

                switch (actionState)
                {
                    case ActionState.None:
                        if (rightMouseUp)
                            PlayerUnitsController.Instance.UnselectSelectedPlayerUnits();

                        break;


                    case ActionState.MovePreparation:

                        DrawPath();
                        if (rightMouseUp)
                        {
                            SwitchActionState(ActionState.None);
                            currentPath = null;
                        }

                        break;

                    case ActionState.Moving:
                        break;

                    case ActionState.PreparingRangeAttack:
                        if (rightMouseUp)
                        {
                            SwitchActionState(ActionState.None);
                        }
                        break;
                }
                break;
            case UnitState.Dead:
                PlayerUnitsController.Instance.units.Remove(this);
                Destroy(this.gameObject);

                break;
        }
    }

    void OnDestroy()
    {
        StageUIController.Instance.UpdateUnitPanel();
        StageUIController.Instance.ClearPlayerActions();
    }

    IEnumerator MoveNextTile()
    {
        //Remove the old first node and move us to that position
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
            actionState = ActionState.None;
            StageUIController.Instance.playerMoveButton.interactable = true;
            if (unitState == UnitState.Selected && currentActionPoints > 0)
                Dijkstra.Instance.GetNodesInRange(currentNode, maxSteps);
        }

        yield return new WaitForSeconds(0.5f);
    }


    public void Move()
    {
        Dijkstra.Instance.Clear();
        if (actionState == ActionState.MovePreparation &&
            actionState != ActionState.Moving &&
            currentPath != null &&
            currentActionPoints > 0)
        {
            SwitchActionState(ActionState.Moving);
            StartCoroutine(MoveCoroutine());
            currentActionPoints--;
        }
    }


    IEnumerator MoveCoroutine()
    {


        while (currentPath != null && currentPath.Count() > 1)
        {
            yield return StartCoroutine(MoveNextTile());
        }
    }

    public void PrecalculatePathTo(Node target)
    {
        if (unitState == UnitState.Selected &&
            actionState == ActionState.MovePreparation &&
            currentActionPoints > 0)
        {
            currentPath = Dijkstra.Instance.GeneratePathTo(currentNode, target, maxSteps);
        }
    }

    public void Shoot(Node v)
    {
        if (actionState == ActionState.PreparingRangeAttack)
        {
            //Clear all previous highlighted fields 
            Dijkstra.Instance.Clear();

            //Highlight Target Node
            v.HighlightField(Color.yellow);

            //Calculate Recoid Direction and get the node, where the unit would land after a shot
            Vector3 recoilDirection = Node.GetOppositePlanarDirection(currentNode, v);
            Node recoilTarget = CalculateRecoilTarget(equippedRangeWeapon.recoil, recoilDirection);

            //If a valid recoil target is found, hightlight it blue
            if (recoilTarget != null)
                recoilTarget.HighlightField(Color.blue);


            //Get Unit on target tile
            Unit unitOntargetTile = v.unitOnTile;

            //If there is a unit check with a raycast if it's hit
            if (unitOntargetTile != null && UnitIsHitWithRaycast(unitOntargetTile, gunbarrel.position))
            {
                v.HighlightField(Color.red);
            }


            //If the mouse is clicked
            if (Input.GetMouseButtonUp(0))
            {
                //If we have a weapon and action points left
                if (equippedRangeWeapon != null && currentActionPoints > 0)
                {
                    //Shoot!                    
                    equippedRangeWeapon.Fire(currentNode, v);

                    if (unitOntargetTile != null &&
                        UnitIsHitWithRaycast(unitOntargetTile, gunbarrel.position))
                    {
                        unitOntargetTile.healthController.Damage(equippedRangeWeapon.damage);
                    }

                    currentActionPoints--;

                    //If we have a recoil target, move to that position
                    if (recoilTarget != null)
                    {
                        currentNode = recoilTarget;
                        StartCoroutine(MoveWithRecoil(recoilTarget));
                    }
                    //If not you fly over the edge and die in space
                    else
                    {
                        StartCoroutine(DieLonesomeInSpace(recoilDirection));
                    }
                }
            }
        }
    }

    IEnumerator DieLonesomeInSpace(Vector3 direction)
    {
        SetMoveDestination(direction * 5f, 2f);
        yield return new WaitForSeconds(2f);
        SwitchUnitState(UnitState.Dead);

    }

    private bool UnitIsHitWithRaycast(Unit unit, Vector3 start)
    {
        Vector3 direction = unit.raycastTarget.position - start;
        Ray shootRay = new Ray(gunbarrel.position, direction * 10);

        Debug.DrawRay(shootRay.origin, shootRay.direction, Color.red, 2f);

        RaycastHit shootHit;

        if (Physics.SphereCast(shootRay, 0.1f, out shootHit))
        {
            if (shootHit.collider.gameObject.GetComponent<Unit>() == unit)
            {
                Debug.Log("Hit!");
                return true;
            }
        }

        return false;
    }

    Node CalculateRecoilTarget(int steps, Vector3 recoilDirection)
    {
        Vector3 offset = currentNode.transform.up * 0.05f;
        Debug.Log(steps);
        Debug.DrawRay(currentNode.transform.position + offset, recoilDirection * 1.5f, Color.cyan);

        Node recoilNode = currentNode;

        for (int i = 0; i < steps; i++)
        {
            RaycastHit[] hits = Physics.RaycastAll(recoilNode.transform.position + offset, recoilDirection, 1.5f).OrderBy(h => h.distance).ToArray();
            for (int j = 0; j < hits.Length; j++)
            {
                RaycastHit hit = hits[j];

                Debug.Log(hit.collider.name);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Tile") ||
                    hit.collider.gameObject.tag == "Player" ||
                    hit.collider.gameObject.tag == "Enemy")
                {
                    Debug.Log("Tile Detected");
                    break;
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NavData"))
                {
                    if (hit.transform.tag == "Node")
                    {
                        Node n = hit.collider.GetComponent<Node>();
                        if (n != null)
                        {
                            recoilNode = n;
                            break;
                        }
                    }
                }
            }

            if (hits.Length == 0)
            {
                Debug.Log("Dead");
                return null;
            }
        }

        recoilNode.HighlightField(Color.red);
        return recoilNode;
    }

    IEnumerator MoveWithRecoil(Node recoilNode)
    {
        SetMoveDestination(recoilNode.transform.position, 0.5f);
        yield return new WaitForSeconds(0.5f);
        SwitchActionState(ActionState.None);
    }

    public override void SwitchActionState(ActionState a)
    {
        switch (a)
        {
            case ActionState.None:
                if (currentActionPoints > 0)
                {
                    Dijkstra.Instance.GetNodesInRange(currentNode, maxSteps);                    
                }
                PlayerUnitsController.Instance.lineRenderer.gameObject.SetActive(false);

                break;

            case ActionState.MovePreparation:
                if (currentActionPoints > 0)
                {
                    Dijkstra.Instance.GetNodesInRange(currentNode, maxSteps);
                    PlayerUnitsController.Instance.lineRenderer.gameObject.SetActive(true);
                }
                break;

            case ActionState.PreparingRangeAttack:
                Dijkstra.Instance.Clear();
                PlayerUnitsController.Instance.lineRenderer.gameObject.SetActive(false);
                break;

            case ActionState.Moving:
                PlayerUnitsController.Instance.lineRenderer.gameObject.SetActive(false);
                break;
        }

        actionState = a;
    }



    public override void OnSelect()
    {
        base.OnSelect();

        if (unitState != UnitState.Selected && isPlayerTurn)
        {
            PlayerUnitsController.Instance.UnselectSelectedPlayerUnits();
            PlayerUnitsController.Instance.SelectUnit(this);
            StageUIController.Instance.SetPlayerActionContainer(true);
            unitState = UnitState.Selected;

            StageUIController.Instance.CreatePlayerActionMenu(actions);
            // StageUIController.Instance.playerMoveButton.interactable = !moveActionAvailable;

            if (currentActionPoints > 0)
                Dijkstra.Instance.GetNodesInRange(currentNode, maxSteps);
        }
    }

    private void DrawPath()
    {
        if (currentPath != null)
        {
            //Draw Debug Path
            int currentNode = 0;

            while (currentNode < currentPath.Count - 1)
            {
                Vector3 start = currentPath[currentNode].transform.position;
                Vector3 end = currentPath[currentNode + 1].transform.position;
                Debug.DrawLine(start, end, Color.cyan);
                currentNode++;
            }

            //Draw actual Path
            List<Transform> transforms = new List<Transform>();

            foreach (Node v in currentPath)
            {
                transforms.Add(v.transform);
            }

            int seg = transforms.Count();
            Vector3[] vP = new Vector3[transforms.Count()];

            for (int i = 0; i < transforms.Count(); i++)
            {
                vP[i] = transforms[i].position;
            }
            for (int i = 0; i < seg; i++)
            {
                float t = i / (float)seg;
                PlayerUnitsController.Instance.lineRenderer.positionCount = seg;
                PlayerUnitsController.Instance.lineRenderer.SetPositions(vP);
            }
        }
    }


    public override void SwitchUnitState(UnitState state)
    {
        switch (state)
        {
            case UnitState.Dead:
                PlayerUnitsController.Instance.units.Remove(this);
                Destroy(this.gameObject);
                break;
        }

        unitState = state;
    }


    protected override void OnPlayerTurn()
    {
        base.OnPlayerTurn();
        isPlayerTurn = true;
    }

    protected override void OnEnemyTurn()
    {
        base.OnEnemyTurn();
        isPlayerTurn = false;
    }
}

