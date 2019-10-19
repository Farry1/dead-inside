﻿using System.Collections;
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

    public Unit unitOnTile;


    private void Start()
    {
        stepCounterText.text = "0";
    }

    private void Update()
    {

    }

    public void IndicateNavigation(int stepsRequired, int maxSteps, Node v)
    {        
        stepCounterText.text = stepsRequired.ToString();

        if (stepsRequired <= maxSteps && v.unitOnTile == false)
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

        visibleSurfaceIndicator.gameObject.SetActive(true);
        visibleSurfaceIndicator.material = materials[1];
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
        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, currentNode.transform.up);

        planarDirection = planarDirection.normalized;

        //Lock rays in 45 Degree Angles
        planarDirection = LockAnglesOn45Degrees(planarDirection);
        return planarDirection;
    }

    public Unit GetUnitOnTile()
    {
        return unitOnTile;
    }

    public static Vector3 LockAnglesOn45Degrees(Vector3 direction)
    {
        Vector3 lockedDirection = direction;

        if (direction.x < -0.9)
        {
            lockedDirection.x = 1;
        }
        else if ((direction.x > -0.9) && (direction.x <= -0.3))
        {
            lockedDirection.x = 0.7f;
        }
        else if ((direction.x > -0.3) && (direction.x <= 0.3))
        {
            lockedDirection.x = 0f;
        }
        else if ((direction.x > 0.3) && (direction.x <= 0.9))
        {
            lockedDirection.x = -0.7f;
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
            lockedDirection.y = 0.7f;
        }
        else if ((direction.y > -0.3) && (direction.y <= 0.3))
        {
            lockedDirection.y = 0f;
        }
        else if ((direction.y > 0.3) && (direction.y <= 0.9))
        {
            lockedDirection.y = -0.7f;
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
            lockedDirection.z = 0.7f;
        }
        else if ((direction.z > -0.3) && (direction.z <= 0.3))
        {
            lockedDirection.z = 0f;
        }
        else if ((direction.z > 0.3) && (direction.z <= 0.9))
        {
            lockedDirection.z = -0.7f;
        }
        else if (direction.z > 0.9f)
        {
            lockedDirection.z = -1f;
        }
        return lockedDirection;
    }
}
