using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ButtonManager : MonoBehaviour, IFocusable, IInputClickHandler  {

	public Vector3 offsetScale = new Vector3(0.2f, 0, 0.2f);
	public GameObject GOToEnable, GOToDisable;

	private Vector3 initScale;

	// Use this for initialization
	void Start()
	{
		initScale = this.transform.localScale;
	}

	public void OnFocusEnter()
	{
		this.transform.localScale = initScale + offsetScale;
	}

	public void OnFocusExit()
	{
		this.transform.localScale = initScale;
	}

	public void OnInputClicked(InputClickedEventData eventData)
	{
		Debug.Log("OnInputClicked!");
		GOToEnable.SetActive(true);
		GOToDisable.SetActive(false);
	}
	
	
	// Update is called once per frame
	void Update () {
		
	}
}
