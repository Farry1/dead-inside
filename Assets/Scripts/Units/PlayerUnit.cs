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
        bool leftMouseUp = Input.GetMouseButtonUp(0);

        //Todo: Make this more efficient and not in Update, Event after action and at begin of player turn.
        if (isPlayerTurn)
        {
            if (relatedUIPanel != null)
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
        StageUIController.Instance.ClearPlayerActionsPanel();
    }

    //Called After a click on a navigable Surface
    public void Move()
    {
        //If the current Action State allows it, move to that position and decrement an Action Point
        if (actionState == ActionState.MovePreparation &&
            actionState != ActionState.Moving &&
            currentPath != null &&
            currentActionPoints > 0)
        {
            SwitchActionState(ActionState.Moving);
            StartCoroutine(unitMovement.MoveCoroutine());
            currentActionPoints--;
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

    public void RangeAttack(Node v)
    {
        if (actionState == ActionState.PreparingRangeAttack)
        {
            //Clear all previous highlighted fields 
            Dijkstra.Instance.Clear();

            //Highlight Target Node
            v.HighlightField(Color.yellow);

            //Calculate Recoid Direction and get the node, where the unit would land after a shot
            Vector3 recoilDirection = Node.GetOppositePlanarDirection(currentNode, v);
            Node recoilTarget = unitMovement.CalculateRecoilTarget(equippedRangeWeapon.recoil, recoilDirection);

            //If a valid recoil target is found, hightlight it blue
            if (recoilTarget != null)
                recoilTarget.HighlightField(Color.blue);


            //Get Unit on target tile
            Unit unitOntargetTile = v.unitOnTile;

            //If there is a unit check with a raycast if it's hit
            if (unitOntargetTile != null && Calculations.UnitIsHitWithRaycast(unitOntargetTile, gunbarrel.position))
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

                    transform.rotation = unitMovement.PlanarRotation(v.transform.position - currentNode.transform.position);


                    if (unitOntargetTile != null &&
                        Calculations.UnitIsHitWithRaycast(unitOntargetTile, gunbarrel.position))
                    {
                        unitOntargetTile.healthController.Damage(equippedRangeWeapon.damage);
                    }

                    GameObject shootprojectile = Instantiate(equippedRangeWeapon.projectile, gunbarrel.position, gunbarrel.rotation);
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

    //Die!
    IEnumerator DieLonesomeInSpace(Vector3 direction)
    {
        //Todo: Change this to some shot and then die animation. But for now just normal recoil state.
        SwitchActionState(ActionState.Recoil);
        PlayerUnitsController.Instance.UnselectSelectedPlayerUnits();
        unitMovement.SetMoveDestination(direction * 5f, 2f);
        yield return new WaitForSeconds(2f);
        SwitchUnitState(UnitState.Dead);
    }



    IEnumerator MoveWithRecoil(Node recoilNode)
    {
        SwitchActionState(ActionState.Recoil);
        unitMovement.SetMoveDestination(recoilNode.transform.position, 0.5f);
        yield return new WaitForSeconds(0.5f);
        SwitchActionState(ActionState.None);
    }


    public override void OnSelect()
    {
        base.OnSelect();

        if (isPlayerTurn)
        {
            PlayerUnitsController.Instance.UnselectSelectedPlayerUnits();
            PlayerUnitsController.Instance.SelectUnit(this);
            StageUIController.Instance.SetPlayerActionContainer(true);
            SwitchUnitState(UnitState.Selected);

            StageUIController.Instance.CreatePlayerActionMenu(actions);
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

    /*
     * 
     * UNIT STATES
     * 
     */

    public override void SwitchUnitState(UnitState state)
    {
        switch (state)
        {
            case UnitState.Selected:
                Dijkstra.Instance.GetNodesInRange(currentNode, maxSteps);
                break;

            case UnitState.Dead:
                PlayerUnitsController.Instance.units.Remove(this);
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
                StartCoroutine(unitAnimation.TransitionToIdle());
                Dijkstra.Instance.Clear();
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
                unitAnimation.PlayMoveAnimation();
                PlayerUnitsController.Instance.lineRenderer.gameObject.SetActive(false);
                break;
            case ActionState.Recoil:
                actionState = ActionState.Recoil;
                unitAnimation.PlayShootAnimation();
                break;
        }

        actionState = a;
    }

    /*
     * 
     * Unit Events
     * 
     */
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

