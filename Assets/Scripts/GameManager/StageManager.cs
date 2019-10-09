using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public delegate void StateChanged();
    public static event StateChanged OnPlayerTurn;
    public static event StateChanged OnEnemyTurn;
    public static event StateChanged OnLost;
    public static event StateChanged OnWon;


    public enum StageState { PlayerTurn, EnemyTurn, Lost, Won };
    public StageState stageState;

    private static StageManager _instance;
    public static StageManager Instance { get { return _instance; } }

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


    // Start is called before the first frame update
    void Start()
    {
        InitScene();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReloadCurrentScene()
    {
        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void InitScene()
    {
        stageState = StageState.PlayerTurn;
        OnPlayerTurn();
    }

    public void EndPlayerTurn()
    {
        stageState = StageState.EnemyTurn;
        OnEnemyTurn();
    }

    public void EndEnemyTurn()
    {
        stageState = StageState.PlayerTurn;
        OnPlayerTurn();
    }

}
