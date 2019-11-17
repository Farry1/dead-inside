using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Node : MonoBehaviour
{
    public List<Node> edges = new List<Node>();

    public float movementCost = 1;
    public bool canBeEntered = true;
    public TextMesh stepCounterText;
    public Renderer surfaceIndicator;
    public Renderer visibleSurfaceIndicator;

    public Material[] materials;

    public Unit unitOnTile;


    private void Start()
    {
        stepCounterText.text = "0";
    }

    private void Update()
    {

    }

    public void IndicateNavigation(float stepsRequired, int maxSteps, Node v)
    {
        stepCounterText.text = stepsRequired.ToString();

        if (stepsRequired <= maxSteps && v.unitOnTile == false)
        {
            v.HighlightField(Color.green, true);
        }
        else
        {
            v.HighlightField(Color.cyan, false);
        }
    }

    public void HighlightField(Color color, bool fullTile)
    {
        surfaceIndicator.gameObject.SetActive(true);
        visibleSurfaceIndicator.gameObject.SetActive(true);
        visibleSurfaceIndicator.material = materials[0];

        if (fullTile)
        {
            visibleSurfaceIndicator.material = materials[1];
            visibleSurfaceIndicator.material.SetColor("_BaseColor", color);
        }
            

        visibleSurfaceIndicator.material.SetColor("_MainColor", color);
    }

    public void HideNavigationIndicator()
    {
        surfaceIndicator.gameObject.SetActive(false);
        visibleSurfaceIndicator.gameObject.SetActive(false);

    }

    public float DistanceTo(Vector3 position)
    {
        return Vector3.Distance(transform.position, position);
    }

    public static Vector3 GetOppositePlanarDirection(Node currentNode, Node targetNode)
    {
        //Origin and Destination of Ray
        Vector3 origin = currentNode.transform.position;
        Vector3 destination = targetNode.transform.position;

        //Get opposite Direction
        Vector3 direction = destination - origin;

        //Lock Ray on current plane
        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, targetNode.transform.up);

        planarDirection = planarDirection.normalized;

        //Lock rays in 45 Degree Angles
        planarDirection = LockAnglesOn45Degrees(planarDirection);

        return planarDirection;
    }

    public static Vector3 GetPlanarDirection(Node currentNode, Node targetNode)
    {
        //Origin and Destination of Ray
        Vector3 origin = currentNode.transform.position;
        Vector3 destination = targetNode.transform.position;

        //Get opposite Direction
        Vector3 direction = origin - destination;

        //Lock Ray on current plane
        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, targetNode.transform.up);

        planarDirection = planarDirection.normalized;

        //Lock rays in 45 Degree Angles
        planarDirection = LockAnglesOn45Degrees(planarDirection);

        //Debug.DrawRay(currentNode.transform.position, planarDirection, Color.cyan, 2f);

        return planarDirection;
    }

    public void PushAdjacentUnits(int steps)
    {
        Dictionary<Node, Vector3> nodesAndDirections = Get8NodesAndDirections();

        foreach (KeyValuePair<Node, Vector3> nodeWithDirection in nodesAndDirections)
        {
            Unit unit = nodeWithDirection.Key.unitOnTile;

            if (unit != null)
            {
                Debug.Log("Pushing " + unit.name);
                Node targetNode = unit.unitMovement.CalculatePushTarget(steps, nodeWithDirection.Value, null, true);

                if (targetNode != null)
                {

                    StartCoroutine(unit.unitMovement.MoveWithPush(steps, nodeWithDirection.Value));
                }
                else
                {
                    StartCoroutine(unit.unitMovement.DieLonesomeInSpace(nodeWithDirection.Value));
                }
            }
        }
    }

    Node previouseHoveredNode;
    Dictionary<Node, Vector3> previousCollectionOfAdjacentUnits = null;

    public void CalculateArealPush(int steps, Node hoveredNode)
    {

        Dictionary<Node, Vector3> nodesAndDirections = Get8NodesAndDirections();

        foreach (KeyValuePair<Node, Vector3> nodeWithDirection in nodesAndDirections)
        {
            Unit unit = nodeWithDirection.Key.unitOnTile;

            if (unit != null)
            {
                unit.unitMovement.CalculatePushTarget(steps, nodeWithDirection.Value, hoveredNode, false);
            }
        }

    }


    public Dictionary<Node, Vector3> Get8NodesAndDirections()
    {
        Dictionary<Node, Vector3> nodesAndDirections = new Dictionary<Node, Vector3>();

        for (int i = 0; i < 8; i++)
        {
            Vector3 rayDirection = Vector3.zero;
            switch (i)
            {
                //Rays On a Plane
                case 0:
                    rayDirection = transform.forward;
                    break;
                case 1:
                    rayDirection = transform.forward * -1;
                    break;
                case 2:
                    rayDirection = transform.right;
                    break;
                case 3:
                    rayDirection = transform.right * -1;
                    break;
                case 4:
                    rayDirection = (transform.forward + transform.right).normalized;
                    break;
                case 5:
                    rayDirection = (transform.forward * -1 + transform.right).normalized;
                    break;
                case 6:
                    rayDirection = (transform.forward * -1 + transform.right * -1).normalized;
                    break;
                case 7:
                    rayDirection = (transform.forward + transform.right * -1).normalized;
                    break;
            }

            Ray ray = new Ray(transform.localPosition, rayDirection);
            Debug.DrawRay(ray.origin, ray.direction * 1.5f, Color.yellow, 2f);


            RaycastHit[] hits = Physics.RaycastAll(ray, 1.5f).OrderBy(h => h.distance).ToArray();
            for (int j = 0; j < hits.Length; j++)
            {
                RaycastHit hit = hits[j];

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NavData"))
                {
                    if (hit.transform.tag == "Node")
                    {
                        Node n = hit.collider.GetComponent<Node>();
                        if (n != null)
                        {
                            nodesAndDirections.Add(n, rayDirection);
                            break;
                        }
                    }
                }
            }
        }

        return nodesAndDirections;
    }

    public Unit GetUnitOnTile()
    {
        return unitOnTile;
    }

    public static Vector3 LockAnglesOn45Degrees(Vector3 direction)
    {
        Vector3 lockedDirection = Vector3.zero;

        if (direction.x < -0.9)
        {
            lockedDirection.x = 1;
        }
        else if ((direction.x > -0.9) && (direction.x <= -0.3))
        {
            lockedDirection.x = 1f;
        }
        else if ((direction.x > -0.3) && (direction.x <= 0.3))
        {
            lockedDirection.x = 0f;
        }
        else if ((direction.x > 0.3) && (direction.x <= 0.9))
        {
            lockedDirection.x = -1f;
        }
        else if (direction.x > 0.9f)
        {
            lockedDirection.x = -1f;
        }

        if (direction.y < -0.9)
        {
            lockedDirection.y = 1;
        }
        else if ((direction.y > -0.9) && (direction.y <= -0.3))
        {
            lockedDirection.y = 1f;
        }
        else if ((direction.y > -0.3) && (direction.y <= 0.3))
        {
            lockedDirection.y = 0f;
        }
        else if ((direction.y > 0.3) && (direction.y <= 0.9))
        {
            lockedDirection.y = -1f;
        }
        else if (direction.y > 0.9f)
        {
            lockedDirection.y = -1f;
        }

        if (direction.z < -0.9)
        {
            lockedDirection.z = 1;
        }
        else if ((direction.z > -0.9) && (direction.z <= -0.3))
        {
            lockedDirection.z = 1f;
        }
        else if ((direction.z > -0.3) && (direction.z <= 0.3))
        {
            lockedDirection.z = 0f;
        }
        else if ((direction.z > 0.3) && (direction.z <= 0.9))
        {
            lockedDirection.z = -1f;
        }
        else if (direction.z > 0.9f)
        {
            lockedDirection.z = -1f;
        }
        return lockedDirection;
    }
}
