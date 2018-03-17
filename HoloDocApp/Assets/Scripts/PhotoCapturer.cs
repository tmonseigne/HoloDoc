using HoloToolkit.Unity;

using UnityEngine;
using UnityEngine.XR.WSA.WebCam;

public class PhotoCapturer : Singleton<PhotoCapturer> {

	[HideInInspector]
	public bool HasFoundCamera;

	private PhotoCapture    photoCaptureObject;
    private Resolution      cameraResolution;
    private Texture2D       photo;

    public delegate void OnPhotoTakenCallback(Texture2D photo, Resolution res);

	// Use this for initialization
	void Start () {
		if (WebCamTexture.devices.Length != 0) {
			this.cameraResolution = new Resolution {
				width = 1280,
				height = 720
			};

			this.photo = new Texture2D(this.cameraResolution.width, this.cameraResolution.height);
			this.HasFoundCamera = true;
        } 
		else {
			this.HasFoundCamera = false;
        }
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result) {
		// Shutdown our photo capture resource
		this.photoCaptureObject.Dispose();
		this.photoCaptureObject = null;
    }

    public void TakePhoto(OnPhotoTakenCallback callback) {
        // Create a PhotoCapture object
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
			this.photoCaptureObject = captureObject;
			CameraParameters cameraParameters = new CameraParameters {
				hologramOpacity = 0.0f,
				cameraResolutionWidth = this.cameraResolution.width,
				cameraResolutionHeight = this.cameraResolution.height,
				pixelFormat = CapturePixelFormat.BGRA32
			};

			// Activate the camera
			photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                // Take a picture
                photoCaptureObject.TakePhotoAsync((photoCaptureResult, photoCaptureFrame) => {
                    photoCaptureFrame.UploadImageDataToTexture(photo);

                    if (callback != null) {
                        callback.Invoke(photo, cameraResolution);
                    }

					this.photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
                });
            });
        });
    }
}
