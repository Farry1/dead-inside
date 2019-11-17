﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        // SceneManager.UnloadScene(SceneManager.GetActiveScene().buildIndex);
    }



    public void QuitGame()
    {
        Application.Quit();
    }
}
