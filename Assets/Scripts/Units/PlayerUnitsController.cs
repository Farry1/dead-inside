using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerUnitsController : MonoBehaviour
{
    [HideInInspector] public List<PlayerUnit> units = new List<PlayerUnit>();
    [HideInInspector] public PlayerUnit selectedPlayerUnit = null;

    public LineRenderer lineRenderer;

    private static PlayerUnitsController _instance;
    public static PlayerUnitsController Instance { get { return _instance; } }

    bool isPlayerTurn;

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
                        selectedPlayerUnit.PrecalculatePathTo(hitNode);
                        selectedPlayerUnit.Shoot(hitNode);
                       
                    }                    
                }
                break;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit leftClickHit;
            if (Physics.Raycast(mousePositionRay, out leftClickHit))
            {
                //A Selectable is clicked
                ISelectable selectable = leftClickHit.collider.GetComponentInParent<ISelectable>();
                if (selectable != null)
                {
                    selectable.OnSelect();
                }
            }
            // TileMap.Instance.selectedUnit.Move();
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

        // Draw Debug Path
        if (selectedPlayerUnit != null &&
            selectedPlayerUnit.actionState == PlayerUnit.PlayerActionState.MoveSelection &&
            selectedPlayerUnit.currentPath != null &&
            selectedPlayerUnit.actionState != PlayerUnit.PlayerActionState.Moving)
        {
            int currentNode = 0;

            while (currentNode < selectedPlayerUnit.currentPath.Count - 1)
            {
                Vector3 start = selectedPlayerUnit.currentPath[currentNode].transform.position;
                Vector3 end = selectedPlayerUnit.currentPath[currentNode + 1].transform.position;
                Debug.DrawLine(start, end, Color.cyan);
                currentNode++;
            }

            DrawPath();
        }


    }


    void DrawPath()
    {
        List<Transform> transforms = new List<Transform>();

        foreach (Node v in selectedPlayerUnit.currentPath)
        {
            transforms.Add(v.transform);
        }

        int seg = transforms.Count();
        Vector3[] vP = new Vector3[transforms.Count()];

        for (int i = 0; i < transforms.Count(); i++)
        {
            vP[i] = transforms[i].position;
        }
        for (int i = 0; i < seg; i++)
        {
            float t = i / (float)seg;
            lineRenderer.positionCount = seg;
            lineRenderer.SetPositions(vP);
        }

    }

    public void UnselectSelectedUnits()
    {
        StageUIController.Instance.SetPlayerActionContainer(false);

        StageUIController.Instance.ClearPlayerActions();

        PlayerUnit[] selectedUnits = units.Where(u => u.unitState == PlayerUnit.UnitState.Selected).ToArray();
        foreach (PlayerUnit unit in selectedUnits)
        {
            unit.unitState = PlayerUnit.UnitState.Idle;
        }

        selectedPlayerUnit = null;
        TileMap.Instance.Clear();
    }

    public PlayerUnit GetSelectedUnit()
    {
        PlayerUnit[] selectedUnits = units.Where(u => u.unitState == PlayerUnit.UnitState.Selected).ToArray();
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
        UnselectSelectedUnits();
    }
}
