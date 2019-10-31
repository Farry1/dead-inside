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
        
        //Foreach through all enemies and create a copy of that list, so the Coroutine won't break; 
        foreach (EnemyUnit enemyUnit in enemyUnits.ToList())
        {
            if (enemyUnit != null)
            {
                Debug.Log("Do Evil Stuff");
                yield return (enemyUnit.MakeTurn());
                yield return new WaitForSeconds(1f);
            }
        }
        yield return new WaitForSeconds(1f);
     

        StageManager.Instance.EndEnemyTurn();
    }

    public void CheckForWin()
    {
        if (AllEnemiesDead())
        {
            StageUIController.Instance.WinPanel.SetActive(true);
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
