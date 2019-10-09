using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnit : Unit
{
    public enum PlayerActionState { MoveSelection, Aiming, None, Moving }
    public PlayerActionState actionState;

    public List<Action> actions = new List<Action>();
    public GameObject relatedUIPanel;

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
                    case PlayerActionState.None:
                        if (rightMouseUp)
                            PlayerUnitsController.Instance.UnselectSelectedUnits();

                        break;


                    case PlayerActionState.MoveSelection:
                        if (rightMouseUp)
                        {
                            SwitchActionState(PlayerActionState.None);
                            currentPath = null;
                        }

                        break;

                    case PlayerActionState.Aiming:
                        if (rightMouseUp)
                        {
                            SwitchActionState(PlayerActionState.None);
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
            actionState = PlayerActionState.None;
            StageUIController.Instance.playerMoveButton.interactable = true;
            if (unitState == UnitState.Selected && currentActionPoints > 0)
                TileMap.Instance.Dijkstra(currentNode, maxSteps);
        }

        yield return new WaitForSeconds(0.5f);
    }


    public void Move()
    {
        TileMap.Instance.Clear();
        if (actionState == PlayerActionState.MoveSelection &&
            actionState != PlayerActionState.Moving &&
            currentPath != null &&
            currentActionPoints > 0)
        {
            actionState = PlayerActionState.Moving;
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
            actionState == PlayerActionState.MoveSelection &&
            currentActionPoints > 0)
        {
            currentPath = TileMap.Instance.GeneratePathTo(currentNode, target, maxSteps);
        }
    }

    public void Shoot(Node v)
    {
        if (actionState == PlayerActionState.Aiming)
        {
            TileMap.Instance.Clear();
            v.HighlightField(Color.red);

            if (Input.GetMouseButtonUp(0))
            {
                if (equippedWeapon != null && currentActionPoints > 0)
                {
                    equippedWeapon.Fire(currentNode, v, "Enemy");
                    currentActionPoints--;
                    actionState = PlayerActionState.None;
                }
            }
        }
    }

    public void SwitchActionState(PlayerActionState a)
    {
        switch (a)
        {
            case PlayerActionState.None:
                if (currentActionPoints > 0)
                {
                    TileMap.Instance.Dijkstra(currentNode, maxSteps);
                }
                PlayerUnitsController.Instance.lineRenderer.gameObject.SetActive(false);
                Debug.Log("Turn Off");
                
                break;

            case PlayerActionState.MoveSelection:
                if (currentActionPoints > 0)
                {
                    TileMap.Instance.Dijkstra(currentNode, maxSteps);
                    PlayerUnitsController.Instance.lineRenderer.gameObject.SetActive(true);
                }
                break;

            case PlayerActionState.Aiming:
                TileMap.Instance.Clear();
                break;
        }

        actionState = a;
    }

    public override void OnSelect()
    {
        if (unitState != UnitState.Selected && isPlayerTurn)
        {
            PlayerUnitsController.Instance.UnselectSelectedUnits();
            PlayerUnitsController.Instance.SelectUnit(this);
            StageUIController.Instance.SetPlayerActionContainer(true);
            unitState = PlayerUnit.UnitState.Selected;

            StageUIController.Instance.CreateActionMenu(actions);
            // StageUIController.Instance.playerMoveButton.interactable = !moveActionAvailable;

            if (currentActionPoints > 0)
                TileMap.Instance.Dijkstra(currentNode, maxSteps);
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

