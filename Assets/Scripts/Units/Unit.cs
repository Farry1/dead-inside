using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Unit : MonoBehaviour, ISelectable
{
    public enum UnitState { Idle, Selected, Dead };
    public UnitState unitState = UnitState.Idle;

    public enum ActionState { MovePreparation, PreparingRangeAttack, None, Moving }
    public ActionState actionState;

    public List<Action> actions = new List<Action>();

    protected Health healthController;

    public int maxActionPoints = 2;
    protected int currentActionPoints = 2;
    public int maxSteps = 1;
    public int health = 3;
    public Weapon equippedRangeWeapon;


    [HideInInspector] public Node currentNode = null;
    [HideInInspector] public List<Node> currentPath = null;

    protected float t;
    protected Vector3 startPosition;
    protected Vector3 target;
    protected float timeToReachTarget;


    void OnEnable()
    {
        StageManager.OnPlayerTurn += OnPlayerTurn;
        StageManager.OnEnemyTurn += OnEnemyTurn;
    }


    void OnDisable()
    {
        StageManager.OnPlayerTurn -= OnPlayerTurn;
        StageManager.OnEnemyTurn -= OnEnemyTurn;
    }


    // Start is called before the first frame update
    protected virtual void Start()
    {
        healthController = GetComponent<Health>();
        currentActionPoints = maxActionPoints;
        startPosition = target = transform.position;

        if (currentNode == null)
        {
            currentNode = FindClosestNode();
            currentNode.unitOnTile = this;
        }

        transform.position = currentNode.transform.position;
        FindAttachedActions();
    }

    private void FindAttachedActions()
    {
        if (equippedRangeWeapon != null)
        {
            foreach (Action weaponAction in equippedRangeWeapon.attachedActions)
            {
                actions.Add(weaponAction);
            }
        }
    }

    Node FindClosestNode()
    {
        Node[] nodes = FindObjectsOfType<Node>();

        Node closestNode = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
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

    // Update is called once per frame
    protected virtual void Update()
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

    public virtual void SwitchUnitState(UnitState state)
    {
        unitState = state;
    }


    protected void OnMouseDown()
    {
        OnSelect();
    }

    public virtual void OnSelect()
    {

    }

    protected virtual void OnPlayerTurn()
    {
        currentActionPoints = maxActionPoints;
    }

    protected virtual void OnEnemyTurn()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {

    }

    public virtual void SwitchActionState(ActionState a)
    {

    }
}
