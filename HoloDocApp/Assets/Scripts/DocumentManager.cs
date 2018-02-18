using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class DocumentManager : MonoBehaviour, IFocusable, IInputClickHandler, IInputHandler
{
	public Color color;
	public Color focusColor;
	public Color clickedColor;

	private Material material;
	private Texture2D photoTex;
	private CameraFrame photo;

	// Use this for initialization
	void Start()
	{
		material = this.GetComponent<Renderer>().material;
		this.SetColor(color);
	}

	public void SetColor(Color color)
	{
		material.SetColor("_OutlineColor", color);
	}

	public void OnFocusEnter()
	{
		//this.SetColor(focusColor);
	}

	public void OnFocusExit()
	{
		//this.SetColor(color);
	}

	public void OnInputClicked(InputClickedEventData eventData)
	{
		//PhotoTaker.Instance.Photo(OnPhotoTaken);
		//this.SetColor(clickedColor);
	}

	public void OnInputUp(InputEventData eventData)
	{
		LinkManager.Instance.OnLinkEnded(this.gameObject);
	}

	public void OnInputDown(InputEventData eventData)
	{
		LinkManager.Instance.OnLinkStarted(this.gameObject);
	}

	private void OnPhotoTaken(CameraFrame result)
	{
		photo = result;
		// Debug lines (Only used to draw result on a quad)
		photoTex.SetPixels32(photo.Data);
		photoTex.Apply(true);

        RequestLauncher.Instance.CreateNewDocument(photoTex);
	}
}
