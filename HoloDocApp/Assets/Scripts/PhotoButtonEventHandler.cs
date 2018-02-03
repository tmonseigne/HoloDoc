using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HoloToolkit.Unity.InputModule;

public class PhotoButtonEventHandler : MonoBehaviour, IFocusable, IInputClickHandler {

    private AudioSource m_audio;

    void Start()
    {
        m_audio = this.GetComponent<AudioSource>();
        m_audio.clip = Resources.Load<AudioClip>("Sounds/Button_Press");
    }

    public void OnFocusEnter()
    { }

    public void OnFocusExit()
    {
        m_audio.Stop();
    }

    public void OnInputClicked(InputClickedEventData data)
    {
        
        m_audio.Play();
        this.GetComponent<PhotoScript>().TakePhoto();
    }
}
