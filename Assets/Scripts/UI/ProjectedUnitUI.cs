using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectedUnitUI : MonoBehaviour
{
    public Transform UIHolder;
    public GameObject UIPrefab;

    private GameObject projectedUnitUI;
    private GameObject healthbar;
    private Image pointer;
    [SerializeField] private GameObject singleHealthBar;

    Health health;
    Unit unit;

    private Text currentHealthText;
    private Text maxHealthText;
    // private Image healthBar;
    private Text apLeftText;

    float maxHealth = 1;

    private void OnEnable()
    {
        StageManager.OnWon += HideProjectedUnitUI;
        StageManager.OnLost += HideProjectedUnitUI;
        StageUIController.OnIngameMenuOpened += HideProjectedUnitUI;
    }

    private void OnDisable()
    {
        StageManager.OnWon -= HideProjectedUnitUI;
        StageManager.OnLost -= HideProjectedUnitUI;
        StageUIController.OnIngameMenuOpened -= HideProjectedUnitUI;
    }

    private void Start()
    {
        health = GetComponent<Health>();
        unit = GetComponent<Unit>();

        projectedUnitUI = Instantiate(UIPrefab, StageUIController.Instance.projectedUnitUIsContainer.transform);
        healthbar = projectedUnitUI.transform.Find("Healthbar").gameObject;
        currentHealthText = projectedUnitUI.transform.Find("Health").Find("CurrentHealth").GetComponent<Text>();
        maxHealthText = projectedUnitUI.transform.Find("Health").Find("MaxHealth").GetComponent<Text>();
        apLeftText = projectedUnitUI.transform.Find("AP").Find("APLeft").GetComponent<Text>();
        pointer = projectedUnitUI.transform.Find("Pointer").GetComponent<Image>();

        unit.projectedUnitUI = projectedUnitUI;
    }

    void Update()
    {
        bool isVisible = false;

        RaycastHit hit;
        Ray ray = new Ray(transform.position, Camera.main.transform.position - transform.position);
        Debug.DrawRay(ray.origin, ray.direction, Color.yellow);

        if (Physics.Raycast(transform.position, Camera.main.transform.position - transform.position, out hit))
        {
            if (hit.collider.tag == "MainCamera")
            {
                if (StageManager.Instance.stageState != StageManager.StageState.IngameMenu &&
                    StageManager.Instance.stageState != StageManager.StageState.Lost &&
                    StageManager.Instance.stageState != StageManager.StageState.Won)
                    isVisible = true;
            }
        }

        projectedUnitUI.SetActive(isVisible);

        Vector3 newPos = Camera.main.WorldToScreenPoint(UIHolder.transform.position);
        projectedUnitUI.transform.position = newPos + new Vector3(135, 45, 0);

        UpdateUI();
    }

    //Todo: Call this only at certain events
    private void UpdateUI()
    {
        //Todo: Make this performant!

        Image[] healthBarSingleItems = healthbar.GetComponentsInChildren<Image>();

        foreach (Image img in healthBarSingleItems)
        {
            Destroy(img.gameObject);
        }

        for (int i = 0; i < health.health; i++)
        {
            GameObject sh = Instantiate(singleHealthBar, healthbar.transform);

            EnemyUnit enemyUnit = GetComponent<EnemyUnit>();
            if (enemyUnit != null)
            {
                Color color = Color.red;

                if (unit.unitState != Unit.UnitState.Selected)
                {
                    color.a = 0.75f;
                }
                else
                {
                    color.a = 1;
                }

                sh.GetComponent<Image>().color = color;
                pointer.color = color;
            }

            PlayerUnit playerUnit = GetComponent<PlayerUnit>();
            if (enemyUnit != null)
            {

            }
        }

        if (unit.unitState != Unit.UnitState.Selected)
        {
            projectedUnitUI.transform.localScale = Vector3.one * 0.6f;
            projectedUnitUI.GetComponent<RectTransform>().pivot = new Vector3(0.8f, 0.8f);
        }
        else
        {
            projectedUnitUI.transform.localScale = Vector3.one;
            projectedUnitUI.GetComponent<RectTransform>().pivot = new Vector3(0.5f, 0.5f);
        }

    }

    void HideProjectedUnitUI()
    {
        projectedUnitUI.SetActive(false);
    }
}
