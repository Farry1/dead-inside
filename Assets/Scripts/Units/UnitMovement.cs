using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitMovement : MonoBehaviour
{
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

    void Start()
    {
        unit = GetComponent<Unit>();
        startPosition = target = transform.position;
    }


    void Update()
    {
        t += Time.deltaTime / timeToReachTarget;
        transform.position = Vector3.Lerp(startPosition, target, t);
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
        //Set the Rotation        
        transform.rotation = PlanarRotation(currentPath[1].transform.position - currentPath[0].transform.position);

        //Remove the old first node and move us to that position
        currentPath.RemoveAt(0);

        //Set Destination and move over time, also set Rotation to the rotation of target Node
        SetMoveDestination(currentPath[0].transform.position, 0.45f);


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

    public Node CalculateRecoilTarget(int recoilAmount, Vector3 recoilDirection)
    {
        
        //Set an offset, so the ray doesnt accicentally hits a Tile 
        Vector3 offset = unit.currentNode.transform.up * 0.05f;

        Node recoilNode = unit.currentNode;

        if (recoilAmount == 0)
        {
            return recoilNode;
        }

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

        return recoilNode;
    }


    public Quaternion PlanarRotation(Vector3 direction)
    {
        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, unit.currentNode.transform.up);
        return Quaternion.LookRotation(planarDirection, unit.currentNode.transform.up);
    }
}
