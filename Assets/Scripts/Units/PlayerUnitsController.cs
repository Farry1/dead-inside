using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUnitsController : MonoBehaviour
{
    public List<PlayerUnit> units = new List<PlayerUnit>();
    [HideInInspector] public PlayerUnit selectedPlayerUnit = null;
    [HideInInspector] public LineRenderer lineRenderer;

    private static PlayerUnitsController _instance;
    public static PlayerUnitsController Instance { get { return _instance; } }

    bool isPlayerTurn;

    Node previousHoveredNode = null;

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


    void OnEnable()
    {
        StageManager.OnPlayerTurn += InitPlayerTurn;
        StageManager.OnEnemyTurn += IsEnemyTurn;
    }


    void OnDisable()
    {
        StageManager.OnPlayerTurn -= InitPlayerTurn;
        StageManager.OnEnemyTurn -= IsEnemyTurn;
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerUnit[] unitsArray = FindObjectsOfType<PlayerUnit>();
        foreach (PlayerUnit u in unitsArray)
        {
            units.Add(u);
        }

        StageUIController.Instance.CreateUnitPanel();
        lineRenderer = GetComponentInChildren<LineRenderer>();
        lineRenderer.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: this can be improved. I'm sure.
        // Get Mouse Mouse Position
        Ray mousePositionRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Cast a Ray through all Objects to shoot through Indicator Plane of Nodes. If Ray hits "Tile"-Layer, get the coresponding node
        RaycastHit[] hits = Physics.RaycastAll(mousePositionRay, Mathf.Infinity).OrderBy(h => h.distance).ToArray();

        SetMovement(mousePositionRay);

        if (!IsPointerOverUIObject())
        {
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];

                //If mouse is over a Tile
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Tile"))
                {
                    //Check which side of the tile was hit
                    Transform objectHit = hit.transform;
                    Ray objectSideRay = new Ray(objectHit.transform.position, hit.normal);
                    //Debug.DrawRay(objectSideRay.origin, objectSideRay.direction * 1.5f, Color.red, 5f);

                    // Get the node that is in that direction
                    RaycastHit objectSideHit;
                    if (Physics.Raycast(objectSideRay, out objectSideHit, 1f))
                    {
                        Node hitNode = objectSideHit.collider.gameObject.GetComponent<Node>();
                        if (hitNode != null && selectedPlayerUnit != null)
                        {
                            previousHoveredNode = hitNode;
                            selectedPlayerUnit.PrecalculatePathTo(hitNode);
                            selectedPlayerUnit.RangeAttack(hitNode);
                        }
                    }
                    break;
                }
            }
        }


        void SetMovement(Ray navigateRay)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (selectedPlayerUnit != null &&
                    selectedPlayerUnit.currentPath != null)
                {
                    RaycastHit navigateToHit;
                    if (Physics.Raycast(navigateRay, out navigateToHit))
                    {
                        selectedPlayerUnit.Move();
                    }
                }
            }
        }
    }

    //Is Mouse Touching UI?
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public void UnselectSelectedPlayerUnits()
    {
        if (selectedPlayerUnit != null)
        {
            selectedPlayerUnit.SwitchActionState(Unit.ActionState.None);
            selectedPlayerUnit.OnUnselect();
        }

        lineRenderer.gameObject.SetActive(false);
        StageUIController.Instance.SetPlayerActionContainer(false);
        StageUIController.Instance.ClearPlayerActionsPanel();

        PlayerUnit[] selectedUnits = units.Where(u => u.unitState == Unit.UnitState.Selected).ToArray();
        foreach (PlayerUnit unit in selectedUnits)
        {
            unit.unitState = Unit.UnitState.Idle;
        }

        selectedPlayerUnit = null;
        Dijkstra.Instance.Clear();
    }

    public PlayerUnit GetSelectedUnit()
    {
        PlayerUnit[] selectedUnits = units.Where(u => u.unitState == Unit.UnitState.Selected).ToArray();
        return selectedUnits[0];
    }



    public void SelectUnit(PlayerUnit unit)
    {
        selectedPlayerUnit = unit;
    }

    void InitPlayerTurn()
    {
        isPlayerTurn = true;
    }

    void IsEnemyTurn()
    {
        isPlayerTurn = false;
        UnselectSelectedPlayerUnits();
    }
}
