using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class NodeGenerator : MonoBehaviour
{
    public void GetEdges()
    {
        Node[] nodes = GetComponentsInChildren<Node>();
        int connectionCounter = 0;

        foreach (Node node in nodes)
        {
            for (int i = 0; i < 16; i++)
            {
                Vector3 rayDirection = node.transform.forward;
                switch (i)
                {
                    //Rays On a Plane
                    case 0:
                        break;
                    case 1:
                        rayDirection = node.transform.forward * -1;
                        break;
                    case 2:
                        rayDirection = node.transform.right;
                        break;
                    case 3:
                        rayDirection = node.transform.right * -1;
                        break;
                    case 4:
                        rayDirection = (node.transform.forward + node.transform.right).normalized;
                        break;
                    case 5:
                        rayDirection = (node.transform.forward + node.transform.right).normalized * -1;
                        break;
                    case 6:
                        rayDirection = (node.transform.forward + node.transform.right * -1).normalized;
                        break;
                    case 7:
                        rayDirection = (node.transform.forward + node.transform.right * -1).normalized * -1;
                        break;


                    //Diagonal Rays
                    case 8:
                        rayDirection = (node.transform.forward + node.transform.up).normalized;
                        break;
                    case 9:
                        rayDirection = (node.transform.forward + node.transform.up * -1).normalized;
                        break;
                    case 10:
                        rayDirection = (node.transform.right + node.transform.up).normalized;
                        break;
                    case 11:
                        rayDirection = (node.transform.right + node.transform.up * -1).normalized;
                        break;
                    case 12:
                        rayDirection = (node.transform.forward * -1 + node.transform.up).normalized;
                        break;
                    case 13:
                        rayDirection = (node.transform.forward * -1 + node.transform.up * -1).normalized;
                        break;
                    case 14:
                        rayDirection = (node.transform.right * -1 + node.transform.up).normalized;
                        break;
                    case 15:
                        rayDirection = (node.transform.right * -1 + node.transform.up * -1).normalized;
                        break;
                }

                float castDistance = 1.5f;
                Color rayColor = Color.red;

                if (i > 7)
                {
                    castDistance = 0.7f;
                    rayColor = Color.blue;
                }

                Ray ray = new Ray(node.transform.localPosition, rayDirection);
                Debug.DrawRay(ray.origin, ray.direction * castDistance, rayColor, 2f);


                RaycastHit[] hits = Physics.RaycastAll(ray, castDistance).OrderBy(h => h.distance).ToArray();
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
                                node.edges.Add(n);
                                connectionCounter++;
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (nodes.Count() > 0)
            Debug.Log("Node Generation complete. Found " + nodes.Count() + " Nodes. Created " + connectionCounter + " connections.");
        else
            Debug.Log("No nodes found");
    }

    public void DeleteAllConnections()
    {
        Node[] nodes = GetComponentsInChildren<Node>();

        foreach (Node node in nodes)
        {
            node.edges.Clear();
        }

        Debug.Log(nodes.Count() + " Nodes deleted.");
    }
}
