﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(UnitAnimation))]
[RequireComponent(typeof(UnitMovement))]
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
    public Node currentNode = null;
    [HideInInspector] public List<Node> currentPath = null;


    [Header("Gun Transforms")]
    public Transform gunbarrel;
    public Transform raycastTarget;

    [Header("Actions")]
    [Tooltip("Actions that don't come with any weapon or gear, like moving")]
    public List<Action> actions = new List<Action>();

    [Header("Character Specifics")]
    //Todo: Put this stuff into a Scriptable Object Config Thingy
    public int maxActionPoints = 2;
    [HideInInspector] public int currentActionPoints = 2;
    public int maxSteps = 1;
    public bool ignorePushback;
    public Weapon equippedRangeWeapon;
    public Sprite characterPortrait;  

    protected UnitAnimation unitAnimation;
    public UnitMovement unitMovement;

    public GameObject projectedUnitUI;

   

    //Events
    public delegate void UnitSelection();
    public static event UnitSelection OnUnitSelected;
    public static event UnitSelection OnUnitDeselected;


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
        unitMovement = GetComponent<UnitMovement>();
        healthController = GetComponent<Health>();
        currentActionPoints = maxActionPoints;
        //startPosition = target = transform.position;

        if (currentNode == null)
        {
            currentNode = Calculations.FindClosestNodeToTransform(transform);
            currentNode.unitOnTile = this;
        }

        //transform.position = currentNode.transform.position;
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
        //t += Time.deltaTime / timeToReachTarget;
        //transform.position = Vector3.Lerp(startPosition, target, t);
    }

    //public void SetMoveDestination(Vector3 destination, float time)
    //{
    //    t = 0;
    //    startPosition = transform.position;
    //    timeToReachTarget = time;
    //    target = destination;
    //}

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
        OnUnitSelected();
    }

    public virtual void OnUnselect()
    {
        OnUnitDeselected();
    }

    public virtual void Die()
    {
        currentNode.unitOnTile = null;
        unitMovement.DestroyCollisionWarning();
        unitMovement.DestroyZeroGravityWarning();
        Destroy(projectedUnitUI);
        Destroy(this.gameObject);
    }

    public void Hit(Node targetNode, Vector3 hitDirection, int damage)
    {
        healthController.Damage(damage);

        if (targetNode != null)
        {
            unitMovement.SetMoveDestination(targetNode.transform.position, 0.45f);
            currentNode.unitOnTile = null;
            currentNode = targetNode;
            targetNode.unitOnTile = this;
        }
        else
        {
            StartCoroutine(unitMovement.DieLonesomeInSpace(hitDirection));
        }
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
