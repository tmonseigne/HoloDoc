using UnityEngine;

public class CameraStream : MonoBehaviour
{
	public static CameraStream Instance; // Singleton

	[Tooltip("You can provide a substitution frame if you do not have a camera or for testing purpose.")]
	public Texture2D substituableFrame;

	[Tooltip("If you have a camera and you still want to use a special substitution frame you can force the substitution.")]
	public bool substitute = false;

    private WebCamTexture webCamTexture;
    private CameraFrame frame;

	// Getter CameraFrame
	public CameraFrame Frame
    {
        get
        {
            return this.frame;
        }
    }

	void Start()
	{
		// Ensure singleton
		if (Instance)
		{
			DestroyImmediate(this);
		}

        if (substituableFrame && (substitute || WebCamTexture.devices.Length == 0))
        {
			Resolution resolution =  new Resolution
			{
				width = substituableFrame.width,
				height = substituableFrame.height
			};

			frame = new CameraFrame(resolution, substituableFrame.GetPixels32());
		}
		else if (WebCamTexture.devices.Length > 0)
        {
			webCamTexture = new WebCamTexture();
			webCamTexture.Play();

			Resolution resolution = new Resolution
			{
				width = webCamTexture.width,
				height = webCamTexture.height
			};

			frame = new CameraFrame(resolution, new Color32[resolution.width * resolution.height]);
		}
		else
        {
            throw new System.Exception("No camera/substitution frame found.");
        }

		Instance = this;
	}

	void Update()
	{
		if (webCamTexture)
		{
			webCamTexture.GetPixels32(this.frame.Data);
		}
	}
}
