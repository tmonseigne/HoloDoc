using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HoloToolkit.Unity.InputModule;
using UnityEngine.SceneManagement;

public class EnterButton : MonoBehaviour, IFocusable, IInputClickHandler
{

    void Start()
    {
    }

    public void OnFocusEnter()
    { }

    public void OnFocusExit()
    {
    }

    public void OnInputClicked(InputClickedEventData data)
    {
        Debug.Log("Enter");
        SceneManager.LoadScene("HoloDoc");
    }
}
