using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance { get { return _instance; } }
    FMOD.Studio.EventInstance GameMusic;

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

    private void OnEnable()
    {
        StageManager.OnWon += WonAudio;
        StageManager.OnLost += LostAudio;
    }

    private void OnDisable()
    {
        StageManager.OnWon -= WonAudio;
        StageManager.OnLost -= LostAudio;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameMusic = FMODUnity.RuntimeManager.CreateInstance("event:/ZeroG_m1_BreakIntoTheScene");
        GameMusic.start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void WonAudio()
    {
        GameMusic.setParameterByName("Victory", 1);
    }

    void LostAudio()
    {
        GameMusic.setParameterByName("Death", 1);
    }
    
    public void ShootSound()
    {
        Debug.Log("shothere");
    }
}
