using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public static bool NodeIsHitWithRaycast(Node targetNode, Unit unit)
    {
        Vector3 direction = targetNode.transform.position - unit.gunbarrel.position;
        Ray shootRay = new Ray(unit.gunbarrel.position, direction);

        Debug.DrawRay(shootRay.origin, shootRay.direction, Color.yellow, 2f);

        RaycastHit[] hits = Physics.RaycastAll(shootRay, unit.equippedRangeWeapon.range).OrderBy(h => h.distance).ToArray();
        for (int j = 0; j < hits.Length; j++)
        {
            RaycastHit hit = hits[j];

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Tile"))
            {
                return false;
            }

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NavData"))
            {
                if (hit.transform.tag == "Node")
                {
                    Node n = hit.collider.GetComponent<Node>();
                    if (n != null && n == targetNode)
                    {
                        return true;
                    }
                    return false;
                }
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
