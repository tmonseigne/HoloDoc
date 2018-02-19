using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class DocumentManager : MonoBehaviour, IFocusable, IInputClickHandler, IInputHandler
{
	public GameObject documentInformationsPrefab;

	public Color color;
	public Color focusColor;
	public Color clickedColor;

	private Material material;
	private Texture2D photoTex;
	private CameraFrame photo;
	private DocumentMesh mesh;

	private DocumentProperties props;
	private GameObject documentInformations;


	// Use this for initialization
	void Start()
	{
		material = this.GetComponent<Renderer>().material;
		props = this.GetComponent<DocumentProperties>();
		mesh = this.GetComponent<DocumentMesh>();
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
		if (props.photographied)
		{
			if (documentInformations == null)
			{
				Debug.Log(mesh.centroid);
				documentInformations = Instantiate(documentInformationsPrefab, new Vector3(mesh.centroid.x, mesh.centroid.y, mesh.centroid.z - 0.2f), this.transform.rotation);
			}
			documentInformations.GetComponent<InformationManager>().UpdateInformations(this.props);
		}
		else
		{
			props.photographied = true;
			//PhotoTaker.Instance.Photo(OnPhotoTaken);
		}
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

        RequestLauncher.Instance.DetectDocuments(photoTex, null);
	}
}
