using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    private void Start()
    {
        stepCounterText.text = "0";
    }

    public void IndicateNavigation(int stepsRequired, int maxSteps)
    {
        stepCounterText.text = stepsRequired.ToString();

        if (stepsRequired <= maxSteps)
        {
            surfaceIndicator.gameObject.SetActive(true);
            surfaceIndicator.material.color = Color.green;

            visibleSurfaceIndicator.gameObject.SetActive(true);
            visibleSurfaceIndicator.material = materials[0];
        }
        else
        {
            surfaceIndicator.gameObject.SetActive(false);
            visibleSurfaceIndicator.gameObject.SetActive(false);
        }
    }

    public void HighlightField(Color color)
    {
        surfaceIndicator.gameObject.SetActive(true);
        visibleSurfaceIndicator.material.color = color;

        visibleSurfaceIndicator.gameObject.SetActive(true);
        visibleSurfaceIndicator.material = materials[1];
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

        Vector3 origin = currentNode.transform.position;
        Vector3 destination = targetNode.transform.position;




        Vector3 direction = origin - destination;

        Debug.DrawRay(origin, currentNode.transform.up, Color.blue, 2f);

        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, currentNode.transform.up);

        planarDirection = planarDirection.normalized;
        Color debugColor = Color.red;

        planarDirection = LockAnglesOn45Degrees(planarDirection);


        Debug.DrawRay(origin, planarDirection, debugColor, 5f);

        return direction;
    }

    public static Vector3 LockAnglesOn45Degrees(Vector3 direction)
    {

        Debug.Log(direction);

        Vector3 lockedDirection = Vector3.zero;

        if (direction.x <= -0.9)
        {
            lockedDirection += Vector3.right * -1;
        }



        //if (planarDirection.x <= -0.9)
        //{
        //    debugColor = Color.yellow;
        //    planarDirection = -currentNode.transform.right;

        //}
        //else if ((planarDirection.x > -0.9) && (planarDirection.x <= -0.3))
        //{

        //    if (planarDirection.z > 0)
        //    {
        //        planarDirection = currentNode.transform.right * -1 + currentNode.transform.forward;
        //        debugColor = Color.green;
        //    }
        //    else
        //    {
        //        //planarDirection = currentNode.transform.right * -1 + currentNode.transform.forward;
        //    }

        //}
        //else if ((planarDirection.x > -0.3) && (planarDirection.x <= 0.3))
        //{

        //    if (planarDirection.z > 0)
        //    {
        //        //planarDirection = new Vector3(0f, 0f, 1f);
        //    }
        //    else
        //    {
        //        //planarDirection = new Vector3(0f, 0f, -1f);
        //    }

        //}
        //else if ((planarDirection.x > 0.3) && (planarDirection.x <= 0.9))
        //{

        //    if (planarDirection.z > 0)
        //    {
        //        //planarDirection = new Vector3(0.7f, 0f, 0.7f);
        //    }
        //    else
        //    {
        //        //planarDirection = new Vector3(0.7f, 0f, -0.7f);
        //    }

        //}
        //else if (planarDirection.x > 0.9)
        //{

        //    //planarDirection = new Vector3(1f, 0f, 0f);
        //}

        // planarDirection = currentNode.transform.localRotation * planarDirection;


        return direction;
    }

}
