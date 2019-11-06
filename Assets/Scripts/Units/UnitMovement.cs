using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitMovement : MonoBehaviour
{
    public GameObject zeroGravityWarningPrefab;
    private GameObject zeroGravityWarningInstance;

    public GameObject collisionWarningPrefab;
    private GameObject collisionWarningInstance;

    //Variables for linear movement over time    
    protected float t;
    protected Vector3 startPosition;
    protected Vector3 target;
    protected float timeToReachTarget;
    Unit unit;
    List<Node> _currentPath;
    List<Node> currentPath
    {
        get
        {
            return unit.currentPath;
        }
    }


    void OnEnable()
    {
        Unit.OnUnitSelected += DestroyZeroGravityWarning;
        Unit.OnUnitUnselected += DestroyZeroGravityWarning;
        PlayerUnit.OnActionStateNone += DestroyZeroGravityWarning;
        PlayerUnit.OnActionStateMovementPreparation += DestroyZeroGravityWarning;
        PlayerUnit.OnHoveredNodeChanged += DestroyZeroGravityWarning;
        PlayerUnit.OnNodeClicked += DestroyZeroGravityWarning;

        Unit.OnUnitSelected += DestroyCollisionWarning;
        Unit.OnUnitUnselected += DestroyCollisionWarning;
        PlayerUnit.OnActionStateNone += DestroyCollisionWarning;
        PlayerUnit.OnActionStateMovementPreparation += DestroyCollisionWarning;
        PlayerUnit.OnHoveredNodeChanged += DestroyCollisionWarning;
        PlayerUnit.OnNodeClicked += DestroyCollisionWarning;
    }


    void OnDisable()
    {
        Unit.OnUnitSelected -= DestroyZeroGravityWarning;
        Unit.OnUnitUnselected -= DestroyZeroGravityWarning;
        PlayerUnit.OnActionStateNone -= DestroyZeroGravityWarning;
        PlayerUnit.OnActionStateMovementPreparation -= DestroyZeroGravityWarning;
        PlayerUnit.OnHoveredNodeChanged -= DestroyZeroGravityWarning;
        PlayerUnit.OnNodeClicked -= DestroyZeroGravityWarning;

        Unit.OnUnitSelected -= DestroyCollisionWarning;
        Unit.OnUnitUnselected -= DestroyCollisionWarning;
        PlayerUnit.OnActionStateNone -= DestroyCollisionWarning;
        PlayerUnit.OnActionStateMovementPreparation -= DestroyCollisionWarning;
        PlayerUnit.OnHoveredNodeChanged -= DestroyCollisionWarning;
        PlayerUnit.OnNodeClicked -= DestroyCollisionWarning;
    }

    void Start()
    {
        unit = GetComponent<Unit>();
        startPosition = target = transform.position;
    }


    void Update()
    {
        t += Time.deltaTime / timeToReachTarget;
        transform.position = Vector3.Lerp(startPosition, target, t);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DestroyCollisionWarning();
        }
    }

    public void SetMoveDestination(Vector3 destination, float time)
    {
        t = 0;
        startPosition = transform.position;
        timeToReachTarget = time;
        target = destination;
    }

    public IEnumerator MoveCoroutine()
    {
        unit.SwitchActionState(Unit.ActionState.Moving);
        //As long as we have avalid path that has more than one entry left, Move to that Position
        while (currentPath != null && currentPath.Count() > 1)
        {
            //Wait until movement to next tile is done
            yield return StartCoroutine(MoveNextTile());
        }
        unit.SwitchActionState(Unit.ActionState.None);
    }


    //Move To the next Tile
    IEnumerator MoveNextTile()
    {
        //Set the Rotation Vectors        
        Vector3 direction = currentPath[1].transform.position - currentPath[0].transform.position;
        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, currentPath[1].transform.up);

        //Remove the old first node and move us to that position
        currentPath.RemoveAt(0);



        //Set Destination and move over time, also set Rotation to the rotation of target Node
        SetMoveDestination(currentPath[0].transform.position, 0.45f);

        //Set Rotation
        transform.rotation = currentPath[0].transform.rotation;
        transform.rotation = Quaternion.LookRotation(planarDirection, currentPath[0].transform.up);


        //Reset Nodes
        unit.currentNode.unitOnTile = null;
        unit.currentNode = currentPath[0];
        unit.currentNode.unitOnTile = unit;


        if (currentPath.Count == 1)
        {
            //Next thingy in path would be our ultimate goal and we're standing on it. So make the path null to end this
            unit.currentPath = null;
            unit.SwitchActionState(Unit.ActionState.None);
        }

        yield return new WaitForSeconds(0.5f);
    }

    Node previousHoveredNode = null;

    public Node CalculatePushTarget(int steps, Vector3 direction, Node hoveredNode)
    {
        //A negative pushAmount should inverse the push direction;
        if (steps < 0)
            direction = -direction;
        steps = Mathf.Abs(steps);

        //Set an offset, so the ray doesnt accicentally hits a Tile 
        Vector3 offset = unit.currentNode.transform.up * 0.05f;

        Node pushNode = unit.currentNode;

        if (steps == 0)
        {
            return pushNode;
        }

        //Checks node after node in the recoil direction until we reach the recoilAmount defined on the weapon
        for (int i = 0; i < steps; i++)
        {
            RaycastHit[] hits = Physics.RaycastAll(pushNode.transform.position + offset, direction, 1.5f).OrderBy(h => h.distance).ToArray();
            for (int j = 0; j < hits.Length; j++)
            {
                RaycastHit hit = hits[j];

                //If we hit something that stops us, this is our target Node and we leave the loop

                if (hit.collider.gameObject.tag == "Player" ||
                    hit.collider.gameObject.tag == "Enemy" ||
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("Tile"))
                {
                    UpdateCollisionWarning(hoveredNode, pushNode, direction);
                    previousHoveredNode = hoveredNode;
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
                            pushNode = n;
                            break;
                        }
                    }
                }
            }

            //If we hit no node, nor a stopping target, this has to be an edge. So we die!
            if (hits.Length == 0)
            {
                UpdateZeroGravityWarning(pushNode, direction);

                pushNode.HighlightField(Color.red, true);

                previousHoveredNode = hoveredNode;
                return null;
            }
        }

        previousHoveredNode = hoveredNode;
        return pushNode;
    }

    private void UpdateZeroGravityWarning(Node pushNode, Vector3 direction)
    {
        if (zeroGravityWarningInstance == null)
        {
            Debug.Log(this.gameObject.name + " Instantiating Gravity Warning!");
            zeroGravityWarningInstance = Instantiate(zeroGravityWarningPrefab, pushNode.transform.localPosition, pushNode.transform.localRotation);
        }

        if (zeroGravityWarningInstance != null)
        {
            zeroGravityWarningInstance.transform.localPosition = pushNode.transform.localPosition;
            zeroGravityWarningInstance.transform.Find("ArrowContainer").rotation = Quaternion.LookRotation(direction);
        }
    }

    private void UpdateCollisionWarning(Node hoveredNode, Node pushNode, Vector3 pushDirection)
    {
        if (hoveredNode != null && hoveredNode != previousHoveredNode && collisionWarningInstance == null)
        {
            Debug.Log(gameObject.name + " Collision Warning Instantiate" + hoveredNode);
            collisionWarningInstance = Instantiate(collisionWarningPrefab, pushNode.transform.localPosition + pushDirection * 0.45f, pushNode.transform.localRotation);
        }
    }

    Node previousVisitedNode = null;
    public IEnumerator MoveWithPush(int steps, Vector3 direction)
    {
        //A negative pushAmount should inverse the push direction;
        if (steps < 0)
            direction = -direction;
        steps = Mathf.Abs(steps);

        Node pushNode = unit.currentNode;

        //If no steps, quit the Coroutine and don't move
        if (steps == 0)
        {
            yield return 0;
        }

        //Set an offset, so the ray doesnt accicentally hits a Tile 
        Vector3 offset = unit.currentNode.transform.up * 0.05f;

        for (int i = 0; i < steps; i++)
        {
            //Check for movement int the specified direction
            RaycastHit[] hits = Physics.RaycastAll(unit.currentNode.transform.position + offset, direction, 1.5f).OrderBy(h => h.distance).ToArray();
            for (int j = 0; j < hits.Length; j++)
            {
                RaycastHit hit = hits[j];

                //If we hit something that stops us, this is our target Node and we leave the loop
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Tile"))
                {
                    //Apply Collision Damage to this unit
                    if (previousVisitedNode == pushNode)
                        break;

                    unit.healthController.Damage(Constants.COLLISION_DAMAGE);
                    previousVisitedNode = pushNode;
                    break;
                }
                //If we hit another Player Or Enemy Unit, get that unit and apply damage to both: this unit and the hit unit. Then leave the loop.
                else if (
                hit.collider.gameObject.tag == "Player" ||
                hit.collider.gameObject.tag == "Enemy")
                {
                    if (previousVisitedNode == pushNode)
                        break;

                    Unit hitUnit = hit.collider.GetComponent<Unit>();

                    if (hitUnit != null)
                    {
                        Debug.Log("Apply Collision Damage to " + hitUnit.name);
                        hitUnit.healthController.Damage(Constants.COLLISION_DAMAGE);
                    }

                    unit.healthController.Damage(Constants.COLLISION_DAMAGE);
                    previousVisitedNode = pushNode;
                    break;
                }

                //If we hit another nav data this is our node to move to
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NavData"))
                {
                    if (hit.transform.tag == "Node")
                    {
                        Node n = hit.collider.GetComponent<Node>();
                        if (n != null)
                        {
                            unit.SwitchActionState(Unit.ActionState.Recoil);
                            pushNode = n;
                            unit.currentNode.unitOnTile = null;
                            unit.currentNode = pushNode;
                            unit.currentNode.unitOnTile = unit;

                            //Apply Movement and wait for a moment
                            SetMoveDestination(pushNode.transform.position, 0.25f);
                            yield return new WaitForSeconds(0.25f);
                            break;
                        }
                    }
                }
            }
        }

        unit.SwitchActionState(Unit.ActionState.None);
        yield return new WaitForSeconds(0.25f);
    }

    public void CalculateArealPush(int steps, Vector3 direction, Node hoveredNode)
    {
        Debug.Log("Calculate Areal Push");
    }

    public void MoveArealPush(int steps, Node hoveredNode)
    {
        Debug.Log("Move Areal Push");
    }


    public IEnumerator DieLonesomeInSpace(Vector3 direction)
    {
        //Todo: Change this to some shot and then die animation. But for now just normal recoil state.
        unit.SwitchActionState(Unit.ActionState.Recoil);
        SetMoveDestination(direction * 5f, 2f);
        yield return new WaitForSeconds(2f);
        unit.SwitchUnitState(Unit.UnitState.Dead);
    }

    public void DestroyZeroGravityWarning()
    {
        if (zeroGravityWarningInstance != null)
        {
            Destroy(zeroGravityWarningInstance);
            zeroGravityWarningInstance = null;
        }
        previousHoveredNode = null;
    }

    public void DestroyCollisionWarning()
    {
        if (collisionWarningInstance != null)
        {
            Destroy(collisionWarningInstance);
            collisionWarningInstance = null;
        }
        previousHoveredNode = null;
    }

    public Quaternion PlanarRotation(Vector3 direction)
    {
        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, unit.currentNode.transform.up);
        return Quaternion.LookRotation(planarDirection, unit.currentNode.transform.up);
    }

    public void ResetPreviousStoredValues()
    {
        previousHoveredNode = null;
        previousVisitedNode = null;
    }
}
