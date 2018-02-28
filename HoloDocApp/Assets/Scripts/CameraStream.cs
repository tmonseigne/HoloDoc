using UnityEngine;

public class CameraStream : MonoBehaviour {

	public static CameraStream Instance; // Singleton

	[Range(15, 60)]
	public int Framerate = 25;

	[Tooltip("You can provide a substitution frame if you do not have a camera or for testing purpose.")]
	public Texture2D substituableFrame;

	[Tooltip("If you have a camera and you still want to use a special substitution frame you can force the substitution.")]
	public bool substitute = false;

	private WebCamTexture _webCamTexture;
	private CameraFrame _frame;

	// Getter CameraFrame
	public CameraFrame Frame {
		get { return this._frame; }
	}

	void Start() {
		// Ensure singleton
		if (Instance) {
			DestroyImmediate(this);
		}

		if (substituableFrame && (substitute || WebCamTexture.devices.Length == 0)) {
			Resolution resolution = new Resolution {
				width = substituableFrame.width,
				height = substituableFrame.height
			};

			_frame = new CameraFrame(resolution, substituableFrame.GetPixels32());
		}
		else if (WebCamTexture.devices.Length > 0) {
			_webCamTexture = new WebCamTexture
			{
				// This may help reduce lag in the application
				requestedFPS = Framerate
			};

			_webCamTexture.Play();

			Resolution resolution = new Resolution {
				width = _webCamTexture.width,
				height = _webCamTexture.height
			};

			_frame = new CameraFrame(resolution, new Color32[resolution.width * resolution.height]);
		}
		else {
			throw new System.Exception("No camera/substitution frame found.");
		}

		Instance = this;
	}

	void Update() {
		if (_webCamTexture) {
			_webCamTexture.GetPixels32(this._frame.Data);
		}
	}
}