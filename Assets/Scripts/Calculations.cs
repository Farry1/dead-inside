using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calculations : MonoBehaviour
{
    public static bool UnitIsHitWithRaycast(Unit unit, Vector3 start)
    {
        Vector3 direction = unit.raycastTarget.position - start;
        Ray shootRay = new Ray(start, direction * 10);

        Debug.DrawRay(shootRay.origin, shootRay.direction, Color.red, 2f);

        RaycastHit shootHit;

        if (Physics.SphereCast(shootRay, 0.1f, out shootHit))
        {
            if (shootHit.collider.gameObject.GetComponent<Unit>() == unit)
            {
                return true;
            }
        }

        return false;
    }

    public static Node FindClosestNodeToTransform(Transform transformToCheck)
    {
        Node[] nodes = FindObjectsOfType<Node>();

        Node closestNode = null;
        float distance = Mathf.Infinity;
        Vector3 position = transformToCheck.position;
        foreach (Node n in nodes)
        {
            Vector3 diff = n.transform.position - position;
            float currDistance = diff.sqrMagnitude;
            if (currDistance < distance)
            {
                closestNode = n;
                distance = currDistance;
            }
        }
        return closestNode;
    }
}
