using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HoloToolkit.Unity.InputModule;

public class PhotoButtonEventHandler : MonoBehaviour, IFocusable, IInputClickHandler
{
    private AudioSource m_audioSource;
    private AudioClip m_audioClip;

    void Start()
    {
        m_audioSource = this.GetComponent<AudioSource>();
        m_audioClip = Resources.Load<AudioClip>("Sounds/Button_Press");
    }

    public void OnFocusEnter()
    { }

    public void OnFocusExit()
    { }

    public void OnInputClicked(InputClickedEventData data)
    {
        m_audioSource.PlayOneShot(m_audioClip, 1);

        this.GetComponent<PhotoScript>().TakePhoto();
    }
}