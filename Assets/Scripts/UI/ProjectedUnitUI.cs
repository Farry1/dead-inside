using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectedUnitUI : MonoBehaviour
{
    public Transform UIHolder;
    public GameObject UIPrefab;

    private GameObject projectedUnitUI;
    Health health;
    Unit unit;

    private Text currentHealthText;
    private Text maxHealthText;
    private Image healthBar;
    private Text apLeftText;

    float maxHealth = 1;



    private void Start()
    {
        health = GetComponent<Health>();
        unit = GetComponent<Unit>();
        

        projectedUnitUI = Instantiate(UIPrefab, StageUIController.Instance.projectedUnitUIsContainer.transform);
        healthBar = projectedUnitUI.transform.Find("Health").Find("HealthBar").GetComponent<Image>();
        currentHealthText = projectedUnitUI.transform.Find("Health").Find("CurrentHealth").GetComponent<Text>();
        maxHealthText = projectedUnitUI.transform.Find("Health").Find("MaxHealth").GetComponent<Text>();
        apLeftText = projectedUnitUI.transform.Find("AP").Find("APLeft").GetComponent<Text>();

        unit.projectedUnitUI = projectedUnitUI;

        Init();
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
                isVisible = true;
            }
        }

        projectedUnitUI.SetActive(isVisible);

        Vector3 newPos = Camera.main.WorldToScreenPoint(UIHolder.transform.position);
        projectedUnitUI.transform.position = newPos;

        projectedUnitUI.GetComponentInChildren<Text>().text = gameObject.name;

        UpdateUI();
    }

    private void Init()
    {
        maxHealth = health.health;
        Vector3 healthBarScale = new Vector3((float)health.health / maxHealth, 1, 1);
        healthBar.transform.localScale = healthBarScale;
        currentHealthText.text = health.health.ToString();
        maxHealthText.text = health.health.ToString();
        apLeftText.text = unit.currentActionPoints.ToString();
    }

    //Todo: Call this only at certain events
    private void UpdateUI()
    {
        Vector3 healthBarScale = new Vector3((float)health.health / maxHealth, 1, 1);
        healthBar.transform.localScale = healthBarScale;
        currentHealthText.text = health.health.ToString();
        apLeftText.text = unit.currentActionPoints.ToString();
    }
}
