using UnityEngine;

using HoloToolkit.Unity;

public class CameraStreamHoloLens : Singleton<CameraStreamHoloLens>  {

	[Range(15, 60)] public int Framerate = 25;

	private WebCamTexture webCamTexture;
	private CameraFrame	  frame;
	private Texture2D	  cameraTexture;

	// Getter CameraFrame
	public CameraFrame Frame {
		get { return this.frame; }
	}

	void Start() {
		if (WebCamTexture.devices.Length == 0) {
			cameraTexture = new Texture2D(Camera.main.pixelWidth, Camera.main.pixelHeight, TextureFormat.RGB24, false);

			Resolution resolution = new Resolution {
				width = Camera.main.pixelWidth,
				height = Camera.main.pixelHeight
			};

			frame = new CameraFrame(resolution, new Color32[resolution.width * resolution.height]);
		}
		else if (WebCamTexture.devices.Length > 0) {
			webCamTexture = new WebCamTexture {
				// This may help reduce lag in the application
				requestedFPS = Framerate
			};

			webCamTexture.Play();

			Resolution resolution = new Resolution {
				width = webCamTexture.width,
				height = webCamTexture.height
			};

			frame = new CameraFrame(resolution, new Color32[resolution.width * resolution.height]);
		}
		else {
			throw new System.Exception("No camera/substitution frame found.");
		}
	}

	void FixedUpdate() {
		if (webCamTexture) {
			webCamTexture.GetPixels32(this.frame.Data);
		}
		else {
			this.frame.Data = cameraTexture.GetPixels32();
		}
	}

	private void OnPostRender() {
		if (!webCamTexture)	{
			RenderTexture.active = Camera.main.targetTexture;
			cameraTexture.ReadPixels(Camera.main.pixelRect, 0, 0);
			cameraTexture.Apply();
			RenderTexture.active = null;
		}
	}
}