using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileMap : MonoBehaviour
{
    // public List<Node> currentPath;

    private Node[] graph;

    private static TileMap _instance;
    public static TileMap Instance { get { return _instance; } }

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
   

    void ChangeSelectedUnit()
    {

    }

    float CostToEnterNode(Node node, Node targetNode)
    {
        float cost = node.movementCost;

        if (node.canBeEntered)
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

    public List<Node> GeneratePathTo(Node source, Node target, int maxSteps)
    {
        PlayerUnitsController.Instance.selectedPlayerUnit.currentPath = null;

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

    public void Dijkstra(Node source, int maxSteps)
    {
        source = PlayerUnitsController.Instance.selectedPlayerUnit.currentNode;

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

                v.IndicateNavigation((int)distance[v], maxSteps);
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
