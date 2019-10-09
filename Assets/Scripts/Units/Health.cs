using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public GameObject healthIconPrefab;
    public GameObject healthContainer;
    public int health;
    Unit unit;
    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<Unit>();   
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
    }

    public void Damage(int amount)
    {
        health -= amount;
        UpdateUI();
        CheckLifeSigns();
    }

    public void Add(int amount)
    {
        health += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        int childs = healthContainer.transform.childCount;
        for (int i = childs - 1; i > 0; i--)
        {
            GameObject.Destroy(healthContainer.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < health; i++)
        {
            GameObject health = Instantiate(healthIconPrefab, healthContainer.transform);
            health.transform.position = new Vector3(
                health.transform.position.x,
                health.transform.position.y,
                health.transform.position.z + (i * 0.15f));
        }
    }

    void CheckLifeSigns()
    {
        if (health <= 0)
        {
            unit.SwitchUnitState(Unit.UnitState.Dead);
        }
    }
}
