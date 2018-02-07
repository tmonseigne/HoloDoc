using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HoloToolkit.Unity.InputModule;

public class PhotoButtonEventHandler : MonoBehaviour, IFocusable, IInputClickHandler
{
    private AudioSource audioSource;
    private AudioClip audioClip;

    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
        audioClip = Resources.Load<AudioClip>("Sounds/Button_Press");
    }

    public void OnFocusEnter()
    { }

    public void OnFocusExit()
    { }

    public void OnInputClicked(InputClickedEventData data)
    {
        audioSource.PlayOneShot(audioClip, 1);
        this.GetComponent<PhotoScript>().TakePhoto();
    }
}