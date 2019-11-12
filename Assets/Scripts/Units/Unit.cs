using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    [HideInInspector] public UnitMovement unitMovement;
    [HideInInspector] public GameObject projectedUnitUI;

    public UnityEvent OnDie;



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

    protected void ShootProjectile(Node recoilTarget, Vector3 recoilDirection, Vector3 projectileTargetPosition, Node targetNode, Unit targetUnit, Vector3 hitDirection, Node targetPushbackNode)
    {
        GameObject shootFX = Instantiate(equippedRangeWeapon.shootFX, gunbarrel.position, gunbarrel.rotation);
        GameObject simulatedProjectile = Instantiate(equippedRangeWeapon.simulatedProjectile, gunbarrel.position, gunbarrel.rotation);
        GameObject simulatedProjectileTarget = Instantiate(equippedRangeWeapon.projectileTarget, projectileTargetPosition, Quaternion.identity);

        Projectile projectile = simulatedProjectile.GetComponent<Projectile>();
        projectile.target = simulatedProjectileTarget;
        projectile.damage = equippedRangeWeapon.damage;
        projectile.projectilePushAmount = equippedRangeWeapon.projectilePushAmount;
        projectile.projectileType = equippedRangeWeapon.projectileType;
        projectile.hitNode = targetNode;
        projectile.targetUnit = targetUnit;
        projectile.initialUnit = this;
        projectile.hitDirection = hitDirection;
        projectile.targetPushbackNode = targetPushbackNode;

        Vector3 projetileDirection = simulatedProjectileTarget.transform.position - simulatedProjectile.transform.position;

        Rigidbody simulatedProjectileRB = simulatedProjectile.GetComponent<Rigidbody>();
        simulatedProjectileRB.AddForce(equippedRangeWeapon.projectileSpeed * projetileDirection);

        //If we have a recoil target, move to that position
        if (recoilTarget != null)
        {
            StartCoroutine(unitMovement.MoveWithPush(equippedRangeWeapon.recoilAmount, recoilDirection));
        }

        //If not you fly over the edge and die in space
        else
        {
            StartCoroutine(unitMovement.DieLonesomeInSpace(recoilDirection));
        }        
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
      
    }

    public virtual void SwitchUnitState(UnitState state)
    {
        //unitState = state;
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
        OnDie.Invoke();
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

    public void Explode(int amount)
    {
        currentNode.PushAdjacentUnits(amount);
    }
}
