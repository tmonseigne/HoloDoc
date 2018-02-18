using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class DocumentManager : MonoBehaviour, IFocusable, IInputClickHandler
{
	public Color color;
	public Color focusColor;
	public Color clickedColor;

	private GameObject quad;

	private Material material;
	private Texture2D photoTex;
	private CameraFrame photo;

	// Use this for initialization
	void Start()
	{

		// Debug lines (Only used to draw result on a quad)
		Resolution frameResolution = CameraStream.Instance.Resolution;
		photoTex = new Texture2D(frameResolution.width, frameResolution.height);
		quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		quad.transform.position = new Vector3(0, 0, 10);


		material = this.GetComponent<Renderer>().material;
	}

	public void OnFocusEnter()
	{
		material.SetColor("_OutlineColor", focusColor);
	}

	public void OnFocusExit()
	{
		material.SetColor("_OutlineColor", color);
	}

	public void OnInputClicked(InputClickedEventData eventData)
	{
		PhotoTaker.Instance.Photo(OnPhotoTaken);
		material.SetColor("_OutlineColor", clickedColor);
	}

	private void OnPhotoTaken(CameraFrame result)
	{
		photo = result;

		// Debug lines (Only used to draw result on a quad)
		photoTex.SetPixels32(photo.Data);
		photoTex.Apply(true);
		Renderer qR = quad.GetComponent<Renderer>() as Renderer;
		qR.material.mainTexture = photoTex;

        RequestLauncher.Instance.DetectDocuments(photoTex, null);
	}
}
