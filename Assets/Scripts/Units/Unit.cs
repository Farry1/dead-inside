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

    protected bool isPlayerTurn;

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

    protected virtual void Update()
    {
        bool rightMouseUp = Input.GetMouseButtonUp(1);
        bool leftMouseUp = Input.GetMouseButtonUp(0);

        switch (unitState)
        {
            case UnitState.Idle:
                break;

            case UnitState.Selected:

                switch (actionState)
                {
                    case ActionState.None:
                        if (rightMouseUp)
                            PlayerUnitsController.Instance.UnselectUnits();
                        break;
                }

                break;
        }
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

        if (isPlayerTurn)
        {
            PlayerUnitsController.Instance.UnselectUnits();
            PlayerUnitsController.Instance.SelectUnit(this);
            SwitchUnitState(UnitState.Selected);

            StageUIController.Instance.selectedUnitInformationContainer.SetActive(true);
            if (characterPortrait != null)
                StageUIController.Instance.selectedUnitImage.sprite = characterPortrait;
            else
                Debug.LogError("No character portrait set!");

            StageUIController.Instance.selectedUnitMoveRange.text = maxSteps.ToString();
            StageUIController.Instance.selectedUnitName.text = gameObject.name.ToString();


            if (PlayerUnitsController.Instance.selectedUnit.tag == "Player")
            {
                StageUIController.Instance.SetPlayerActionContainer(true);
                StageUIController.Instance.CreatePlayerActionMenu(actions);
            }
            else
            {
                StageUIController.Instance.SetPlayerActionContainer(false);
            }

        }
    }

    public virtual void OnUnselect()
    {
        OnUnitDeselected();

        StageUIController.Instance.SetPlayerActionContainer(false);
        StageUIController.Instance.selectedUnitInformationContainer.SetActive(false);
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
        isPlayerTurn = true;
    }

    protected virtual void OnEnemyTurn()
    {
        isPlayerTurn = false;
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
