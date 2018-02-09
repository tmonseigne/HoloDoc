using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using System.Linq;

public class CameraStream : MonoBehaviour
{
	public static CameraStream Instance;

	[Tooltip("You can provide a substitution frame if you do not have a camera or for testing purpose.")]
	public Texture2D substituableFrame;

	[Tooltip("If you have a camera and you still want to use a special substitution frame you can force the substitution.")]
	public bool substitute = false;

	private WebCamTexture cameraFrame;
	private Frame frame;
	private Resolution resolution;

	public Frame Frame
	{
		get
		{
			return this.frame;
		}
	}

	public Resolution Resolution
	{
		get
		{
			return this.resolution;
		}
	}

	void Start()
	{
		// Ensure singleton
		if (Instance)
		{
			DestroyImmediate(this);
		}

		// If no substituable frame is provided we can not force substitution
		if (substituableFrame == null)
		{
			substitute = false;
		}

		Resolution frameResolution = new Resolution();
		WebCamDevice[] devices = WebCamTexture.devices;
		if (substitute || (devices.Length == 0 && substituableFrame))
		{
			frameResolution.width = substituableFrame.width;
			frameResolution.height = substituableFrame.height;
			frame = new Frame(frameResolution, substituableFrame.GetPixels32());
		}
		else if (devices.Length > 0)
		{
			frameResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
			cameraFrame = new WebCamTexture(frameResolution.width, frameResolution.height);
			cameraFrame.Play();
			frame = new Frame(frameResolution, cameraFrame.GetPixels32());
		}
		else
		{
			throw new System.Exception("No camera/substitution frame found.");
		}

		this.resolution = frameResolution;
		Instance = this;

	}

	void Update()
	{
		if (cameraFrame != null)
		{
			frame.Data = cameraFrame.GetPixels32();
		}
	}


}
