using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicScript : MonoBehaviour
{
    public AudioSource fonMusic;
    public AudioClip clickFx;
    public Sprite playOn, playOff;  
    private bool isOn;

    private void Start()
    {
        isOn = true;
    }
    public void _switchMusic()
    {
        if (isOn)
        {
            fonMusic.mute = true;
            GetComponent<Image>().sprite = playOff;
            isOn = false;
        }
        else
        {
            fonMusic.mute = false;
            GetComponent<Image>().sprite = playOn;
            isOn = true;
        }            
    }   
    public void ClickSound()
    {
        GetComponent<AudioSource>().PlayOneShot(clickFx);
    }
}
