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
        GameMusic = FMODUnity.RuntimeManager.CreateInstance("event:/GameMusic");
        GameMusic.start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void WonAudio()
    {
        Debug.Log("Play Win Audio");
    }

    void LostAudio()
    {
        GameMusic.setParameterByName("GameOver", 1f);
    }

    public void ShootSound()
    {

    }

    public void StopGameMusic()
    {
        GameMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}
