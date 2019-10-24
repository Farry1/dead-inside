using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerAnimation))]
public class PlayerUnit : Unit
{
    [HideInInspector] public GameObject relatedUIPanel;
    bool isPlayerTurn;

    PlayerAnimation playerAnimation;

    protected override void Start()
    {
        base.Start();
        playerAnimation = GetComponent<PlayerAnimation>();
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
            StartCoroutine(MoveCoroutine());
            currentActionPoints--;
        }
    }

    //Initiate The Actual Movement
    IEnumerator MoveCoroutine()
    {
        //As long as we have avalid path that has more than one entry left, Move to that Position
        while (currentPath != null && currentPath.Count() > 1)
        {
            //Wait until movement to next tile is done
            yield return StartCoroutine(MoveNextTile());
        }
    }

    //Move To the next Tile
    IEnumerator MoveNextTile()
    {
        //Set the Rotation
        //Todo: transfer rotation and movement to animator script
        //
        Vector3 direction = currentPath[1].transform.position - currentPath[0].transform.position;
        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, currentPath[1].transform.up);

        Debug.Log(direction);
        Debug.DrawRay(currentNode.transform.position, direction, Color.red, 2f);

        //Remove the old first node and move us to that position
        currentPath.RemoveAt(0);

        //Set Destination and move over time, also set Rotation to the rotation of target Node
        SetMoveDestination(currentPath[0].transform.position, 0.45f);

        transform.rotation = currentPath[0].transform.rotation;
        transform.rotation = Quaternion.LookRotation(planarDirection, currentPath[0].transform.up);

        //Reset Nodes
        currentNode.unitOnTile = null;
        currentNode = currentPath[0];
        currentNode.unitOnTile = this;


        if (currentPath.Count == 1)
        {
            //Next thingy in path would be our ultimate goal and we're standing on it. So make the path null to end this
            currentPath = null;
            SwitchActionState(ActionState.None);

            StageUIController.Instance.playerMoveButton.interactable = true;
            if (unitState == UnitState.Selected && currentActionPoints > 0)
                Dijkstra.Instance.GetNodesInRange(currentNode, maxSteps);


        }

        yield return new WaitForSeconds(0.5f);
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
                    equippedRangeWeapon.Fire(currentNode, v);


                    if (unitOntargetTile != null &&
                        Calculations.UnitIsHitWithRaycast(unitOntargetTile, gunbarrel.position))
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

    //Die!
    IEnumerator DieLonesomeInSpace(Vector3 direction)
    {
        PlayerUnitsController.Instance.UnselectSelectedPlayerUnits();
        SetMoveDestination(direction * 5f, 2f);
        yield return new WaitForSeconds(2f);
        SwitchUnitState(UnitState.Dead);
    }

    Node CalculateRecoilTarget(int recoilAmount, Vector3 recoilDirection)
    {
        //Set an offset, so the ray doesnt accicentally hits a Tile 
        Vector3 offset = currentNode.transform.up * 0.05f;

        Node recoilNode = currentNode;

        //Checks node after node in the recoil direction until we reach the recoilAmount defined on the weapon
        for (int i = 0; i < recoilAmount; i++)
        {
            RaycastHit[] hits = Physics.RaycastAll(recoilNode.transform.position + offset, recoilDirection, 1.5f).OrderBy(h => h.distance).ToArray();
            for (int j = 0; j < hits.Length; j++)
            {
                RaycastHit hit = hits[j];

                //If we hit something that stops us, this is our target Node and we leave the loop
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Tile") ||
                    hit.collider.gameObject.tag == "Player" ||
                    hit.collider.gameObject.tag == "Enemy")
                {
                    break;
                }

                //If we hit another nav data this is our current recoil node
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

            //If we hit no node, nor a stopping target, this has to be an edge. So we die!
            if (hits.Length == 0)
            {
                return null;
            }
        }

        //Highlight the recoil node and return it
        recoilNode.HighlightField(Color.red);
        return recoilNode;
    }

    IEnumerator MoveWithRecoil(Node recoilNode)
    {
        SwitchActionState(ActionState.Recoil);
        SetMoveDestination(recoilNode.transform.position, 0.5f);
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
                StartCoroutine(playerAnimation.TransitionToIdle());
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
                playerAnimation.PlayMoveAnimation();
                PlayerUnitsController.Instance.lineRenderer.gameObject.SetActive(false);
                break;
            case ActionState.Recoil:
                actionState = ActionState.Recoil;
                playerAnimation.PlayShootAnimation();
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

