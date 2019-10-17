using System.Collections;
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
                break;
        }
    }



    IEnumerator MoveNextTile()
    {
        //Remove the old first node and move us to that position
        currentPath.RemoveAt(0);

        SetMoveDestination(currentPath[0].transform.position, 0.45f);

        //transform.position = currentPath[0].transform.position;
        transform.rotation = currentPath[0].transform.rotation;

        currentNode = currentPath[0];

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
            actionState = ActionState.Moving;
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
            Dijkstra.Instance.Clear();
            v.HighlightField(Color.red);

            Node recoilTarget;

            Node.GetOppositePlanarDirection(currentNode, v);

            if (Input.GetMouseButtonUp(0))
            {
                if (equippedRangeWeapon != null && currentActionPoints > 0)
                {
                    equippedRangeWeapon.Fire(currentNode, v, "Enemy");
                    currentActionPoints--;
                    actionState = ActionState.None;
                }
            }
        }
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
                Debug.Log("Turn Off");

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
            unitState = PlayerUnit.UnitState.Selected;

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

