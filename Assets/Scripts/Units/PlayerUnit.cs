using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnit : Unit
{
    [HideInInspector] public GameObject relatedUIPanel;
    bool isPlayerTurn;

    public delegate void ActionStateChange();
    public static event ActionStateChange OnActionStateNone;
    public static event ActionStateChange OnActionStateMovementPreparation;

    public delegate void NodeAction();
    public static event NodeAction OnHoveredNodeChanged;
    public static event NodeAction OnNodeClicked;

    public GameObject validCrosshairPrefab;
    public GameObject invalidCrosshairPrefab;
    private GameObject crosshairInstance = null;




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


    void TestFunction()
    {
        Debug.Log("Test!");
    }

    Node previousHoveredNode = null;
    Unit previousHoveredUnit = null;

    //Todo: Move shooting to a separate class
    public void RangeAttack(Node v)
    {
        float distance = Vector3.Distance(currentNode.transform.position, v.transform.position);

        if (actionState == ActionState.PreparingRangeAttack)
        {
            //If target is in shoot range and target node is not the node we're currently on
            if (distance < equippedRangeWeapon.range && v != currentNode)
            {
                //Clear all previous highlighted fields 
                Dijkstra.Instance.Clear();

                //Get Unit on target tile
                Unit unitOnTargetTile = v.unitOnTile;

                bool validTarget = false;

                //If there is a unit check with a raycast if it's hit
                if (unitOnTargetTile != null && Calculations.UnitIsHitWithRaycast(unitOnTargetTile, gunbarrel.position))
                {
                    v.HighlightField(Color.red);
                    validTarget = true;
                }

                //If No Unit, check if the Node is Hit
                else if (Calculations.NodeIsHitWithRaycast(v, this))
                {
                    v.HighlightField(Color.green);
                    validTarget = true;
                }
                else
                {
                    validTarget = false;
                    v.HighlightField(Color.black);
                }


                if (previousHoveredNode != v)
                {
                    OnHoveredNodeChanged();
                    DisableCrosshair();
                    previousHoveredNode = v;

                    if (validTarget)
                        crosshairInstance = Instantiate(validCrosshairPrefab, v.transform.position, v.transform.rotation);
                    else
                        crosshairInstance = Instantiate(invalidCrosshairPrefab, v.transform.position, v.transform.rotation);
                }

                if (validTarget)
                {
                    //Calculate Recoil Direction and get the node, where the unit would land after a shooting
                    Vector3 recoilDirection = Node.GetPlanarDirection(v, currentNode);

                    Debug.DrawRay(currentNode.transform.position, recoilDirection, Color.cyan, 2f);

                    Node recoilTarget = unitMovement.CalculatePushTarget(equippedRangeWeapon.recoilAmount, recoilDirection, v);

                    if (recoilTarget != null)
                    {
                        Debug.Log("Recoil Target Found! " + recoilTarget.name);
                        recoilTarget.HighlightField(Color.cyan);
                    }

                    if (unitOnTargetTile != null)
                    {
                        Debug.Log("Unit on Target: " + unitOnTargetTile.name);
                        Node unitTargetNode = unitOnTargetTile.unitMovement.CalculatePushTarget(equippedRangeWeapon.projetilePushAmount, Node.GetPlanarDirection(currentNode, unitOnTargetTile.currentNode), v);
                        if (unitTargetNode != null)
                            unitTargetNode.HighlightField(Color.cyan);
                        previousHoveredUnit = unitOnTargetTile;
                    }

                    //If the mouse is clicked
                    if (Input.GetMouseButtonUp(0))
                    {

                        //If we have a weapon and action points left
                        if (equippedRangeWeapon != null && currentActionPoints > 0)
                        {
                            //Shoot!   
                            transform.rotation = unitMovement.PlanarRotation(v.transform.position - currentNode.transform.position);

                            //If we hit a unit on a target Tile
                            if (unitOnTargetTile != null)
                            {
                                Node unitTargetNode = unitOnTargetTile.unitMovement.CalculatePushTarget(equippedRangeWeapon.projetilePushAmount, Node.GetPlanarDirection(currentNode, v), v);


                                unitOnTargetTile.healthController.Damage(equippedRangeWeapon.damage);
                                ShootProjectile(recoilTarget, recoilDirection);

                                if (unitTargetNode != null)
                                {
                                    unitOnTargetTile.unitMovement.StartCoroutine(unitOnTargetTile.unitMovement.MoveWithPush(equippedRangeWeapon.projetilePushAmount, Node.GetPlanarDirection(currentNode, v)));
                                }
                                else
                                {
                                    StartCoroutine(unitOnTargetTile.unitMovement.DieLonesomeInSpace(Node.GetPlanarDirection(currentNode, v)));
                                }
                            }
                            else
                            {
                                if (Calculations.NodeIsHitWithRaycast(v, this))
                                {
                                    ShootProjectile(recoilTarget, recoilDirection);
                                }
                            }
                        }
                        OnNodeClicked();
                    }
                }
            }
            else // If Not in Shoot Range
            {

                //Clear all previous highlighted fields 
                Dijkstra.Instance.Clear();

                //Highlight Target Node
                v.HighlightField(Color.black);
            }
        }
    }

    void DisableCrosshair()
    {
        if (crosshairInstance != null)
        {
            Destroy(crosshairInstance);
        }
    }

    private void ShootProjectile(Node recoilTarget, Vector3 recoilDirection)
    {
        GameObject shootprojectile = Instantiate(equippedRangeWeapon.projectile, gunbarrel.position, gunbarrel.rotation);
        currentActionPoints--;

        //If we have a recoil target, move to that position
        if (recoilTarget != null)
        {
            StartCoroutine(unitMovement.MoveWithPush(equippedRangeWeapon.recoilAmount, recoilDirection));
        }
        //If not you fly over the edge and die in space
        else
        {
            StartCoroutine(unitMovement.DieLonesomeInSpace(recoilDirection));
        }
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
                Dijkstra.Instance.Clear();
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
                OnActionStateNone();
                DisableCrosshair();
                StartCoroutine(unitAnimation.TransitionToIdle());
                Dijkstra.Instance.Clear();
                if (currentActionPoints > 0)
                {
                    Dijkstra.Instance.GetNodesInRange(currentNode, maxSteps);
                }
                PlayerUnitsController.Instance.lineRenderer.gameObject.SetActive(false);

                break;

            case ActionState.MovePreparation:
                OnActionStateMovementPreparation();
                if (currentActionPoints > 0)
                {
                    Dijkstra.Instance.Clear();
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
                Dijkstra.Instance.Clear();
                DisableCrosshair();
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

