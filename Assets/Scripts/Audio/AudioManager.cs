using UnityEngine;
using System.Collections.Generic;




public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("AudioManager initialized (no audio)");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    public void PlaySound(string soundName)
    {
        
    }

    public void PlayPowerModeMusic()
    {
        
    }

    public void StopPowerModeMusic()
    {
        
    }

    public void StopMusic()
    {
        
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}