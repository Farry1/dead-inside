﻿using System.Collections;
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
        AudioManager.Instance.StopGameMusic();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopGameMusic();
        }
    }

    public void InitScene()
    {
        if (stageState != StageState.Lost || stageState != StageState.Won)
        {
            SwitchStageState(StageState.PlayerTurn);
            OnPlayerTurn();
        }
    }

    public void EndPlayerTurn()
    {
        if (stageState != StageState.Lost || stageState != StageState.Won)
        {
            SwitchStageState(StageState.EnemyTurn);
        }
    }

    public void EndEnemyTurn()
    {
        if (stageState == StageState.EnemyTurn)
        {
            SwitchStageState(StageState.PlayerTurn);
        }
    }

    public void SwitchStageState(StageState state)
    {
        switch (state)
        {
            case StageState.Lost:
                OnLost();
                break;

            case StageState.Won:
                OnWon();
                break;

            case StageState.PlayerTurn:
                OnPlayerTurn();
                break;

            case StageState.EnemyTurn:
                OnEnemyTurn();
                break;
        }

        stageState = state;
    }

}
