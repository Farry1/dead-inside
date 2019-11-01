﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Dijkstra : MonoBehaviour
{
    // public List<Node> currentPath;

    private Node[] graph;

    private static Dijkstra _instance;
    public static Dijkstra Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        graph = FindObjectsOfType<Node>();
    }

    // Update is called once per frame
    void Update()
    {

    }


    float CostToEnterNode(Node node, Node targetNode)
    {
        float cost = node.movementCost;

        if (node.canBeEntered && node.unitOnTile == null && NoUnitOnSlants(node))
        {
            if (Vector3.Distance(node.transform.position, targetNode.transform.position) > 1.1f)
            {
                cost += 0.001f;
            }
        }
        else
            cost = Mathf.Infinity;


        return cost;
    }

    private bool NoUnitOnSlants(Node v)
    {
        for (int i = 0; i < 4; i++)
        {
            //Check for 4 directions that are set here
            Vector3 rayDirection = Vector3.zero;
            switch (i)
            {
                case 0:
                    rayDirection = (v.transform.forward + v.transform.up).normalized;
                    break;
                case 1:
                    rayDirection = (v.transform.right + v.transform.up).normalized;
                    break;
                case 2:
                    rayDirection = (v.transform.right * -1 + v.transform.up).normalized;
                    break;
                case 3:
                    rayDirection = (v.transform.forward * -1 + v.transform.up).normalized;
                    break;
            }

            Ray ray = new Ray(v.transform.localPosition, rayDirection);
            //Debug.DrawRay(ray.origin, ray.direction * 0.7f, Color.magenta, 2f);

            RaycastHit[] hits = Physics.RaycastAll(ray, 0.8f).OrderBy(h => h.distance).ToArray();
            for (int j = 0; j < hits.Length; j++)
            {
                RaycastHit hit = hits[j];

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("NavData"))
                {
                    if (hit.transform.tag == "Node")
                    {
                        Node n = hit.collider.GetComponent<Node>();
                        if (n != null && n.unitOnTile != null)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;



    }

    public List<Node> GeneratePathTo(Node source, Node target, int maxSteps)
    {
        //if theres a unit on the target tile
        if (target.unitOnTile != null)
        {
            target = ValidateTarget(target, source);
        }

        Dictionary<Node, float> distance = new Dictionary<Node, float>();
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();

        List<Node> unvisitedNodes = new List<Node>();

        distance[source] = 0;
        previous[source] = null;

        //Initialize everything with infinite distance for now.
        //Also some nodes may not be reachable
        foreach (Node v in graph)
        {
            if (v != source)
            {
                distance[v] = Mathf.Infinity;
                previous[v] = null;
            }

            unvisitedNodes.Add(v);
        }


        while (unvisitedNodes.Count > 0)
        {
            Node u = null;

            foreach (Node possibleU in unvisitedNodes)
            {
                if (u == null || distance[possibleU] < distance[u])
                {
                    u = possibleU;
                }
            }

            if (u == target)
            {
                break; // Exit while loop
            }

            unvisitedNodes.Remove(u);

            foreach (Node v in u.edges)
            {
                //This calculates the cost for our costs set up in the Node instance. For real distance use Vector3.Distance().
                float alt = distance[u] + CostToEnterNode(v, u);
                if (alt < distance[v])
                {
                    distance[v] = alt;
                    previous[v] = u;
                }
            }
        }

        //Shortest route or no route at all

        if (previous[target] == null)
        {
            // No Route detected
        }

        List<Node> currentPath = new List<Node>();

        Node curr = target;

        while (previous[curr] != null)
        {
            currentPath.Add(curr);
            curr = previous[curr];
        }

        currentPath.Add(source);
        currentPath.Reverse();

        //If path exceeds max size, cut it. +1 because of source element in currentPath
        if (currentPath.Count() > maxSteps + 1)
        {
            currentPath.RemoveRange(maxSteps + 1, currentPath.Count - maxSteps - 1);
        }

        return currentPath;
    }


    Node ValidateTarget(Node source, Node target)
    {
        //Todo: Make this better. So that characters cannot clip into each other diagonally
        List<Node> validatedNodes = source.edges.Where(h => h.unitOnTile == null).ToList();
        return validatedNodes[0];
    }


    public void GetNodesInRange(Node source, int range)
    {
        Dictionary<Node, float> distance = new Dictionary<Node, float>();
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();

        List<Node> unvisitedNodes = new List<Node>();

        distance[source] = 0;
        previous[source] = null;

        //Initialize everything with infinite distance for now.
        //Also some nodes may not be reachable
        foreach (Node v in graph)
        {
            if (v != source)
            {
                distance[v] = Mathf.Infinity;
                previous[v] = null;
            }

            unvisitedNodes.Add(v);
        }

        while (unvisitedNodes.Count > 0)
        {
            Node u = null;

            //TODO: make sorting this faster
            foreach (Node possibleU in unvisitedNodes)
            {
                if (u == null || distance[possibleU] < distance[u])
                {
                    u = possibleU;
                }
            }

            unvisitedNodes.Remove(u);


            foreach (Node v in u.edges)
            {
                //This calculates the cost with costs value set up in the Node instance. For real distance use Vector3.Distance().
                float alt = distance[u] + CostToEnterNode(v, u);
                if (alt < distance[v])
                {
                    distance[v] = alt;
                    previous[v] = u;
                }

                v.IndicateNavigation(distance[v], range, v);
            }
        }
    }

    public void Clear()
    {
        foreach (Node v in graph)
        {
            v.HideNavigationIndicator();
        }
    }
}
