using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(UnitAnimation))]
public class Unit : MonoBehaviour, ISelectable
{
    //Unit and Action States
    public enum UnitState { Idle, Selected, Dead };
    public UnitState unitState = UnitState.Idle;

    public enum ActionState { MovePreparation, PreparingRangeAttack, None, Moving, Recoil }
    public ActionState actionState;

    //Reference to Health Controller
    [HideInInspector] public Health healthController;

    //Nodes for Pathfinding
    [HideInInspector] public Node currentNode = null;
    [HideInInspector] public List<Node> currentPath = null;


    [Header("Gun Transforms")]
    public Transform gunbarrel;
    public Transform raycastTarget;

    [Header("Actions")]
    [Tooltip("Actions that don't come with any weapon or gear, like moving")]
    public List<Action> actions = new List<Action>();

    [Header("Character Specifics")]
    public int maxActionPoints = 2;
    protected int currentActionPoints = 2;
    public int maxSteps = 1;
    public Weapon equippedRangeWeapon;

    //Variables for linear movement over time    
    protected float t;
    protected Vector3 startPosition;
    protected Vector3 target;
    protected float timeToReachTarget;

    protected UnitAnimation unitAnimation;

    void OnEnable()
    {
        //Subscribe to events
        StageManager.OnPlayerTurn += OnPlayerTurn;
        StageManager.OnEnemyTurn += OnEnemyTurn;
    }


    void OnDisable()
    {
        //Unsubscribe events
        StageManager.OnPlayerTurn -= OnPlayerTurn;
        StageManager.OnEnemyTurn -= OnEnemyTurn;
    }


    protected virtual void Start()
    {
        unitAnimation = GetComponent<UnitAnimation>();
        healthController = GetComponent<Health>();
        currentActionPoints = maxActionPoints;
        startPosition = target = transform.position;

        if (currentNode == null)
        {
            currentNode = Calculations.FindClosestNodeToTransform(transform);
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

    public virtual void Die()
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
