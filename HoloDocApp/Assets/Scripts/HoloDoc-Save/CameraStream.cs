using UnityEngine;

using HoloToolkit.Unity;

public class CameraStream : Singleton<CameraStream>  {

	[Range(15, 60)]
	public int Framerate = 25;

	[Tooltip("You can provide a substitution frame if you do not have a camera or for testing purpose.")]
	public Texture2D SubstituableFrame;

	[Tooltip("If you have a camera and you still want to use a special substitution frame you can force the substitution.")]
	public bool Substitute = false;

	private WebCamTexture webCamTexture;
	private CameraFrame	  frame;

	// Getter CameraFrame
	public CameraFrame Frame {
		get { return this.frame; }
	}

	void Start() {
		if (SubstituableFrame && (Substitute || WebCamTexture.devices.Length == 0)) {
			Resolution resolution = new Resolution {
				width = SubstituableFrame.width,
				height = SubstituableFrame.height
			};

			frame = new CameraFrame(resolution, SubstituableFrame.GetPixels32());
		}
		else if (WebCamTexture.devices.Length > 0) {
			webCamTexture = new WebCamTexture
			{
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

	void Update() {
		if (webCamTexture) {
			webCamTexture.GetPixels32(this.frame.Data);
		}
	}
}