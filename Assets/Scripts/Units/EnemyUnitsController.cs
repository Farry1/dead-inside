using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyUnitsController : MonoBehaviour
{
    public List<EnemyUnit> enemyUnits = new List<EnemyUnit>();

    private static EnemyUnitsController _instance;
    public static EnemyUnitsController Instance { get { return _instance; } }

    void OnEnable()
    {
        StageManager.OnEnemyTurn += InitEnemyTurn;
    }


    void OnDisable()
    {
        StageManager.OnEnemyTurn -= InitEnemyTurn;
    }


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

    void Start()
    {
        EnemyUnit[] unitsArray = FindObjectsOfType<EnemyUnit>();
        foreach (EnemyUnit u in unitsArray)
        {
            enemyUnits.Add(u);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void InitEnemyTurn()
    {
        StartCoroutine(DoEnemyActions());
    }

    IEnumerator DoEnemyActions()
    {
        foreach (EnemyUnit enemyUnit in enemyUnits)
        {
            Debug.Log("Do Evil Stuff");
            yield return new WaitForSeconds(1);
        }

        StageManager.Instance.EndEnemyTurn();
    }

    public void CheckForWin()
    {
        if (AllEnemiesDead())
        {
            StageUIController.Instance.GameOverScreen.SetActive(true);
        }
    }

    private bool AllEnemiesDead()
    {
        if (enemyUnits.Count == 0)
        {
            return true;
        }
        else
            return false;
    }
}
