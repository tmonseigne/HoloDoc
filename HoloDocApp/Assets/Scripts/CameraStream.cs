using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using System.Linq;

public class CameraStream : MonoBehaviour
{
	public static CameraStream Instance; // Singleton

	[Tooltip("You can provide a substitution frame if you do not have a camera or for testing purpose.")]
	public Texture2D substituableFrame;

	[Tooltip("If you have a camera and you still want to use a special substitution frame you can force the substitution.")]
	public bool substitute = false;

    private WebCamTexture webCamTexture;
    private CameraFrame frame;
    private Resolution resolution;

	// Getter CameraFrame
	public CameraFrame Frame
    {
        get
        {
            return this.frame;
        }
    }

	// Getter Resolution
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

		Resolution frameResolution;
        if (substituableFrame && (substitute || WebCamTexture.devices.Length == 0))
        {
			frameResolution = new Resolution
			{
				width = substituableFrame.width,
				height = substituableFrame.height
			};
			frame = new CameraFrame(frameResolution, substituableFrame.GetPixels32());
        }
        else if (WebCamTexture.devices.Length > 0)
        {
            frameResolution = PhotoCapture.SupportedResolutions.OrderByDescending(res => res.width * res.height).First();
			webCamTexture = new WebCamTexture(frameResolution.width, frameResolution.height);
			webCamTexture.Play();
            frame = new CameraFrame(frameResolution, webCamTexture.GetPixels32());
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
		if (webCamTexture)
		{
			frame.Data = webCamTexture.GetPixels32();
		}
	}
}
