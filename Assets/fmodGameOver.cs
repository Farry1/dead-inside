using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fmodGameOver : MonoBehaviour
{
    FMOD.Studio.EventInstance ZeroG_m1_BreakIntoTheScene;

    // Start is called before the first frame update
    private void Start()
    {
        ZeroG_m1_BreakIntoTheScene = FMODUnity.RuntimeManager.CreateInstance("event:/ZeroG_m1_BreakIntoTheScene");
        ZeroG_m1_BreakIntoTheScene.start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
